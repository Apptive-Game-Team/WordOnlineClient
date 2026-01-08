using System.Collections.Generic;
using System.Linq;
using Script.GameScene.Dto;
using Script.GameScene.Object;
using Script.GameScene.UI;
using Script.Global;

namespace Script.GameScene.Handler
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
            // 플레이어 HP 업데이트 
            GameSceneUIController.Instance.UpdateUserHps(syncFrameInfo.leftPlayerHp, syncFrameInfo.rightPlayerHp);
            
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
 
            // 동기화
            ObjectSyncer.Instance.Sync(syncFrameInfo.snapshotResponseDto.objects);
            
            TimerController.Instance.UpdateTimer(syncFrameInfo.remainingTime);
        }
    }
}