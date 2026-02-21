using System.Collections.Generic;
using Scripts.Global;
using UnityEngine;

namespace Scripts.Data
{
    [System.Serializable]
    public class CardUseInput
    {
        public string type = "useMagic";
        public List<string> cards;
        public int id = IDMaker.GetCardUseInputID();
        public Vector3 position;
        
        public CardUseInput(List<string> selectedCards, Vector3 pos)
        {
            cards = selectedCards;
            position = pos;
        }
    }

}