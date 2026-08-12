using Data;
using Data.GameConfig;
using Data.Magic;
using GameScene.Card;
using GameScene.Player;
using GameScene.ServedObjectComponent;
using Global;
using Global.Sound;
using Sound;
using UnityEngine;

namespace GameScene
{
    /// <summary>
    /// 필드 선택 모드에서 조준/사거리/스킬 인디케이터를 마우스에 맞춰 갱신한다.
    /// <para>
    /// 이 Update는 마우스를 따라가야 하므로 프레임당 비용이 곧 체감 반응성이다.
    /// 씬 전역 탐색, GetComponent, UI 레이캐스트처럼 매 프레임 반복할 이유가 없는 작업은
    /// 캐시하거나 실제로 필요한 시점까지 미룬다.
    /// </para>
    /// </summary>
    public class FieldSelector : MonoBehaviour
    {
        private const int RangeIndicatorSortingOrder = 5;
        private const int AimIndicatorSortingOrder = 16;
        private const float AimIndicatorRadius = 0.18f;

        /// <summary>참조를 찾지 못했을 때 씬 전체 스캔을 매 프레임 되풀이하지 않기 위한 재시도 간격.</summary>
        private const float MissingReferenceRetryInterval = 0.5f;

        CardInputSender cardInputSender;
        private GameObject currentAimObj;
        private GameObject currentRangeObj;
        private GameObject currentSkillIndicator;
        private bool currentSkillIndicatorIsLine;

        // 인디케이터 컴포넌트는 생성 시점에 잡아 둔다. 매 프레임 GetComponent를 부를 이유가 없다.
        private SkillIndicatorShapeRenderer aimShapeRenderer;
        private SkillIndicatorShapeRenderer rangeShapeRenderer;
        private CircleSkillIndicator currentCircleIndicator;
        private LineSkillIndicator currentLineIndicator;

        // 매 프레임 다시 구할 필요가 없는 참조/결과 캐시.
        private Camera cachedCamera;
        private ServedObject cachedCaster;
        private Vector3 cachedCasterSourcePosition;
        private Vector3 cachedCasterGroundPosition;
        private bool hasCachedCasterGroundPosition;
        private float nextCasterSearchTime;
        private float nextGroundColliderSearchTime;

        // 경고 로그가 필드 선택 중 매 프레임 쏟아지지 않도록 상태 전환에서만 남긴다.
        private bool warnedMissingCaster;
        private bool warnedMissingMagicData;
        private string warnedMissingRangeMagicName;

        private string lastLoggedMagicName;
        private float lastLoggedRange;
        private float lastLoggedRadius;
        private bool hasLoggedMagicParameters;

        [SerializeField] private GameObject aimObject;
        [SerializeField] private GameObject rangeObject;
        [SerializeField] private GameObject lineSkillIndicator;
        [SerializeField] private GameObject circleSkillIndicator;
        [SerializeField] private Collider groundCollider;
        [SerializeField] private string groundObjectName = "PopupBookGround";
        private AudioSource interactionAudioSource;

        void Start()
        {
            cardInputSender = FindObjectOfType<CardInputSender>();
            interactionAudioSource = gameObject.GetComponent<AudioSource>();
            if (interactionAudioSource == null)
            {
                interactionAudioSource = gameObject.AddComponent<AudioSource>();
            }
            SoundVolumeSetter.Attach(interactionAudioSource, SoundVolumeSetter.SoundType.UI);
            currentAimObj = CreateAimIndicator(out aimShapeRenderer);
            currentRangeObj = CreateRangeIndicator(out rangeShapeRenderer);
            currentAimObj.SetActive(false);
            currentRangeObj.SetActive(false);

            currentSkillIndicator = CreateCircleSkillIndicator(out currentCircleIndicator);
            currentLineIndicator = null;
            currentSkillIndicatorIsLine = false;
            currentSkillIndicator.SetActive(false);
        }

        void Update()
        {
            if (!CardInputSender.Instance.IsFieldSelectMode())
            {
                if (currentAimObj.activeSelf) currentAimObj.SetActive(false);
                if (currentRangeObj.activeSelf) currentRangeObj.SetActive(false);
                if (currentSkillIndicator != null && currentSkillIndicator.activeSelf)
                {
                    currentSkillIndicator.SetActive(false);
                }

                return;
            }

            if (!currentAimObj.activeSelf) currentAimObj.SetActive(true);
            if (!currentRangeObj.activeSelf) currentRangeObj.SetActive(true);


            if (!TryGetCurrentMagicParameters(out CombinedMagicData magicData, out float range, out float radius))
            {
                if (currentRangeObj.activeSelf) currentRangeObj.SetActive(false);
                if (currentSkillIndicator != null && currentSkillIndicator.activeSelf)
                {
                    currentSkillIndicator.SetActive(false);
                }
                return;
            }
            LogMagicParametersIfChanged(magicData, range, radius);

            Vector3 casterPosition = GetCasterPosition();
            rangeShapeRenderer.SetCircle(casterPosition, range, true, RangeIndicatorSortingOrder, 0f);


            bool wantLine = IsLineMagic(magicData);


            if (currentSkillIndicator == null || currentSkillIndicatorIsLine != wantLine)
            {
                if (currentSkillIndicator != null) Destroy(currentSkillIndicator);
                if (wantLine)
                {
                    currentSkillIndicator = CreateLineSkillIndicator(out currentLineIndicator);
                    currentCircleIndicator = null;
                }
                else
                {
                    currentSkillIndicator = CreateCircleSkillIndicator(out currentCircleIndicator);
                    currentLineIndicator = null;
                }
                currentSkillIndicatorIsLine = wantLine;
            }


            if (!currentSkillIndicator.activeSelf) currentSkillIndicator.SetActive(true);

            if (!TryGetGroundPosition(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                return;
            }

            Vector3 previewPosition = ClampToRange(mouseWorldPos, casterPosition, range);
            aimShapeRenderer.SetCircle(previewPosition, AimIndicatorRadius, true, AimIndicatorSortingOrder, 0f);
            UpdateSkillIndicator(wantLine, casterPosition, previewPosition, range, radius);

            // UI 레이캐스트는 클릭을 걸러내는 용도뿐이므로, 실제로 버튼을 뗀 프레임에만 수행한다.
            if (!Input.GetMouseButtonUp(0))
            {
                return;
            }

            if (PointerInputUtility.IsPointerOverUiOrSelectable()) return;

            if (!CardInputSender.Instance.TrySendInput(previewPosition))
            {
                return;
            }

            interactionAudioSource.PlayOneShot(SoundAssets.FieldConfirm);
            PlayerFeedbackController.Instance.UseMagicFeedback();
            currentAimObj.SetActive(false);
            currentRangeObj.SetActive(false);
            currentSkillIndicator.SetActive(false);
            CardInputSender.Instance.SetExpectedMagicUI();
        }

        private Vector3 GetCasterPosition()
        {
            ServedObject caster = ResolveCaster();
            if (caster == null)
            {
                if (!warnedMissingCaster)
                {
                    warnedMissingCaster = true;
                    WDebug.LogWarning($"[FieldSelector] Could not find caster object for {SceneContext.Me}.");
                }
                return currentRangeObj.transform.position;
            }

            warnedMissingCaster = false;
            Vector3 position = caster.transform.position;

            // 캐스터가 그대로면 지면 투영 레이캐스트를 다시 쏠 이유가 없다.
            if (hasCachedCasterGroundPosition && cachedCasterSourcePosition == position)
            {
                return cachedCasterGroundPosition;
            }

            // 투영에 실패했을 때는 캐시하지 않는다. 지면 콜라이더가 뒤늦게 잡히면 그때 제대로 투영되어야 한다.
            if (!TryProjectToGround(position, out Vector3 groundPosition))
            {
                hasCachedCasterGroundPosition = false;
                return position;
            }

            cachedCasterSourcePosition = position;
            cachedCasterGroundPosition = groundPosition;
            hasCachedCasterGroundPosition = true;
            return cachedCasterGroundPosition;
        }

        /// <summary>
        /// 캐스터를 찾는 씬 스캔은 비싸므로 결과를 들고 있는다.
        /// 아직 못 찾았을 때(사망/미스폰)만 일정 간격으로 다시 시도한다.
        /// </summary>
        private ServedObject ResolveCaster()
        {
            if (cachedCaster != null)
            {
                return cachedCaster;
            }

            if (Time.unscaledTime < nextCasterSearchTime)
            {
                return null;
            }

            nextCasterSearchTime = Time.unscaledTime + MissingReferenceRetryInterval;
            cachedCaster = FindPlayerObject(SceneContext.Me);
            hasCachedCasterGroundPosition = false;
            return cachedCaster;
        }

        private static ServedObject FindPlayerObject(string master)
        {
            PlayerNameSetter[] playerNameSetters = FindObjectsByType<PlayerNameSetter>(FindObjectsSortMode.None);
            foreach (PlayerNameSetter playerNameSetter in playerNameSetters)
            {
                ServedObject servedObject = playerNameSetter.GetComponent<ServedObject>();
                if (servedObject != null && servedObject.GetMaster() == master)
                {
                    return servedObject;
                }
            }

            return null;
        }

        private static Vector3 ClampToRange(Vector3 targetPosition, Vector3 origin, float range)
        {
            float safeRange = Mathf.Max(range, 0f);
            Vector3 flattenedTarget = targetPosition;
            flattenedTarget.y = origin.y;
            Vector3 offset = flattenedTarget - origin;
            if (offset.sqrMagnitude <= safeRange * safeRange)
            {
                return targetPosition;
            }

            Vector3 clampedPosition = origin + offset.normalized * safeRange;
            clampedPosition.y = targetPosition.y;
            return clampedPosition;
        }

        public bool TryGetGroundPosition(Vector3 screenPosition, out Vector3 groundPosition)
        {
            groundPosition = Vector3.zero;
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            Ray ray = camera.ScreenPointToRay(screenPosition);
            bool hasGroundCollider = TryGetGroundCollider(out Collider resolvedGroundCollider);
            if (hasGroundCollider && resolvedGroundCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                groundPosition = hit.point;
                return true;
            }

            Plane groundPlane = hasGroundCollider
                ? new Plane(resolvedGroundCollider.transform.up, resolvedGroundCollider.transform.position)
                : new Plane(Vector3.up, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return false;
            }

            groundPosition = ray.GetPoint(distance);
            return true;
        }

        private Camera ResolveCamera()
        {
            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }

            return cachedCamera;
        }

        private bool TryProjectToGround(Vector3 worldPosition, out Vector3 groundPosition)
        {
            if (!TryGetGroundCollider(out Collider resolvedGroundCollider))
            {
                groundPosition = worldPosition;
                return false;
            }

            Ray downRay = new Ray(worldPosition + Vector3.up * 100f, Vector3.down);
            if (resolvedGroundCollider.Raycast(downRay, out RaycastHit hit, 200f))
            {
                groundPosition = hit.point;
                return true;
            }

            Plane groundPlane = new Plane(resolvedGroundCollider.transform.up, resolvedGroundCollider.transform.position);
            if (groundPlane.Raycast(downRay, out float distance))
            {
                groundPosition = downRay.GetPoint(distance);
                return true;
            }

            groundPosition = worldPosition;
            return false;
        }

        private bool TryGetGroundCollider(out Collider resolvedGroundCollider)
        {
            if (groundCollider != null)
            {
                resolvedGroundCollider = groundCollider;
                return true;
            }

            // 인스펙터 참조가 비어 있어도 씬 전체 스캔이 매 프레임 반복되지는 않게 한다.
            if (Time.unscaledTime < nextGroundColliderSearchTime)
            {
                resolvedGroundCollider = null;
                return false;
            }

            nextGroundColliderSearchTime = Time.unscaledTime + MissingReferenceRetryInterval;

            if (!string.IsNullOrEmpty(groundObjectName))
            {
                GameObject groundObject = GameObject.Find(groundObjectName);
                if (groundObject != null && groundObject.TryGetComponent(out groundCollider))
                {
                    resolvedGroundCollider = groundCollider;
                    return true;
                }
            }

            resolvedGroundCollider = FindObjectByName<Collider>("Ground") ??
                                     FindObjectByName<Collider>("Panel");
            groundCollider = resolvedGroundCollider;
            return resolvedGroundCollider != null;
        }

        private static T FindObjectByName<T>(string namePart) where T : Component
        {
            T[] components = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T component in components)
            {
                if (component != null && component.name.Contains(namePart, System.StringComparison.OrdinalIgnoreCase))
                {
                    return component;
                }
            }

            return null;
        }

        private bool TryGetCurrentMagicParameters(out CombinedMagicData magicData, out float range, out float radius)
        {
            magicData = null;
            range = 0f;
            radius = 0f;

            if (!CardInputSender.Instance.TryGetCurrentMagicData(out magicData))
            {
                if (!warnedMissingMagicData)
                {
                    warnedMissingMagicData = true;
                    WDebug.LogWarning("[FieldSelector] Could not resolve current magic data.");
                }
                return false;
            }

            warnedMissingMagicData = false;

            if (!GameParameterResolver.TryGetMagicParameter(magicData, "range", out range))
            {
                if (warnedMissingRangeMagicName != magicData.serverName)
                {
                    warnedMissingRangeMagicName = magicData.serverName;
                    WDebug.LogWarning($"[FieldSelector] Could not find range parameter for {magicData.serverName}.");
                }
                return false;
            }

            warnedMissingRangeMagicName = null;
            GameParameterResolver.TryGetMagicParameter(magicData, "radius", out radius);
            return true;
        }

        private static bool IsLineMagic(CombinedMagicData magicData)
        {
            return magicData.castType == CardType.Shoot;
        }

        private void UpdateSkillIndicator(
            bool isLine,
            Vector3 casterPosition,
            Vector3 previewPosition,
            float range,
            float radius)
        {
            if (isLine)
            {
                if (currentLineIndicator != null)
                {
                    currentLineIndicator.SetIndicator(casterPosition, previewPosition, range, radius);
                }
                return;
            }

            if (currentCircleIndicator != null)
            {
                currentCircleIndicator.SetIndicator(previewPosition, radius);
            }
        }

        private static GameObject CreateAimIndicator(out SkillIndicatorShapeRenderer shapeRenderer)
        {
            GameObject indicator = new GameObject("AimIndicator");
            shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            shapeRenderer.SetLocalCircle(AimIndicatorRadius, AimIndicatorSortingOrder);
            return indicator;
        }

        private static GameObject CreateRangeIndicator(out SkillIndicatorShapeRenderer shapeRenderer)
        {
            GameObject indicator = new GameObject("RangeIndicator");
            shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            return indicator;
        }

        private static GameObject CreateLineSkillIndicator(out LineSkillIndicator lineIndicator)
        {
            GameObject indicator = new GameObject("LineSkillIndicator");
            lineIndicator = indicator.AddComponent<LineSkillIndicator>();
            return indicator;
        }

        private static GameObject CreateCircleSkillIndicator(out CircleSkillIndicator circleIndicator)
        {
            GameObject indicator = new GameObject("CircleSkillIndicator");
            circleIndicator = indicator.AddComponent<CircleSkillIndicator>();
            return indicator;
        }

        private void LogMagicParametersIfChanged(CombinedMagicData magicData, float range, float radius)
        {
            // 문자열 키를 만들어 비교하면 매 프레임 문자열이 할당된다. 값 비교로 충분하다.
            if (hasLoggedMagicParameters &&
                lastLoggedMagicName == magicData.serverName &&
                lastLoggedRange == range &&
                lastLoggedRadius == radius)
            {
                return;
            }

            hasLoggedMagicParameters = true;
            lastLoggedMagicName = magicData.serverName;
            lastLoggedRange = range;
            lastLoggedRadius = radius;
            WDebug.Log($"[FieldSelector] magic={magicData.serverName}, range={range:F3}, radius={radius:F3}");
        }
    }
}
