using System;
using Global;
using UnityEngine;

namespace TutorialScene
{
    public abstract class SceneTutorialController<T> : LocalSingletonObject<T> where T : SceneTutorialController<T>
    {
        [SerializeField] private GameObject mask;
        [SerializeField] private GameObject targetClickBlocker;
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

        protected void Show(string messageKey, Transform target, Action onNext, bool blockTargetClick)
        {
            Show(messageKey, target != null ? new[] { target } : null, onNext, TutorialPanelSide.Left, blockTargetClick);
        }

        protected void Show(string messageKey, Transform target, TutorialPanelSide panelSide, Action onNext = null)
        {
            Show(messageKey, target != null ? new[] { target } : null, onNext, panelSide);
        }

        protected void Show(string messageKey, Transform[] targets, Action onNext = null)
        {
            Show(messageKey, targets, onNext, TutorialPanelSide.Left);
        }

        protected void Show(string messageKey, Transform[] targets, Action onNext, TutorialPanelSide panelSide, bool blockTargetClick = false)
        {
            if (mask != null)
            {
                mask.SetActive(true);
                mask.transform.SetAsLastSibling();
            }

            if (targets != null)
            {
                foreach (Transform target in targets)
                {
                    if (target != null)
                    {
                        target.SetAsLastSibling();
                    }
                }
            }

            if (targetClickBlocker != null)
            {
                targetClickBlocker.SetActive(blockTargetClick);
                if (blockTargetClick)
                {
                    targetClickBlocker.transform.SetAsLastSibling();
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

            if (targetClickBlocker != null)
            {
                targetClickBlocker.SetActive(false);
            }

            panel?.Hide();
        }
    }
}
