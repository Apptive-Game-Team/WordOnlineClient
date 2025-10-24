using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Script.Data;
using Script.Data.Localization;
using Script.GameScene;
using Script.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardManaText;
    [SerializeField] private TextMeshProUGUI cardCountText;
    [SerializeField] private CardImageMapper cardImageMapper;
    
    [SerializeField] private Sprite typeSprite;
    [SerializeField] private Sprite magicSprite;
    
    public async void Init(string cName, int count)
    {
        cardCountText.text = $" X {count.ToString()}" ;
        transform.GetChild(2).GetComponent<Image>().sprite = cardImageMapper.GetCardImage(cName);
        
        MagicData magicData = LocalMagicData.GetMagicData(cName);
        cardManaText.text = magicData.mana.ToString();

        switch (magicData.type)
        {
            case "type":
                GetComponent<Image>().sprite = typeSprite;
                break;
            case "magic":
                GetComponent<Image>().sprite = magicSprite;
                break;
            default:
                WDebug.LogError($"Unknown magic type: {magicData.type}");
                break;
        }
        cardNameText.text = await LocaleUtils.GetStringAsync("Card", cName);
    }
}
