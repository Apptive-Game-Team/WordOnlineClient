using System;

namespace Script.GameScene.Dto
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
    }
}