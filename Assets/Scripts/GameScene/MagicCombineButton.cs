using GameScene.Card;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
    public class MagicCombineButton : MonoBehaviour
    {

        [SerializeField] private Image _buttonImage;
    
        private void Update()
        {
            _buttonImage.color = CardInputSender.Instance.IsFieldSelectMode() || CardInputSender.Instance.IsWaitingInputResponse()
                ? Color.gray
                : Color.white;
        }
    
        public void OnClickCombineButton()
        {
            if (CardInputSender.Instance.IsWaitingInputResponse())
            {
                return;
            }

            if (CardInputSender.Instance.IsFieldSelectMode())
            {
                CardInputSender.Instance.Cancel();
            } else
            {
                CardInputSender.Instance.Confirm();
            }
        }
    }
}
