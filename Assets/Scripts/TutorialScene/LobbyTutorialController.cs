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
            Show("onboarding.matching.matchPlayers", matchButton != null ? matchButton.transform : null, onNext, true);
        }

        public void ShowBotButton(System.Action onNext)
        {
            Show("onboarding.matching.matchBot", botButton != null ? botButton.transform : null, onNext, true);
        }

        public void ShowAdventureButton(System.Action onNext)
        {
            Show("onboarding.matching.adventure", adventureButton != null ? adventureButton.transform : null, onNext, true);
        }

        public void ShowMenuButton(System.Action onNext)
        {
            Show("onboarding.menu.optionsProfile", menuButton != null ? menuButton.transform : null, onNext, true);
        }

        // 앞선 로비 스텝들과 달리 버튼 클릭을 막지 않는다. 여기서는 설명이 아니라 플레이어가
        // 실제로 눌러 첫 전투를 시작하는 것이 목적이다.
        public void ShowPracticeButton(System.Action onClick)
        {
            Show("onboarding.hospitality.startPractice", botButton != null ? botButton.transform : null);
            if (botButton != null)
            {
                botButton.OnClick += onClick;
            }
        }

        public void HidePracticeButton(System.Action onClick)
        {
            if (botButton != null)
            {
                botButton.OnClick -= onClick;
            }

            Hide();
        }

        /// <summary>첫 전투를 마치고 로비로 돌아온 플레이어에게 보내는 마무리 안내.</summary>
        public void ShowFirstMatchComplete(System.Action onNext)
        {
            Show("onboarding.hospitality.complete", (Transform)null, onNext);
        }
    }
}
