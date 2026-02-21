using UnityEngine;

namespace Scripts.TutorialScene
{
    [CreateAssetMenu(menuName = "Game/Tutorial/TutorialData")]
    public class TutorialData : ScriptableObject
    {
        public string stringTableName = "tutorial";
        public TutorialStep[] steps;
        public string lobbySceneName = "LobbyScene";
    }
}