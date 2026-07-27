using System;
using Global.Sound;
using Sound;
using UnityEngine;

namespace Global.Button
{
    public abstract class ButtonBase : MonoBehaviour
    {
        public event Action OnClick;

        protected abstract void OnClickButton();

        public virtual void ButtonEvent()
        {
            UiSfxPlayer.Play(SoundAssets.ClickButton);
            OnClick?.Invoke();
            OnClickButton();
        }
    }
}
