using GameScene.Dto;
using Global;

namespace GameScene.Handler
{
    public class BotThoughtHandler : IFrameInfoHandler<BotThoughtInfo>
    {
        public void Handler(BotThoughtInfo thought)
        {
            if (thought == null)
            {
                return;
            }

            string summary = $"[BotThought:{thought.botSide}] {thought.reason} ({thought.ruleId})";
            string cards = thought.cards == null ? "" : string.Join(",", thought.cards);
            WDebug.Log($"{summary} cards=[{cards}] target={thought.target}");

            if (SystemMessageUI.Instance != null)
            {
                SystemMessageUI.Instance.ShowMessage(summary);
            }
        }
    }
}
