using GameScene.Dto;
using Global;

namespace GameScene.Handler
{
    public class PveScriptEventHandler : IFrameInfoHandler<PveScriptEventInfo>
    {
        public void Handler(PveScriptEventInfo pveScriptEvent)
        {
            if (pveScriptEvent == null)
            {
                return;
            }

            if (pveScriptEvent.lines != null && pveScriptEvent.lines.Count > 0)
            {
                foreach (string line in pveScriptEvent.lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        SystemMessageUI.Instance.ShowMessage(line);
                    }
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(pveScriptEvent.key))
            {
                SystemMessageUI.Instance.ShowMessage(pveScriptEvent.key);
            }
        }
    }
}