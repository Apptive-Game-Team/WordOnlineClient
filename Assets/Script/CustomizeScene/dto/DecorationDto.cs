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

[System.Serializable]
public class DetailDecorationDto
{
    public long decorationId;
    public string name;
    public bool isEquipped;
    public bool unlocked;
    public string unlockText;
    public string progressText;
}

[System.Serializable]
public class DetailDecorationsResponse
{
    public DetailDecorationDto[] decorations;
}