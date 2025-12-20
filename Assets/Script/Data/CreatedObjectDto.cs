using Script.GameScene.Dto;
using UnityEngine;

[System.Serializable]
public class CreatedObjectDto
{
    public int id;
    public string master;
    public Vector3 position;
    public string type; // enum 대응 가능

    public CreatedObjectDto()
    {
        
    }
    
    public CreatedObjectDto(SnapshotObjectDto objectDto)
    {
        id = objectDto.id;
        master = objectDto.master;
        position = new Vector3(objectDto.x, objectDto.y, objectDto.z);
        type = objectDto.prefab;
    }
}