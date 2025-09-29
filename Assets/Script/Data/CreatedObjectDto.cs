using UnityEngine;

[System.Serializable]
public class CreatedObjectDto
{
    public int id;
    public string master;
    public Vector3 position;
    public string type; // enum 대응 가능
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}