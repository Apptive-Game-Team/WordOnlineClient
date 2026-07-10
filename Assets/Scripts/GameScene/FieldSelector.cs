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
        private GameObject currentAimObj;
        private GameObject currentRangeObj;
        private GameObject currentSkillIndicator;

        [SerializeField] private GameObject aimObject;
        [SerializeField] private GameObject rangeObject;
        [SerializeField] private GameObject lineSkillIndicator;
        [SerializeField] private GameObject circleSkillIndicator;

        void Start()
        {
            currentAimObj = Instantiate(aimObject);
            currentRangeObj = Instantiate(rangeObject);
            currentAimObj.SetActive(false);
            currentRangeObj.SetActive(false);

            currentSkillIndicator = Instantiate(circleSkillIndicator);
            currentSkillIndicatorPrefabRef = circleSkillIndicator;
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

            Vector3 casterPosition = GetCasterPosition();
            SetCircleWorldRadius(currentRangeObj, range);
            currentRangeObj.transform.position = casterPosition;


            bool wantLine = IsLineMagic(magicData);
            GameObject wantedPrefab = wantLine ? lineSkillIndicator : circleSkillIndicator;


            if (currentSkillIndicator == null || !ReferenceEquals(currentSkillIndicatorPrefabRef, wantedPrefab))
            {
                if (currentSkillIndicator != null) Destroy(currentSkillIndicator);
                currentSkillIndicator = Instantiate(wantedPrefab);
                currentSkillIndicatorPrefabRef = wantedPrefab;
            }


            if (!currentSkillIndicator.activeSelf) currentSkillIndicator.SetActive(true);

            if (!TryGetGroundPosition(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                return;
            }

            Vector3 previewPosition = ClampToRange(mouseWorldPos, casterPosition, range);
            currentAimObj.transform.position = previewPosition;
            UpdateSkillIndicator(wantLine, casterPosition, previewPosition, range, radius);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonUp(0))
            {
                CardInputSender.Instance.SendInput(mouseWorldPos);
                PlayerFeedbackController.Instance.UseMagicFeedback();
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                currentSkillIndicator.SetActive(false);
                CardInputSender.Instance.SetExpectedMagicUI();
            }
        }

        private GameObject currentSkillIndicatorPrefabRef;

        private Vector3 GetCasterPosition()
        {
            ServedObject caster = FindPlayerObject(SceneContext.Me);
            if (caster != null)
            {
                Vector3 position = caster.transform.position;
                position.y = 0f;
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
            offset.y = 0f;
            if (offset.sqrMagnitude <= safeRange * safeRange)
            {
                return targetPosition;
            }

            return origin + offset.normalized * safeRange;
        }

        private static bool TryGetGroundPosition(Vector3 screenPosition, out Vector3 groundPosition)
        {
            groundPosition = Vector3.zero;
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return false;
            }

            groundPosition = ray.GetPoint(distance);
            groundPosition.y = 0f;
            return true;
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
                    lineIndicator.SetIndicator(casterPosition, previewPosition, range);
                }
                return;
            }

            CircleSkillIndicator circleIndicator = currentSkillIndicator.GetComponent<CircleSkillIndicator>();
            if (circleIndicator != null)
            {
                circleIndicator.SetIndicator(previewPosition, radius);
            }
        }

        private static void SetCircleWorldRadius(GameObject indicator, float radius)
        {
            SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null || radius <= 0f)
            {
                indicator.transform.localScale = new Vector3(0f, 0f, 1f);
                return;
            }

            float spriteDiameter = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
            float scale = (radius * 2f) / spriteDiameter;
            indicator.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
