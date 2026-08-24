using System.Collections;
using GameScene.Card;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
    public class BarController : MonoBehaviour
    {
    
        private RectTransform _rectTransform;
        [SerializeField] private bool isActive = false;
        [SerializeField] private Button manaBarButton;
        [SerializeField] private Button fieldButton;
        private bool lastActive = false;
        private Coroutine _moveBarCoroutine;

        /// <summary>훈수 시스템이 마나 바를 띄웠는지 판단할 때 읽는다.</summary>
        public bool IsBarOpen => isActive;

        /// <summary>훈수 시스템이 강조할 마나 바 버튼.</summary>
        public Transform ManaBarButtonTransform => manaBarButton != null ? manaBarButton.transform : null;
    
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        
            manaBarButton.onClick.AddListener(() =>
            {
                if (CardInputSender.Instance.IsWaitingInputResponse())
                {
                    return;
                }

                isActive = true;
            });
            fieldButton.onClick.AddListener(() =>
            {
                isActive = false;
            });
        }

        private void Update()
        {
            if (CardInputSender.Instance.IsFieldSelectMode() || CardInputSender.Instance.IsWaitingInputResponse())
            {
                isActive = false;
            }
        
            fieldButton.gameObject.SetActive(isActive);
        
            if (lastActive != isActive)
            {
                lastActive = isActive;
                SetBarActive(isActive);
            }
        }

        private void SetBarActive(bool active)
        {
            if (_moveBarCoroutine != null)
            {
                StopCoroutine(_moveBarCoroutine);
            }
            _moveBarCoroutine = StartCoroutine(MoveBar(active));
        }


    
        private IEnumerator MoveBar(bool up, float duration = 0.5f)
        {
        
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector2 endPosition = up ? 
                new Vector2(0, 540) : 
                new Vector2(0, 240);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _rectTransform.anchoredPosition = endPosition; // Ensure final position is set
        }
    }
}
