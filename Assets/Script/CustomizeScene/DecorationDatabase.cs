using System.Collections.Generic;
using UnityEngine;

public enum DecorationType
{
    Hat,
    Cape,
}

[System.Serializable]
public class DecorationData
{
    public long decorationId;
    public DecorationType type;
    public Sprite iconSprite;      
    public Sprite characterSprite; 
}

[CreateAssetMenu(menuName = "Game/DecorationDB")]
public class DecorationDatabase : ScriptableObject
{
    public DecorationData[] datas;

    public DecorationData Find(long id)
    {
        foreach (var m in datas)
        {
            if (m.decorationId == id) return m;
        }
        return null;
    }

    public IEnumerable<DecorationData> GetByType(DecorationType type)
    {
        foreach (var m in datas)
        {
            if (m.type == type) yield return m;
        }
    }
}