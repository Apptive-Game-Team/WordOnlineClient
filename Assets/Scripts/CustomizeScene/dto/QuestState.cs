using System;

namespace CustomizeScene.dto
{
    [Serializable]
    public class QuestState
    {
        public string state; // IN_PROGRESS, COMPLETED, CLAIMED
        public int progress;
        public int requireValue;
    }
}