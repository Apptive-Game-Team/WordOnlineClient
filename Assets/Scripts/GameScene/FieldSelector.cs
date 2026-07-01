using Data;
using Data.GameConfig;
using Data.Magic;
using GameScene.Card;
using GameScene.Player;
using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;
using UnityEngine.EventSystems;

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

        void Start()
        {
            cardInputSender = FindObjectOfType<CardInputSender>();
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
            if (!cardInputSender.IsFieldSelectMode())
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

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Vector3 previewPosition = ClampToRange(mouseWorldPos, casterPosition, range);
            currentAimObj.transform.position = previewPosition;
            UpdateSkillIndicator(wantLine, casterPosition, previewPosition, range, radius);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonUp(0))
            {
                cardInputSender.SendInput(mouseWorldPos);
                PlayerFeedbackController.Instance.UseMagicFeedback();
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                currentSkillIndicator.SetActive(false);
                cardInputSender.SetExpectedMagicUI();
            }
        }

        private Vector3 GetCasterPosition()
        {
            ServedObject caster = FindPlayerObject(SceneContext.Me);
            if (caster != null)
            {
                Vector3 position = caster.transform.position;
                position.z = 0f;
                return position;
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
            Vector3 offset = targetPosition - origin;
            offset.z = 0f;
            if (offset.sqrMagnitude <= safeRange * safeRange)
            {
                return targetPosition;
            }

            return origin + offset.normalized * safeRange;
        }

        private bool TryGetCurrentMagicParameters(out CombinedMagicData magicData, out float range, out float radius)
        {
            magicData = null;
            range = 0f;
            radius = 0f;

            if (!cardInputSender.TryGetCurrentMagicData(out magicData))
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
