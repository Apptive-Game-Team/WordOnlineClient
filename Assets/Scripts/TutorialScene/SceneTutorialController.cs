using System;
using Global;
using UnityEngine;

namespace TutorialScene
{
    public abstract class SceneTutorialController<T> : LocalSingletonObject<T> where T : SceneTutorialController<T>
    {
        [SerializeField] private GameObject mask;
        [SerializeField] private TutorialPanel panel;

        protected override void Awake()
        {
            base.Awake();
            Hide();
        }

        protected void Show(string messageKey, Transform target, Action onNext = null)
        {
            Show(messageKey, target != null ? new[] { target } : null, onNext);
        }

        protected void Show(string messageKey, Transform target, TutorialPanelSide panelSide, Action onNext = null)
        {
            Show(messageKey, target != null ? new[] { target } : null, onNext, panelSide);
        }

        protected void Show(string messageKey, Transform[] targets, Action onNext = null)
        {
            Show(messageKey, targets, onNext, TutorialPanelSide.Left);
        }

        protected void Show(string messageKey, Transform[] targets, Action onNext, TutorialPanelSide panelSide)
        {
            if (mask != null)
            {
                mask.SetActive(true);
                mask.transform.SetAsFirstSibling();
            }

            if (targets != null)
            {
                foreach (Transform target in targets)
                {
                    if (target != null)
                    {
                        target.SetAsFirstSibling();
                    }
                }
            }

            panel?.Show(messageKey, onNext, panelSide);
        }

        public void Hide()
        {
            if (mask != null)
            {
                mask.SetActive(false);
            }

            panel?.Hide();
        }
    }
}
