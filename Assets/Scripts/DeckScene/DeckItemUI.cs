using TMPro;
using UnityEngine;

namespace Scripts.DeckScene
{
    public class DeckItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        public void Init(string cName)
        {
            cardNameText.text = cName;
        }
    }
}
