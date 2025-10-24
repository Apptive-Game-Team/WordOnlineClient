using System;

namespace Script.GameScene.Dto
{
    [Serializable]
    public class SnapshotDto
    {
        public int frame;
        public SnapshotObjectDto[] objects;
        public string[] myCards;
    }
}