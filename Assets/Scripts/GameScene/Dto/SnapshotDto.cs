using System;

namespace GameScene.Dto
{
    [Serializable]
    public class SnapshotDto
    {
        public int frame;
        public SnapshotObjectDto[] objects;
        public string[] myCards;
    }
}