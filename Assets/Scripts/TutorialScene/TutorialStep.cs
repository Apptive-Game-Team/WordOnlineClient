using System;

namespace Scripts.TutorialScene
{
    [Serializable]
    public class TutorialStep
    {
        public string localizationKey;
        public string[] cardNames;
        public bool shouldClearCards;
        public TutorialWaitType waitType;
    }
}