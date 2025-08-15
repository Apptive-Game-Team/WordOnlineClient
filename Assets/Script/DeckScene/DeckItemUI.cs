using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;

    string[] typeArray = { "fire", "water", "lightning", "rock", "leaf" };
    string[] magicArray = { "shoot", "spawn", "summon", "explode" };
    [SerializeField] private Sprite typeSprite;
    [SerializeField] private Sprite magicSprite;
    public void Init(string cName)
    {
        cardNameText.text = cName;
        if (typeArray.Contains(cName))
        {
            GetComponent<Image>().sprite = typeSprite;
        }
        else if (magicArray.Contains(cName))
        {
            GetComponent<Image>().sprite = magicSprite;
        }
    }
}
