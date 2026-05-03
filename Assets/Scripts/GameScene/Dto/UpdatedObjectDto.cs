using System.Collections.Generic;
using GameScene.Dto;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class UpdatedObjectDto
    {
        public int id;
        public Vector3 position;
        public string master;
        public string status;
        public string effect;
        public List<Gauge> gauges;
        
        public UpdatedObjectDto() { }

        public UpdatedObjectDto(SnapshotObjectDto snapshotObjectDto)
        {
            id = snapshotObjectDto.id;
            position = new Vector3(snapshotObjectDto.x, snapshotObjectDto.y, snapshotObjectDto.z);
            status = snapshotObjectDto.status;
            effect = snapshotObjectDto.effect;
            gauges = snapshotObjectDto.gauges;
        }
    }
}
