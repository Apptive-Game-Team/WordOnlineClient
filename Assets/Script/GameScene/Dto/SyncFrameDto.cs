using System.Collections.Generic;
using Script.GameScene.Dto.Projectile;

namespace Script.GameScene.Dto
{
    [System.Serializable]
    public class SyncFrameInfo
    {
        public string type;
        
        public int remainingTime;
        
        public int updatedMana;
        public int leftPlayerHp;
        public int rightPlayerHp;
        public SnapshotDto snapshotResponseDto;
        public List<ProjectileDto> projectileDtos;
    }
}