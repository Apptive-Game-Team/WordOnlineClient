using System;
using UnityEngine;

namespace GameScene.Dto
{
    [Serializable]
    public class BotThoughtInfo : ServerMessage
    {
        public string botSide;
        public string ruleId;
        public string reason;
        public string[] cards;
        public Vector3 target;
    }
}
