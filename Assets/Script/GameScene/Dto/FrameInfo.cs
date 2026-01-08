[System.Serializable]
public class FrameInfoDto
{
    public string type;

    public int remainingTime;
    
    public int updatedMana;
    public int leftPlayerHp;
    public int rightPlayerHp;
    public CardInfo cards;
    public ObjectsInfo objects;
}