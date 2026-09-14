using Global;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// 시전 입력. 손패에서 카드 한 장을 고르면 그 마법이 정해지므로 마법 id 하나와 위치만 보낸다.
    /// </summary>
    [System.Serializable]
    public class CardUseInput
    {
        public string type = "useMagic";
        public long magicId;
        public int id = IDMaker.GetCardUseInputID();
        public Vector3 position;

        public CardUseInput(long magicId, Vector3 pos)
        {
            this.magicId = magicId;
            position = pos;
        }
    }

    /// <summary>
    /// 카드를 고른 순간 보낸다. 서버가 그 마법의 원소로 시전자에게 idle aura 를 붙인다.
    /// </summary>
    [System.Serializable]
    public class CardSelectRequestDto
    {
        public string type = "selectCard";
        public long magicId;
        public int id = IDMaker.GetCardUseInputID();

        public CardSelectRequestDto(long magicId)
        {
            this.magicId = magicId;
        }
    }

    [System.Serializable]
    public class CardUnselectRequestDto
    {
        public string type = "unselectCard";
        public long magicId;
        public int id = IDMaker.GetCardUseInputID();

        public CardUnselectRequestDto(long magicId)
        {
            this.magicId = magicId;
        }
    }
}
