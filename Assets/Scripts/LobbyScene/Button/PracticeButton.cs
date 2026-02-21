using System;

namespace Scripts.LobbyScene.Button
{
    public class PracticeButton : LobbyButtonBase
    {
        protected override void OnClickButton()
        {
            if (!isActive)
            {
                return;
            }
        
            try
            {
                LobbySceneViewModel.Instance.PlayPracticeMatch();
                SetButton();
            }
            catch (Exception)
            {
                ResetButton();
            }
        }
    }
}
