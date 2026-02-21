using Scripts.Global.Button;

namespace Scripts.LobbyScene.Button
{
    public class CancelMatchingButton : ButtonBase
    {

        protected override void OnClickButton()
        {
            LobbySceneViewModel.Instance.RemoveFromQueue();
        }
    }
}