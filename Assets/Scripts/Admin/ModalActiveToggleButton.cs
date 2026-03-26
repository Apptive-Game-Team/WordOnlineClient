using Global.Button;
using UnityEngine;

namespace Admin
{
    public class ModalActiveToggleButton : ButtonBase
    {
        [SerializeField] private GameObject modal;
        [SerializeField] private bool initialActiveState = false;

        private void Start()
        {
            modal.SetActive(initialActiveState);
        }
        
        protected override void OnClickButton()
        {
            modal.SetActive(!modal.activeSelf);
        }
    }
}