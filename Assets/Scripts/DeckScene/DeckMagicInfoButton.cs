using System;
using System.Collections.Generic;
using Data.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace DeckScene
{
    public class DeckMagicInfoButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private DeckInfoMagicPopup popup;

        private Func<IReadOnlyList<CombinedMagicData>> getAvailableMagics;

        private void Awake()
        {
            button ??= GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(TogglePopup);
            }
        }

        public void Init(Func<IReadOnlyList<CombinedMagicData>> availableMagicProvider)
        {
            getAvailableMagics = availableMagicProvider;
        }

        private void TogglePopup()
        {
            if (popup == null)
            {
                Debug.LogWarning("[DeckMagicInfoButton] popup is not assigned.");
                return;
            }

            if (popup.IsVisible())
            {
                popup.Hide();
                return;
            }

            popup.Show(getAvailableMagics?.Invoke());
        }
    }
}
