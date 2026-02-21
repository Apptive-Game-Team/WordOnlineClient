using Data;
using UnityEngine;

namespace LobbyScene.SettingPage
{
    public class OnlyOnGuestModeUI : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(GuestContext.IsGuest);
        }
    }
}