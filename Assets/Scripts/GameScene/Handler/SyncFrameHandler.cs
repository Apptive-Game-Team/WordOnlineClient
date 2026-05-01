using System;
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
            
            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(syncFrameInfo.updatedMana);
            
            // // 카드 추가
            try
            {
                var currentCounts = GameSceneUIController.Instance.GetAllCards()
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

                var targetCounts = syncFrameInfo.snapshotResponseDto.myCards
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 모든 카드 종류 추출 (현재 + 목표)
                var allCardTypes = currentCounts.Keys.Union(targetCounts.Keys);

                foreach (var card in allCardTypes)
                {
                    currentCounts.TryGetValue(card, out int currentCount);
                    targetCounts.TryGetValue(card, out int targetCount);

                    int diff = targetCount - currentCount;

                    if (diff > 0) // 추가 필요
                    {
                        for (int i = 0; i < diff; i++) GameSceneUIController.Instance.AddCard(card);
                    }
                    else if (diff < 0) // 삭제 필요
                    {
                        for (int i = 0; i < Math.Abs(diff); i++) GameSceneUIController.Instance.RemoveCard(card);
                    }
                }
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