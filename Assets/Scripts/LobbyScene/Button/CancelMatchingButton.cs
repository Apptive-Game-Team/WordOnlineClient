using Global.Button;

namespace LobbyScene.Button
{
    public class CancelMatchingButton : ButtonBase
    {

        protected override void OnClickButton()
        {
            LobbySceneViewModel.Instance.RemoveFromQueue();
        }
    }
}