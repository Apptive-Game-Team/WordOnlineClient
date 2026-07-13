using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameScene
{
    public static class PointerInputUtility
    {
        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();

        public static bool IsPointerOverUi()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            RaycastResults.Clear();
            eventSystem.RaycastAll(pointerData, RaycastResults);
            foreach (RaycastResult result in RaycastResults)
            {
                if (result.module is GraphicRaycaster)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
