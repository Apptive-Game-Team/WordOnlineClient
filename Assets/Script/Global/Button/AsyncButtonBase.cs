using UnityEngine;
using UnityEngine.UI;

namespace Script.Global
{
    public abstract class AsyncButtonBase : DisableableButtonBase
    {
        public override void ButtonEvent()
        {
            if (!isActive)
            {
                return;
            }
            base.ButtonEvent();
            SetButton();
        }
    }
}