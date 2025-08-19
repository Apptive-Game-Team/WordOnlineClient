using UnityEngine;
using UnityEngine.UI;

namespace Script.Global
{
    public abstract class AsyncButtonBase : ButtonBase
    {
        private bool isActive = true;
        
        public override void ButtonEvent()
        {
            if (!isActive)
            {
                return;
            }
            base.ButtonEvent();
            gameObject.GetComponent<Image>().color = Color.gray;
            isActive = false;
        }
        
        public void ResetButton()
        {
            isActive = true;
            gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}