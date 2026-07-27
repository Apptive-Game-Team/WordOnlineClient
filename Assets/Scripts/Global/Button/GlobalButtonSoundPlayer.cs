using System.Collections.Generic;
using Global.Sound;
using Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Global.Button
{
    public class GlobalButtonSoundPlayer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindObjectOfType<GlobalButtonSoundPlayer>() != null) return;

            GameObject soundPlayer = new GameObject(nameof(GlobalButtonSoundPlayer));
            DontDestroyOnLoad(soundPlayer);
            soundPlayer.AddComponent<GlobalButtonSoundPlayer>();
        }

        private void Update()
        {
            if (!Input.GetMouseButtonUp(0) || EventSystem.current == null) return;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                UnityEngine.UI.Button button = result.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
                if (button == null || !button.IsInteractable()) continue;
                if (button.GetComponentInParent<ButtonBase>() != null) return;

                UiSfxPlayer.Play(SoundAssets.ClickButton);
                return;
            }
        }
    }
}
