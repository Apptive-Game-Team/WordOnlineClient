using Script.GameScene.Object;
using Script.Global;

namespace Script.GameScene.Handler
{
    public class DeltaFrameHandler : IFrameInfoHandler<FrameInfoDto>
    {
        public void Handler(FrameInfoDto data)
        {
            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(data.updatedMana);
            // 플레이어 HP 업데이트 
            GameSceneUIController.Instance.UpdateUserHps(data.leftPlayerHp, data.rightPlayerHp);
            //
            // // 카드 추가
            foreach (string cardName in data.cards.added)
            {
                GameSceneUIController.Instance.AddCard(cardName);
            }
                    
            //
            // // 생성된 오브젝트 배치
            foreach (var created in data.objects.create)
                ObjectSpawner.Instance.SpawnObject(created);
            
            foreach (var projectile in data.objects.projectile)
            {
                ProjectileSpawner.Instance.Spawn(projectile);
            }
        
            // // 기존 오브젝트 업데이트
            foreach (var updated in data.objects.update)
                ObjectUpdater.Instance.UpdateObject(updated);
        }
    }
}