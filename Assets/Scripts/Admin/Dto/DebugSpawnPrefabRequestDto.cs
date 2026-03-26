using System;
using UnityEngine;

namespace Admin.Dto
{
    [Serializable]
    public class DebugSpawnPrefabRequestDto
    {
        public string sessionId;
        public string master;
        public string prefabId;
        public Vector3 position;
    }
}
