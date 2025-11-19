using System;
using Script.Global;
using UnityEngine;

namespace Script.LobbyScene
{
    public class LobbySceneViewModel : LocalSingletonObject<LobbySceneViewModel>
    {
        
        [SerializeField]
        private MatchQueueApiService _matchQueueApi;
        
        public enum LobbyState
        {
            Idle,
            Matching,
        }

        public StateEvent<LobbyState> CurrentState = new StateEvent<LobbyState>(LobbyState.Idle);
        
        public void Enqueue()
        {
            Debug.Log("Enqueue button clicked: Enqueueing player.");
            var newState = _matchQueueApi.Enqueue();
            CurrentState.UpdateData(newState);
        }
        
        public void RemoveFromQueue()
        {
            Debug.Log("Remove button clicked: Removing player.");
            StartCoroutine(_matchQueueApi.RemoveFromQueue());
            CheckIfInQueue();
        }
        
        public void CheckIfInQueue()
        {
            Debug.Log("Check button clicked: Checking player.");
            StartCoroutine(_matchQueueApi.IsMeInQueue(state =>
            {
                CurrentState.UpdateData(state);
            }));
        }
    }
}
