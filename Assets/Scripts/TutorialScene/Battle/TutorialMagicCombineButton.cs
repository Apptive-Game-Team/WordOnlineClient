using UnityEngine;
using UnityEngine.UI;

namespace TutorialScene
{
    public class TutorialMagicCombineButton : MonoBehaviour
    {
    
        [SerializeField] private TutorialCardSender _cardInputSender;
        [SerializeField] private Image _buttonImage;
    
        private void Update()
        {
            _buttonImage.color = _cardInputSender.IsFieldSelectMode() ? Color.gray : Color.white;
        }
    
        public void OnClickCombineButton()
        {
            if (_cardInputSender.IsFieldSelectMode())
            {
                _cardInputSender.Cancel();
            } else
            {
                _cardInputSender.Confirm();
            }
        }
    }
}
