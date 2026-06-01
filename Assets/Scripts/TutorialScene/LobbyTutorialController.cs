using Global.Button;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialScene
{
    public class LobbyTutorialController : SceneTutorialController<LobbyTutorialController>
    {
        [Header("Lobby Targets")]
        [SerializeField] private ButtonBase deckButton;
        [SerializeField] private ButtonBase magicBookButton;
        [SerializeField] private ButtonBase matchButton;
        [SerializeField] private ButtonBase botButton;
        [SerializeField] private ButtonBase adventureButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private TMP_Dropdown deckDropdown;

        public void ShowDeckButton(System.Action onClick)
        {
            Show("onboarding.deck.openDeck", deckButton != null ? deckButton.transform : null);
            if (deckButton != null)
            {
                deckButton.OnClick += onClick;
            }
        }

        public void HideDeckButton(System.Action onClick)
        {
            if (deckButton != null)
            {
                deckButton.OnClick -= onClick;
            }

            Hide();
        }

        public void ShowMagicBookButton(System.Action onClick)
        {
            Show("onboarding.magicBook.openMagicBook", magicBookButton != null ? magicBookButton.transform : null);
            if (magicBookButton != null)
            {
                magicBookButton.OnClick += onClick;
            }
        }

        public void HideMagicBookButton(System.Action onClick)
        {
            if (magicBookButton != null)
            {
                magicBookButton.OnClick -= onClick;
            }

            Hide();
        }

        public void ShowDeckDropdown(System.Action onNext)
        {
            Show("onboarding.matching.deckDropdown", deckDropdown != null ? deckDropdown.transform : null, onNext);
        }

        public void ShowMatchButton(System.Action onNext)
        {
            Show("onboarding.matching.matchPlayers", matchButton != null ? matchButton.transform : null, onNext);
        }

        public void ShowBotButton(System.Action onNext)
        {
            Show("onboarding.matching.matchBot", botButton != null ? botButton.transform : null, onNext);
        }

        public void ShowAdventureButton(System.Action onNext)
        {
            Show("onboarding.matching.adventure", adventureButton != null ? adventureButton.transform : null, onNext);
        }

        public void ShowMenuButton(System.Action onNext)
        {
            Show("onboarding.menu.optionsProfile", menuButton != null ? menuButton.transform : null, onNext);
        }
    }
}
