using System.Collections.Generic;
using GameScene.ServedObjectComponent;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Selectable = GameScene.ServedObjectComponent.Selectable;

namespace GameScene
{
    public static class PointerInputUtility
    {
        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();

        public static bool IsPointerOverUi()
        {
            RefreshRaycastResults();
            foreach (RaycastResult result in RaycastResults)
            {
                if (result.module is GraphicRaycaster)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPointerOverUiOrSelectable()
        {
            RefreshRaycastResults();
            foreach (RaycastResult result in RaycastResults)
            {
                if (result.module is GraphicRaycaster ||
                    result.gameObject != null && result.gameObject.GetComponentInParent<Selectable>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RefreshRaycastResults()
        {
            RaycastResults.Clear();
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            eventSystem.RaycastAll(pointerData, RaycastResults);
        }
    }
}
