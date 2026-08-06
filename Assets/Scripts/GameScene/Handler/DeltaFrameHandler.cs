using GameScene.Dto;
using GameScene.Dto.Projectile;
using GameScene.Object;
using GameScene.UI;

namespace GameScene.Handler
{
    public class DeltaFrameHandler : IFrameInfoHandler<FrameInfoDto>
    {
        public void Handler(FrameInfoDto data)
        {
            if (data == null)
            {
                return;
            }

            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(data.updatedMana);

            // 카드 추가
            if (data.cards?.added != null)
            {
                foreach (string cardName in data.cards.added)
                {
                    GameSceneUIController.Instance.AddCard(cardName);
                }
            }

            if (data.objects != null)
            {
                // 생성된 오브젝트 배치
                if (data.objects.create != null)
                {
                    foreach (CreatedObjectDto created in data.objects.create)
                    {
                        ObjectSpawner.Instance.SpawnObject(created);
                    }
                }

                if (data.objects.projectile != null)
                {
                    foreach (ProjectileDto projectile in data.objects.projectile)
                    {
                        ProjectileSpawner.Instance.Spawn(projectile);
                    }
                }

                // 기존 오브젝트 업데이트
                if (data.objects.update != null)
                {
                    foreach (UpdatedObjectDto updated in data.objects.update)
                    {
                        ObjectUpdater.Instance.UpdateObject(updated);
                    }
                }
            }

            TimerController.Instance.UpdateTimer(data.remainingTime);
        }
    }
}
