[System.Serializable]
public class DecorationDto
{
    public long decorationId;
    public string name;
    public bool isEquipped;
}

[System.Serializable]
public class DecorationsResponse
{
    public DecorationDto[] decorations;
}