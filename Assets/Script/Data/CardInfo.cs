using Script.Data;

[System.Serializable]
public class CardInfo
{
    public CardType[] added; // enum CardType과 매칭되도록 서버에서 대문자 스트링으로 옴
}