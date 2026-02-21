using System.Collections.Generic;
using Scripts.GameScene.Dto.Projectile;

namespace Scripts.GameScene.Dto
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