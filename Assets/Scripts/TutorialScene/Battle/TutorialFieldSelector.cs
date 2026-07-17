using Data;
using Data.Magic;
using GameScene;
using UnityEngine;

namespace TutorialScene
{
    public class TutorialFieldSelector : MonoBehaviour
    {
        private const int RangeIndicatorSortingOrder = 5;
        private const int AimIndicatorSortingOrder = 16;
        private const float AimIndicatorRadius = 0.18f;

        TutorialCardSender cardInputSender;
        private GameObject currentAimObj;
        private GameObject currentRangeObj;
        private GameObject currentSkillIndicator;
        private bool currentSkillIndicatorIsLine;

        void Start()
        {
            cardInputSender = FindObjectOfType<TutorialCardSender>();
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


            var md = LocalMagicData.GetMagicData(cardInputSender.GetMagicName());
            Vector3 casterPosition = new Vector3(1, 5, 0);
            SetCircleWorldRadius(currentRangeObj, casterPosition, md.range);


            bool wantLine = md.name == "Shoot";


            if (currentSkillIndicator == null || currentSkillIndicatorIsLine != wantLine)
            {
                if (currentSkillIndicator != null) Destroy(currentSkillIndicator);
                currentSkillIndicator = wantLine ? CreateLineSkillIndicator() : CreateCircleSkillIndicator();
                currentSkillIndicatorIsLine = wantLine;
            }


            if (!currentSkillIndicator.activeSelf) currentSkillIndicator.SetActive(true);

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentAimObj.transform.position = mouseWorldPos;

            UpdateSkillIndicator(wantLine, casterPosition, mouseWorldPos, md.range, md.radius);


            if (PointerInputUtility.IsPointerOverUi()) return;

            if (Input.GetMouseButtonUp(0))
            {
                cardInputSender.SendInput(mouseWorldPos);
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                currentSkillIndicator.SetActive(false);
                cardInputSender.SetExpectedMagicUI();
            }
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

        private static void SetCircleWorldRadius(GameObject indicator, Vector3 position, float radius)
        {
            SkillIndicatorShapeRenderer shapeRenderer = indicator.GetComponent<SkillIndicatorShapeRenderer>();
            if (shapeRenderer == null)
            {
                shapeRenderer = indicator.AddComponent<SkillIndicatorShapeRenderer>();
            }

            shapeRenderer.SetCircle(position, radius, true, RangeIndicatorSortingOrder, 0f);
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
    }
}
