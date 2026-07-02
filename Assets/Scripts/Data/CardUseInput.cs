using System.Collections.Generic;
using Global;
using UnityEngine;

namespace Data
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

    [System.Serializable]
    public class CardSelectRequestDto
    {
        public string type = "selectCard";
        public string card;
        public int id = IDMaker.GetCardUseInputID();

        public CardSelectRequestDto(CardType card)
        {
            this.card = card.ToString();
        }
    }

    [System.Serializable]
    public class CardUnselectRequestDto
    {
        public string type = "unselectCard";
        public string card;
        public int id = IDMaker.GetCardUseInputID();

        public CardUnselectRequestDto(CardType card)
        {
            this.card = card.ToString();
        }
    }
}
