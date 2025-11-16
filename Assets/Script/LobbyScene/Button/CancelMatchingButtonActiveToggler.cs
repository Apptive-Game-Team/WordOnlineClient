using UnityEngine;

namespace Script.LobbyScene.Button
{
    public class CancelMatchingButtonActiveToggler : MonoBehaviour
    {
        
        [SerializeField] private GameObject buttonObject;
        
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
                    buttonObject.SetActive(false);
                    break;
                case LobbySceneViewModel.LobbyState.Matching:
                    buttonObject.SetActive(true);
                    break;
            }
        }
        
    }
}