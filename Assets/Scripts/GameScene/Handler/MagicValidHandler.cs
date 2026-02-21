using GameScene.Card;
using GameScene.Dto;
using Global;

namespace GameScene.Handler
{
    public class MagicValidHandler : IFrameInfoHandler<MagicValidInfo>
    {
        public void Handler(MagicValidInfo magicValid)
        {
            if (magicValid.valid)
            {
                foreach (CardUI cardUI in CardInputSender.inputRequestDict[magicValid.id])
                {
                    cardUI.Destroy();
                }

                if (magicValid.magicId == -1)
                {
                    // 마법 파사삭
                    GameSceneUIController.Instance.PlayMagicFailEffect();
                }
            }
            else
            {
                foreach (var cardUI in CardInputSender.inputRequestDict[magicValid.id])
                {
                    cardUI.SetCardActive(false);
                }
                SystemMessageUI.Instance.ShowMessage(magicValid.message);
                WDebug.Log("[Magic Valid]" + magicValid.message);
            }
            CardInputSender.inputRequestDict.Remove(magicValid.id);
        }
    }
}