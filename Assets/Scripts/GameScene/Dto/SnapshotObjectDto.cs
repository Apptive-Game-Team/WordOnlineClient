using System;
using System.Collections.Generic;
using GameScene.Dto.debug;

namespace GameScene.Dto
{
    [Serializable]
    public class SnapshotObjectDto
    {
        public int id;
        public string prefab;
        public float x;
        public float y;
        public float z;
        public string master;
        public string status;
        public string effect;
        public int hp;
        public int maxHp;
        public List<Gizmo> gizmos;
    }
}