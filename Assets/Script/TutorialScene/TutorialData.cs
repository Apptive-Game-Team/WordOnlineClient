using System;
using UnityEngine;

namespace Script.TutorialScene
{
    [CreateAssetMenu(menuName = "Game/Tutorial/TutorialData")]
    public class TutorialData : ScriptableObject
    {
        public string stringTableName = "tutorial";
        public TutorialStep[] steps;
        public string lobbySceneName = "LobbyScene";
    }

    [Serializable]
    public class TutorialStep
    {
        public string localizationKey;
        public string[] cardNames;
        public bool shouldClearCards;
        public TutorialWaitType waitType;
    }

    public enum TutorialWaitType
    {
        Next,
        UsedShotFire,
        UsedWaterArcher,
        UsedAnyCard,
        EnemyDead
    }
}