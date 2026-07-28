using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// Fades a dying unit out and scatters a few crumbling fragments.
    /// <para>
    /// The real <see cref="ServedObject"/> is still removed on the server's schedule; everything
    /// here runs on a throwaway clone that copies the sprite only — no collider, hp bar, shadow
    /// or team indicator — so gameplay timing and hit feedback are untouched.
    /// </para>
    /// </summary>
    internal sealed class UnitDeathFadePresenter
    {
        private const float FadeDuration = 0.35f;
        private const float FragmentLifetime = 0.5f;
        private const int FragmentGridSize = 3;
        private const int MaxFragments = FragmentGridSize * FragmentGridSize;
        private const float FragmentRiseDistance = 0.35f;
        private const float FragmentSpreadDistance = 0.3f;
        private const float FragmentSpin = 90f;

        /// <summary>
        /// Creature object types, mirroring the creature rows of the object SFX catalog, plus
        /// MiniRock. Projectiles, fields, runes, drops, explosions and buildings are absent on
        /// purpose, and ThunderSpirit / ThunderBird keep the aerial lightning death of
        /// <c>AerialLightningDeathPresenter</c> instead of this one.
        /// </summary>
        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "AquaArcher",
            "BubbleSpirit",
            "ChickenCommando",
            "CloudDragon",
            "DimensionToad",
            "ElectricSlime",
            "EmberSpirit",
            "FireChildSpirit",
            "FireLordSpirit",
            "FireSlime",
            "FireSpirit",
            "FireTadpole",
            "LeafSlime",
            "LightningTadpole",
            "MagmaSpirit",
            "MiniRock",
            "PveVineWitch",
            "RockGolem",
            "RockMage",
            "RockSlime",
            "SeedSpirit",
            "StormRider",
            "TreeGolem",
            "VineSpirit",
            "WaterSlime",
            "WillOWisp",
            "WindSlime",
            "WindSpirit",
            "ZapMouse"
        };

        private readonly ServedObject servedObject;
        private bool played;

        private UnitDeathFadePresenter(ServedObject servedObject)
        {
            this.servedObject = servedObject;
            servedObject.OnDestroyed += Play;
        }

        public static void Attach(ServedObject servedObject, string objectType)
        {
            if (servedObject == null || objectType == null || !SupportedTypes.Contains(objectType))
            {
                return;
            }

            _ = new UnitDeathFadePresenter(servedObject);
        }

        private void Play()
        {
            if (played)
            {
                return;
            }

            played = true;
            servedObject.OnDestroyed -= Play;

            Transform actualTransform = servedObject.GetActualTransform();
            if (actualTransform == null)
            {
                return;
            }

            SpriteRenderer sourceRenderer = actualTransform.GetComponentInChildren<SpriteRenderer>();
            if (sourceRenderer == null || sourceRenderer.sprite == null)
            {
                return;
            }

            GameObject visual = CreateSpriteClone(sourceRenderer, $"{servedObject.name}DeathFade");
            SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>();
            FadeOut(visual, visualRenderer);
            ScatterFragments(sourceRenderer);
        }

        private static GameObject CreateSpriteClone(SpriteRenderer source, string name)
        {
            GameObject clone = new GameObject(name);
            clone.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            clone.transform.localScale = source.transform.lossyScale;

            SpriteRenderer cloneRenderer = clone.AddComponent<SpriteRenderer>();
            cloneRenderer.sprite = source.sprite;
            cloneRenderer.color = source.color;
            cloneRenderer.flipX = source.flipX;
            cloneRenderer.flipY = source.flipY;
            cloneRenderer.sharedMaterial = source.sharedMaterial;
            cloneRenderer.sortingLayerID = source.sortingLayerID;
            cloneRenderer.sortingOrder = source.sortingOrder;
            cloneRenderer.maskInteraction = source.maskInteraction;
            cloneRenderer.spriteSortPoint = source.spriteSortPoint;
            return clone;
        }

        private static void FadeOut(GameObject visual, SpriteRenderer visualRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetLink(visual);
            sequence
                .Append(visualRenderer.DOFade(0f, FadeDuration).SetEase(Ease.InQuad))
                .OnComplete(() => UnityEngine.Object.Destroy(visual))
                .OnKill(() =>
                {
                    if (visual != null)
                    {
                        UnityEngine.Object.Destroy(visual);
                    }
                });
        }

        private static void ScatterFragments(SpriteRenderer source)
        {
            Sprite sprite = source.sprite;
            Rect textureRect = sprite.textureRect;
            if (textureRect.width < FragmentGridSize || textureRect.height < FragmentGridSize)
            {
                return;
            }

            float cellWidth = textureRect.width / FragmentGridSize;
            float cellHeight = textureRect.height / FragmentGridSize;
            Vector3 scale = source.transform.lossyScale;
            float cellWorldWidth = cellWidth / sprite.pixelsPerUnit * scale.x;
            float cellWorldHeight = cellHeight / sprite.pixelsPerUnit * scale.y;
            Vector3 center = source.bounds.center;
            int spawned = 0;

            for (int column = 0; column < FragmentGridSize; column++)
            {
                for (int row = 0; row < FragmentGridSize; row++)
                {
                    if (spawned >= MaxFragments)
                    {
                        return;
                    }

                    Rect cell = new Rect(
                        textureRect.x + column * cellWidth,
                        textureRect.y + row * cellHeight,
                        cellWidth,
                        cellHeight);
                    float offsetFromCenter = column - (FragmentGridSize - 1) * 0.5f;
                    Vector3 cellPosition = center + new Vector3(
                        offsetFromCenter * cellWorldWidth,
                        (row - (FragmentGridSize - 1) * 0.5f) * cellWorldHeight,
                        0f);

                    SpawnFragment(source, sprite, cell, cellPosition, offsetFromCenter);
                    spawned++;
                }
            }
        }

        private static void SpawnFragment(
            SpriteRenderer source,
            Sprite sprite,
            Rect cell,
            Vector3 position,
            float offsetFromCenter)
        {
            Sprite fragmentSprite = Sprite.Create(
                sprite.texture,
                cell,
                new Vector2(0.5f, 0.5f),
                sprite.pixelsPerUnit);

            GameObject fragment = new GameObject($"{source.name}DeathFragment");
            fragment.transform.position = position;
            fragment.transform.localScale = source.transform.lossyScale;

            SpriteRenderer fragmentRenderer = fragment.AddComponent<SpriteRenderer>();
            fragmentRenderer.sprite = fragmentSprite;
            fragmentRenderer.color = source.color;
            fragmentRenderer.sharedMaterial = source.sharedMaterial;
            fragmentRenderer.sortingLayerID = source.sortingLayerID;
            fragmentRenderer.sortingOrder = source.sortingOrder + 1;

            Vector3 drift = new Vector3(
                offsetFromCenter * FragmentSpreadDistance,
                FragmentRiseDistance,
                0f);

            Sequence sequence = DOTween.Sequence();
            sequence.SetLink(fragment);
            sequence
                .Append(fragment.transform.DOMove(position + drift, FragmentLifetime).SetEase(Ease.OutQuad))
                .Join(fragment.transform.DORotate(
                    new Vector3(0f, 0f, offsetFromCenter * FragmentSpin),
                    FragmentLifetime))
                .Join(fragmentRenderer.DOFade(0f, FragmentLifetime).SetEase(Ease.InQuad))
                .OnComplete(() => DestroyFragment(fragment, fragmentSprite))
                .OnKill(() => DestroyFragment(fragment, fragmentSprite));
        }

        private static void DestroyFragment(GameObject fragment, Sprite fragmentSprite)
        {
            if (fragment != null)
            {
                UnityEngine.Object.Destroy(fragment);
            }

            if (fragmentSprite != null)
            {
                UnityEngine.Object.Destroy(fragmentSprite);
            }
        }
    }
}
