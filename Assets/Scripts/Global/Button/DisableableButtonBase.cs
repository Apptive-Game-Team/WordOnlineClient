using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Global.Button
{
    public abstract class DisableableButtonBase : ButtonBase
    {
        protected bool isActive = true;
        
        protected void SetButton()
        {
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