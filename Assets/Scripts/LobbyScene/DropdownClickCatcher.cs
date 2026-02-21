using UnityEngine;
using UnityEngine.EventSystems;

namespace LobbyScene
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class DropdownClickCatcher : MonoBehaviour, IPointerClickHandler
    {
        // DeckDropdownController 또는 FetchDecks 코루틴을 실행할 참조
        [SerializeField] private LobbyUIController controller;

        public void OnPointerClick(PointerEventData eventData)
        {
            // Dropdown 클릭 시마다 서버에서 덱 리스트 새로 가져오기
            controller.StartCoroutine(controller.FetchDecks());
        }
    }
}