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
    public class FieldSelector : MonoBehaviour
    {
        private const int RangeIndicatorSortingOrder = 5;
        private const int AimIndicatorSortingOrder = 16;
        private const float AimIndicatorRadius = 0.18f;

        CardInputSender cardInputSender;
        private GameObject currentAimObj;
        private GameObject currentRangeObj;
        private GameObject currentSkillIndicator;
        private bool currentSkillIndicatorIsLine;
        private string lastLoggedMagicParameterKey;

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
            currentAimObj = CreateAimIndicator();
            currentRangeObj = CreateRangeIndicator();
            currentAimObj.SetActive(false);
            currentRangeObj.SetActive(false);

            currentSkillIndicator = CreateCircleSkillIndicator();
            currentSkillIndicatorIsLine = false;
            currentSkillIndicator.SetActive(false);
        }

        void Update()
        {
            if (!CardInputSender.Instance.IsFieldSelectMode())
            {
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                if (currentSkillIndicator != null) currentSkillIndicator.SetActive(false);

                return;
            }
        
            if (!currentAimObj.activeSelf) currentAimObj.SetActive(true);
            currentRangeObj.SetActive(true);


            if (!TryGetCurrentMagicParameters(out CombinedMagicData magicData, out float range, out float radius))
            {
                currentRangeObj.SetActive(false);
                if (currentSkillIndicator != null) currentSkillIndicator.SetActive(false);
                return;
            }
            LogMagicParametersIfChanged(magicData, range, radius);

            Vector3 casterPosition = GetCasterPosition();
            SetCircleWorldRadius(currentRangeObj, casterPosition, range);


            bool wantLine = IsLineMagic(magicData);


            if (currentSkillIndicator == null || currentSkillIndicatorIsLine != wantLine)
            {
                if (currentSkillIndicator != null) Destroy(currentSkillIndicator);
                currentSkillIndicator = wantLine ? CreateLineSkillIndicator() : CreateCircleSkillIndicator();
                currentSkillIndicatorIsLine = wantLine;
            }


            if (!currentSkillIndicator.activeSelf) currentSkillIndicator.SetActive(true);

            if (!TryGetGroundPosition(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                return;
            }

            Vector3 previewPosition = ClampToRange(mouseWorldPos, casterPosition, range);
            SetAimIndicator(currentAimObj, previewPosition);
            UpdateSkillIndicator(wantLine, casterPosition, previewPosition, range, radius);

            if (PointerInputUtility.IsPointerOverUiOrSelectable()) return;

            if (Input.GetMouseButtonUp(0))
            {
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
        }

        private Vector3 GetCasterPosition()
        {
            ServedObject caster = FindPlayerObject(SceneContext.Me);
            if (caster != null)
            {
                Vector3 position = caster.transform.position;
                return TryProjectToGround(position, out Vector3 groundPosition) ? groundPosition : position;
            }

            WDebug.LogWarning($"[FieldSelector] Could not find caster object for {SceneContext.Me}.");
            return currentRangeObj.transform.position;
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

        private bool TryGetGroundPosition(Vector3 screenPosition, out Vector3 groundPosition)
        {
            groundPosition = Vector3.zero;
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (TryGetGroundCollider(out Collider resolvedGroundCollider) &&
                resolvedGroundCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                groundPosition = hit.point;
                return true;
            }

            Plane groundPlane = TryGetGroundCollider(out resolvedGroundCollider)
                ? new Plane(resolvedGroundCollider.transform.up, resolvedGroundCollider.transform.position)
                : new Plane(Vector3.up, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return false;
            }

            groundPosition = ray.GetPoint(distance);
            return true;
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
                WDebug.LogWarning("[FieldSelector] Could not resolve current magic data.");
                return false;
            }

            if (!GameParameterResolver.TryGetMagicParameter(magicData, "range", out range))
            {
                WDebug.LogWarning($"[FieldSelector] Could not find range parameter for {magicData.serverName}.");
                return false;
            }

            GameParameterResolver.TryGetMagicParameter(magicData, "radius", out radius);
            return true;
        }

        private static bool IsLineMagic(CombinedMagicData magicData)
        {
            return magicData.recipe != null && magicData.recipe.Contains(CardType.Shoot) ||
                   IsSameMagicName(magicData.serverName, "Shoot") ||
                   IsSameMagicName(magicData.resourceName, "Shoot") ||
                   IsSameMagicName(magicData.localizationKey, "Shoot");
        }

        private static bool IsSameMagicName(string value, string expected)
        {
            return string.Equals(value, expected, System.StringComparison.OrdinalIgnoreCase);
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
                LineSkillIndicator lineIndicator = currentSkillIndicator.GetComponent<LineSkillIndicator>();
                if (lineIndicator != null)
                {
                    lineIndicator.SetIndicator(casterPosition, previewPosition, range, radius);
                }
                return;
            }

            CircleSkillIndicator circleIndicator = currentSkillIndicator.GetComponent<CircleSkillIndicator>();
            if (circleIndicator != null)
            {
                circleIndicator.SetIndicator(previewPosition, radius);
            }
        }

        private static void SetCircleWorldRadius(GameObject indicator, Vector3 position, float radius)
        {
            SkillIndicatorShapeRenderer shapeRenderer = indicator.GetComponent<SkillIndicatorShapeRenderer>();
            if (shapeRenderer == null)
            {
                shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            }

            shapeRenderer.SetCircle(position, radius, true, RangeIndicatorSortingOrder, 0f);
        }

        private static void ConfigureAimIndicator(GameObject indicator)
        {
            SkillIndicatorShapeRenderer shapeRenderer = indicator.GetComponent<SkillIndicatorShapeRenderer>();
            if (shapeRenderer == null)
            {
                shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            }

            if (shapeRenderer != null)
            {
                shapeRenderer.SetLocalCircle(AimIndicatorRadius, AimIndicatorSortingOrder);
            }
        }

        private static void SetAimIndicator(GameObject indicator, Vector3 position)
        {
            SkillIndicatorShapeRenderer shapeRenderer = indicator.GetComponent<SkillIndicatorShapeRenderer>();
            if (shapeRenderer == null)
            {
                shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            }

            shapeRenderer.SetCircle(position, AimIndicatorRadius, true, AimIndicatorSortingOrder, 0f);
        }

        private static GameObject CreateAimIndicator()
        {
            GameObject indicator = new GameObject("AimIndicator");
            indicator.AddComponent<SkillIndicatorShapeRenderer>();
            ConfigureAimIndicator(indicator);
            return indicator;
        }

        private static GameObject CreateRangeIndicator()
        {
            GameObject indicator = new GameObject("RangeIndicator");
            indicator.AddComponent<SkillIndicatorShapeRenderer>();
            return indicator;
        }

        private static GameObject CreateLineSkillIndicator()
        {
            GameObject indicator = new GameObject("LineSkillIndicator");
            indicator.AddComponent<LineSkillIndicator>();
            return indicator;
        }

        private static GameObject CreateCircleSkillIndicator()
        {
            GameObject indicator = new GameObject("CircleSkillIndicator");
            indicator.AddComponent<CircleSkillIndicator>();
            return indicator;
        }

        private void LogMagicParametersIfChanged(CombinedMagicData magicData, float range, float radius)
        {
            string key = $"{magicData.serverName}:{range:F3}:{radius:F3}";
            if (lastLoggedMagicParameterKey == key)
            {
                return;
            }

            lastLoggedMagicParameterKey = key;
            WDebug.Log($"[FieldSelector] magic={magicData.serverName}, range={range:F3}, radius={radius:F3}");
        }
    }
}
