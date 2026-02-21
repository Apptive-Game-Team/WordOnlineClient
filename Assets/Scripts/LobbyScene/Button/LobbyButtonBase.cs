using Scripts.Global.Button;

namespace Scripts.LobbyScene.Button
{
    public abstract class LobbyButtonBase : DisableableButtonBase
    {
        private void Start()
        {
            LobbySceneViewModel.Instance.CurrentState.OnStateChange += OnStatusChanged;
        }
        
        private void OnDestroy()
        {
            LobbySceneViewModel.Instance.CurrentState.OnStateChange -= OnStatusChanged;
        }
        
        private void OnStatusChanged(LobbySceneViewModel.LobbyState state)
        {
            switch (state)
            {
                case LobbySceneViewModel.LobbyState.Idle:
                    ResetButton();
                    break;
                case LobbySceneViewModel.LobbyState.Matching:
                    SetButton();
                    break;
            }
        }
    }
}