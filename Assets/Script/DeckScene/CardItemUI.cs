using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardCountText;
    
    [SerializeField] private Sprite typeSprite;
    [SerializeField] private Sprite magicSprite;
    
    public void Init(string cName, int count)
    {
        cardNameText.text = cName;
        cardCountText.text = $" X {count.ToString()}" ;
        if(LocalMagicData.GetMagicData(cName).type.Equals("type"))
        {
            GetComponent<Image>().sprite = typeSprite;
        }
        else if (LocalMagicData.GetMagicData(cName).type.Equals("magic"))
        {
            GetComponent<Image>().sprite = magicSprite;
        }
    }
}
