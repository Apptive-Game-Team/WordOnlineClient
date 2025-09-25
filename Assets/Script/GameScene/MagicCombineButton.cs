using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicCombineButton : MonoBehaviour
{
    
    [SerializeField] private CardInputSender _cardInputSender;
    [SerializeField] private Image _buttonImage;
    
    private void Update()
    {
        _buttonImage.color = _cardInputSender.IsFieldSelectMode ? Color.gray : Color.white;
    }
    
    public void OnClickCombineButton()
    {
        if (_cardInputSender.IsFieldSelectMode)
        {
            _cardInputSender.Cancel();
        } else
        {
            _cardInputSender.Confirm();
        }
    }
}
