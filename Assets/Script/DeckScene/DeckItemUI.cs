using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    public void Init(string cName)
    {
        cardNameText.text = cName;
    }
}
