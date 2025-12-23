using Script.Data;
using UnityEngine;

namespace Script.LobbyScene.SettingPage
{
    public class OnlyOnGuestModeUI : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(GuestContext.IsGuest);
        }
    }
}