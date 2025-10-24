using Script.GameScene.Dto;
using Script.GameScene.Object;

namespace Script.GameScene.Handler
{
    public class SyncFrameHandler : IFrameInfoHandler<SyncFrameInfo>
    {
        public void Handler(SyncFrameInfo syncFrameInfo)
        {
            // 마나 UI 업데이트
            GameSceneUIController.Instance.UpdateMana(syncFrameInfo.updatedMana);
            // 플레이어 HP 업데이트 
            GameSceneUIController.Instance.UpdateUserHps(syncFrameInfo.leftPlayerHp, syncFrameInfo.rightPlayerHp);
            
            // // 카드 추가
            foreach (string cardName in syncFrameInfo.snapshotResponseDto.myCards)
            {
                GameSceneUIController.Instance.AddCard(cardName);
            }
                    
            // 동기화
            ObjectSyncer.Instance.Sync(syncFrameInfo.snapshotResponseDto.objects);
        }
    }
}