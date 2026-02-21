using Scripts.Data;
using UnityEngine;

namespace Scripts.LobbyScene.SettingPage
{
    public class OnlyOnGuestModeUI : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(GuestContext.IsGuest);
        }
    }
}