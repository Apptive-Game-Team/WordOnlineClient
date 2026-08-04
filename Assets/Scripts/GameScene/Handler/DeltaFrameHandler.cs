using GameScene.Dto;
using GameScene.Object;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Effect;
using GameScene.UI;

namespace GameScene.Handler
{
    public class DeltaFrameHandler : IFrameInfoHandler<FrameInfoDto>
    {
        public void Handler(FrameInfoDto data)
        {
            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(data.updatedMana);
            
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
        
            // 이벤트 처리 - HP 변화로 이펙트가 재생되기 전에 방향을 먼저 넘겨준다
            if (data.events != null)
            {
                foreach (var gameEvent in data.events)
                {
                    if (gameEvent.type != GameEventDto.Hit) continue;

                    ServedObject attacker = ObjectContainer.Instance.FindById(gameEvent.actorId);
                    ServedObject target = ObjectContainer.Instance.FindById(gameEvent.targetId);
                    if (attacker == null || target == null) continue;

                    HitEffectController.NotifyHit(attacker, target);
                }
            }

            // // 기존 오브젝트 업데이트
            foreach (var updated in data.objects.update)
                ObjectUpdater.Instance.UpdateObject(updated);
            
            TimerController.Instance.UpdateTimer(data.remainingTime);
        }
    }
}