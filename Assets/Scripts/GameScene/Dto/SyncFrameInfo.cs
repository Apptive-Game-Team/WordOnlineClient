using System.Collections.Generic;
using GameScene.Dto.Projectile;

namespace GameScene.Dto
{
    [System.Serializable]
    public class SyncFrameInfo
    {
        public string type;
        public int frameNum;

        public int remainingTime;

        public int updatedMana;
        public int leftPlayerHp;
        public int rightPlayerHp;
        public SnapshotDto snapshotResponseDto;
        public List<ProjectileDto> projectileDtos;
    }
}