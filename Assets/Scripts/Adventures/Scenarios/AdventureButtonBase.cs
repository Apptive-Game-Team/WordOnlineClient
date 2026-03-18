using Global.Button;

namespace Adventures.Scenarios
{
    public abstract class AdventureButtonBase : DisableableButtonBase
    {
        private void Start()
        {
            AdventureViewModel.Instance.CurrentState.OnStateChange += OnStatusChanged;
        }
        
        private void OnDestroy()
        {
            AdventureViewModel.Instance.CurrentState.OnStateChange -= OnStatusChanged;
        }
        
        private void OnStatusChanged(AdventureViewModel.AdventureState state)
        {
            switch (state)
            {
                case AdventureViewModel.AdventureState.Idle:
                    ResetButton();
                    break;
                case AdventureViewModel.AdventureState.Requesting:
                    SetButton();
                    break;
            }
        }
    }
}