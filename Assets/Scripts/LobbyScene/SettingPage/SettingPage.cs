using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LobbyScene.SettingPage
{
    public class SettingPage : MonoBehaviour
    {
        [SerializeField] private GameObject closeButton;
        [SerializeField] private GameObject settingPage;
        [SerializeField] private GameObject guestRegisterPage;
    
        private readonly Stack<GameObject> pageStack = new Stack<GameObject>();
    
        public void OpenPage(string pageName = "Setting")
        {
            switch (pageName)
            {
                case "Setting":
                    pageStack.Push(settingPage);
                    break;
                case "GuestRegister":
                    pageStack.Push(guestRegisterPage);
                    break;
                default:
                    pageStack.Push(settingPage);
                    break;
            }
            pageStack.Peek().SetActive(true);
            closeButton.SetActive(true);
        }
    
        public void ClosePage()
        {
            if (pageStack.Count > 0)
            {
                pageStack.Pop().SetActive(false);
            }
            if (pageStack.Count == 0)
            {
                closeButton.SetActive(false);
            }
        }
    }
}
