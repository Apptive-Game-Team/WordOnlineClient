using GameScene.Dto;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class UpdatedObjectDto
    {
        public int id;
        public Vector3 position;
        public int hp;
        public int maxHp;
        public string status;
        public string effect;

        public UpdatedObjectDto() { }

        public UpdatedObjectDto(SnapshotObjectDto snapshotObjectDto)
        {
            id = snapshotObjectDto.id;
            position = new Vector3(snapshotObjectDto.x, snapshotObjectDto.y, snapshotObjectDto.z);
            hp = snapshotObjectDto.hp;
            maxHp = snapshotObjectDto.maxHp;
            status = snapshotObjectDto.status;
            effect = snapshotObjectDto.effect;
        }
    }
}