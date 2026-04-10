using System.Collections.Generic;
using System.Linq;
using GameScene.Dto;
using GameScene.Object;
using GameScene.UI;
using Global;

namespace GameScene.Handler
{
    public class SyncFrameHandler : IFrameInfoHandler<SyncFrameInfo>
    {
        public void Handler(SyncFrameInfo syncFrameInfo)
        {
            if (syncFrameInfo == null)
            {
                WDebug.LogError("[SyncFrameHandler] syncFrameInfo is null");
                return;
            }

            FrameClock.SyncTo(syncFrameInfo.frameNum);

            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(syncFrameInfo.updatedMana);
            
            // // 카드 추가
            try
            {
                List<string> existedCards = GameSceneUIController.Instance.GetAllCards();
                List<string> cardData = syncFrameInfo.snapshotResponseDto.myCards.ToList<string>();
                existedCards.ForEach(x => cardData.Remove(x));
                cardData.ForEach(x => GameSceneUIController.Instance.AddCard(x));
            }
            catch
            {
                WDebug.Log("[SyncFrameHandler] 카드 추가 중 오류 발생");
            }
 
            foreach (var projectile in syncFrameInfo.projectileDtos)
            {
                ProjectileSpawner.Instance.Spawn(projectile);
            }
            
            // 동기화
            ObjectSyncer.Instance.Sync(syncFrameInfo.snapshotResponseDto.objects);
            
            TimerController.Instance.UpdateTimer(syncFrameInfo.remainingTime);
        }
    }
}