using System.Collections.Generic;
using UnityEngine;

namespace Global.Button
{
    public class GameObjectVisibilityButton : ButtonBase
    {
        [SerializeField] private List<GameObject> offList = new();
        [SerializeField] private List<GameObject> onList = new();

        protected override void OnClickButton()
        {
            ApplyVisibility();
        }

        public void ApplyVisibility()
        {
            SetActive(offList, false);
            SetActive(onList, true);
        }

        private static void SetActive(IEnumerable<GameObject> targets, bool active)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.SetActive(active);
                }
            }
        }
    }
}
