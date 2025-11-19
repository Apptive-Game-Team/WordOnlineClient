using Script.Global;

namespace Script.LobbyScene.Button
{
    public class CancelMatchingButton : ButtonBase
    {

        protected override void OnClickButton()
        {
            LobbySceneViewModel.Instance.RemoveFromQueue();
        }
    }
}