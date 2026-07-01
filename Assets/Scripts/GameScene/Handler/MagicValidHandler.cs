using GameScene.Card;
using GameScene.Dto;

namespace GameScene.Handler
{
    public class MagicValidHandler : IFrameInfoHandler<MagicValidInfo>
    {
        public void Handler(MagicValidInfo magicValid)
        {
            CardInputSender.Instance.HandleInputResponse(magicValid);
        }
    }
}
