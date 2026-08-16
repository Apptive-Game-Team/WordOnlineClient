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

        // PointerEventData를 호출마다 새로 만들면 그대로 GC 부담이 된다. EventSystem이 바뀔 때만 다시 만든다.
        private static EventSystem cachedEventSystem;
        private static PointerEventData cachedPointerEventData;

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

            if (cachedPointerEventData == null || cachedEventSystem != eventSystem)
            {
                cachedEventSystem = eventSystem;
                cachedPointerEventData = new PointerEventData(eventSystem);
            }

            cachedPointerEventData.position = Input.mousePosition;
            eventSystem.RaycastAll(cachedPointerEventData, RaycastResults);
        }
    }
}
