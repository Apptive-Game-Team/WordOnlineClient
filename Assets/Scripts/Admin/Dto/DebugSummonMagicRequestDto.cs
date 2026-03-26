using System;
using UnityEngine;

namespace Admin.Dto
{
    [Serializable]
    public class DebugSummonMagicRequestDto
    {
        public string sessionId;
        public string master;
        public int magicId;
        public Vector3 position;
    }
}
