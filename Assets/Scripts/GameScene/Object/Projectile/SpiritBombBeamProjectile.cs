using GameScene.Dto.Projectile;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    /// <summary>
    /// Spirit Bomb 빔을 밑동·가운데·끝 세 조각의 sprite 로 그린다. 가운데 조각만
    /// <see cref="SpriteDrawMode.Tiled"/> 라서 size.x 에 길이를 넣으면 무늬가 늘어나지 않고
    /// 반복되고, size.y 가 그대로 굵기가 된다. StretchProjectile 의 팔에서 가져온 방식이다.
    ///
    /// 이 projection 은 prefab 이 없다. ProjectileSpawner.SpawnSpiritBombBeam 이 빈 GameObject 로
    /// 만들기 때문에 sprite 도 asset 이 아니라 런타임에 Texture2D 로 그린다. Tiled 는 sprite 의
    /// mesh 가 FullRect 일 때만 size 를 읽으므로 Sprite.Create 에 FullRect 를 명시한다.
    ///
    /// 수명은 ProjectileSpawner 가 Destroy(gameObject, dto.duration) 로 잡는다. 스스로 파괴하지
    /// 않는다. 서버는 damage tick 마다 짧은 projection 을 하나씩 보내므로, 이어지는 projection
    /// 들이 4초 빔의 그때그때의 목표 위치를 따라간다.
    /// </summary>
    public class SpiritBombBeamProjectile : MonoBehaviour, IProjectile
    {
        private const int CoreTextureWidth = 48;
        private const int CoreTextureHeight = 24;
        private const int CapTextureSize = 32;

        /// <summary>
        /// 서버가 width 를 정하지 않았을 때 쓰는 굵기, world 단위. LineRenderer 두 가닥으로
        /// 그리던 시절의 굵기와 맞춘 값이다 — 코일 반지름 0.12 의 위아래 합 0.24 에 가닥 굵기
        /// 0.065 를 더하면 0.3 이다. 서버가 보내는 값의 범위는 대략 0.25 ~ 1.0 이라 이 기본값은
        /// 그 범위의 아래쪽에 앉는다.
        /// </summary>
        private const float DefaultWidth = 0.3f;

        /// <summary>밑동과 끝의 크기, 굵기에 대한 배수. 끝이 더 커야 착탄으로 읽힌다.</summary>
        private const float BaseCapScale = 1.3f;
        private const float TipCapScale = 1.5f;

        private const float PulseSpeed = 9f;
        private const float PulseAmount = 0.08f;
        private const float VisibleLength = 0.01f;

        private const int CoreSortingOrder = 12;
        private const int CapSortingOrder = 13;

        private static readonly Color NatureColor = new Color(0.25f, 1f, 0.3f, 0.95f);
        private static readonly Color LightningColor = new Color(1f, 0.92f, 0.18f, 0.95f);

        private static Texture2D coreTexture;
        private static Texture2D capTexture;

        private ProjectileTarget startTarget;
        private ProjectileTarget endTarget;
        private Transform baseTransform;
        private Transform tipTransform;
        private SpriteRenderer coreRenderer;
        private SpriteRenderer tipRenderer;
        private Sprite coreSprite;
        private Sprite capSprite;
        private Vector3 lastStartPosition;
        private Vector3 lastEndPosition;
        private float beamWidth;
        private float startedAt;

        public void Init(ProjectileDto projectileDto)
        {
            startTarget = projectileDto.start;
            endTarget = projectileDto.end;
            startedAt = Time.time;
            TryUpdatePosition(startTarget, ref lastStartPosition);
            TryUpdatePosition(endTarget, ref lastEndPosition);

            // 0 이하는 서버가 굵기를 정하지 않았다는 뜻이다. width 를 아직 실어 보내지 않는
            // 서버와도 같이 돌아야 하므로 그때는 기본값으로 대체한다.
            beamWidth = projectileDto.width > 0f ? projectileDto.width : DefaultWidth;

            // pixelsPerUnit 을 굵기에서 거꾸로 계산해 sprite 의 원래 높이가 굵기와 정확히 같게
            // 만든다. 그래야 size.y 를 굵기로 줬을 때 tile 이 세로로 한 줄만 깔리고, 굵기가
            // tile 높이와 어긋나 위아래가 잘리거나 두 줄로 반복되는 일이 없다. tile 하나의
            // 가로 길이도 같은 비율로 따라 늘어나 무늬가 굵기에 비례한다.
            coreSprite = CreateSprite(GetCoreTexture(), new Vector2(0f, 0.5f), CoreTextureHeight / beamWidth);
            capSprite = CreateSprite(GetCapTexture(), new Vector2(0.5f, 0.5f), CapTextureSize / beamWidth);

            baseTransform = CreateRenderer("Base", capSprite, CapSortingOrder).transform;
            coreRenderer = CreateRenderer("Core", coreSprite, CoreSortingOrder);
            tipRenderer = CreateRenderer("Tip", capSprite, CapSortingOrder);
            tipTransform = tipRenderer.transform;

            coreRenderer.drawMode = SpriteDrawMode.Tiled;
            coreRenderer.tileMode = SpriteTileMode.Continuous;

            UpdateBeam();
        }

        // LateUpdate 라서 이번 프레임에 시전자와 목표가 움직인 결과가 이미 반영된 위치를 읽는다.
        private void LateUpdate()
        {
            if (startTarget == null || endTarget == null)
            {
                return;
            }

            UpdateBeam();
        }

        private void OnDestroy()
        {
            // texture 는 static 으로 돌려 쓰지만 sprite 는 굵기마다 pixelsPerUnit 이 달라
            // projection 마다 새로 만든다. 그래서 sprite 만 치운다.
            if (coreSprite != null)
            {
                Destroy(coreSprite);
            }
            if (capSprite != null)
            {
                Destroy(capSprite);
            }
        }

        private void UpdateBeam()
        {
            TryUpdatePosition(startTarget, ref lastStartPosition);
            TryUpdatePosition(endTarget, ref lastEndPosition);

            // 길이도 회전도 카메라 평면 기준이다. 카메라가 기울어져 있어 Vector3.Distance 는
            // 화면에 그려야 할 길이를 주지 않는다 (.agents/docs/scene-space.md).
            float length = ProjectileUtil.GetCameraPlaneLength(lastStartPosition, lastEndPosition);

            transform.position = lastStartPosition;
            transform.rotation = ProjectileUtil.GetRotation(lastStartPosition, lastEndPosition);

            // size 는 scale 보다 먼저 적용되므로 가운데 조각은 scale 1 을 유지해야 길이가 맞는다.
            coreRenderer.size = new Vector2(length, beamWidth);
            coreRenderer.enabled = length > VisibleLength;
            tipRenderer.enabled = length > VisibleLength;
            tipTransform.localPosition = new Vector3(length, 0f, 0f);

            // 이 빔은 네 번 때리는 동안 같은 자리에 걸려 있어서, 아무것도 움직이지 않으면 정지
            // 화면처럼 보인다. 밑동과 끝만 천천히 키웠다 줄인다.
            float pulse = 1f + Mathf.Sin((Time.time - startedAt) * PulseSpeed) * PulseAmount;
            baseTransform.localScale = Vector3.one * (BaseCapScale * pulse);
            tipTransform.localScale = Vector3.one * (TipCapScale * pulse);
        }

        private SpriteRenderer CreateRenderer(string pieceName, Sprite sprite, int sortingOrder)
        {
            GameObject pieceObject = new GameObject(pieceName);
            pieceObject.transform.SetParent(transform, false);

            // material 을 따로 만들지 않는다. SpriteRenderer 는 기본 sprite material 을 이미
            // 들고 나오고, 그쪽은 Shader.Find 와 달리 WebGL 빌드에서 빠질 일이 없다.
            SpriteRenderer pieceRenderer = pieceObject.AddComponent<SpriteRenderer>();
            pieceRenderer.sprite = sprite;
            pieceRenderer.sortingLayerName = "Default";
            pieceRenderer.sortingOrder = sortingOrder;
            return pieceRenderer;
        }

        private static Sprite CreateSprite(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
        {
            // FullRect 가 핵심이다. Sprite.Create 의 기본값인 Tight 는 투명한 가장자리를 깎아낸
            // mesh 를 만들고, 그러면 SpriteRenderer.size 가 무시돼 Tiled 가 동작하지 않는다.
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                pivot,
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "SpiritBombBeamPiece";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        /// <summary>
        /// 가운데 조각이 반복해서 까는 tile 한 장. 두 원소가 섞인 인상은 renderer 를 겹치는 대신
        /// 이 texture 한 장에 구웠다 — 겹치면 조각이 여섯 개가 되고 반투명한 알파가 두 번 곱해져
        /// 가운데가 오히려 탁해진다. 빔을 가로지르는 단면이 바깥은 NatureColor, 가운데 심지는
        /// LightningColor 라 한 장으로도 두 색이 같이 보인다.
        /// </summary>
        private static Texture2D GetCoreTexture()
        {
            if (coreTexture != null)
            {
                return coreTexture;
            }

            coreTexture = CreateTexture("SpiritBombBeamCore", CoreTextureWidth, CoreTextureHeight);
            Color[] pixels = new Color[CoreTextureWidth * CoreTextureHeight];

            for (int y = 0; y < CoreTextureHeight; y++)
            {
                float axisDistance = Mathf.Abs((y + 0.5f) / CoreTextureHeight * 2f - 1f);

                for (int x = 0; x < CoreTextureWidth; x++)
                {
                    // tile 이음매에서 잘록하고 가운데에서 부푸는 마디. sin 이 양끝에서 0 이라
                    // 옆 tile 과 이어 붙여도 굵기가 튀지 않는다.
                    float envelope = Mathf.Lerp(0.7f, 1f, Mathf.Sin((x + 0.5f) / CoreTextureWidth * Mathf.PI));
                    float distance = axisDistance / envelope;
                    pixels[y * CoreTextureWidth + x] = Blend(distance, 0.35f, 0.72f);
                }
            }

            coreTexture.SetPixels(pixels);
            coreTexture.Apply(false, true);
            return coreTexture;
        }

        /// <summary>밑동과 끝이 함께 쓰는 둥근 빛 한 장. 두 조각은 배수만 다르다.</summary>
        private static Texture2D GetCapTexture()
        {
            if (capTexture != null)
            {
                return capTexture;
            }

            capTexture = CreateTexture("SpiritBombBeamCap", CapTextureSize, CapTextureSize);
            Color[] pixels = new Color[CapTextureSize * CapTextureSize];
            Vector2 center = new Vector2((CapTextureSize - 1) * 0.5f, (CapTextureSize - 1) * 0.5f);
            float radius = CapTextureSize * 0.5f;

            for (int y = 0; y < CapTextureSize; y++)
            {
                for (int x = 0; x < CapTextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                    pixels[y * CapTextureSize + x] = Blend(distance, 0.25f, 0.2f);
                }
            }

            capTexture.SetPixels(pixels);
            capTexture.Apply(false, true);
            return capTexture;
        }

        /// <summary>
        /// 축에서 distance 만큼 떨어진 점의 색. coreEdge 안쪽은 LightningColor 심지고, 거기서
        /// 바깥으로 NatureColor 로 넘어가며, rimStart 부터 1 까지 알파가 빠진다. 알파가 0 인
        /// 자리에도 색은 남겨 둔다 — 검게 두면 bilinear 필터가 테두리를 어둡게 번지게 한다.
        /// </summary>
        private static Color Blend(float distance, float coreEdge, float rimStart)
        {
            if (distance >= 1f)
            {
                return new Color(NatureColor.r, NatureColor.g, NatureColor.b, 0f);
            }

            Color blended = Color.Lerp(
                LightningColor,
                NatureColor,
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(coreEdge, 1f, distance)));
            float alpha = blended.a * (1f - Mathf.SmoothStep(rimStart, 1f, distance));
            return new Color(blended.r, blended.g, blended.b, alpha);
        }

        private static Texture2D CreateTexture(string textureName, int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private static void TryUpdatePosition(ProjectileTarget target, ref Vector3 lastPosition)
        {
            switch (target)
            {
                case PositionProjectileTarget position:
                    lastPosition = position.ToVector3();
                    break;
                case ReferenceProjectileTarget reference:
                    var servedObject = ObjectContainer.Instance.FindById(reference.id);
                    if (servedObject != null)
                    {
                        lastPosition = servedObject.transform.position;
                    }
                    break;
            }
        }
    }
}
