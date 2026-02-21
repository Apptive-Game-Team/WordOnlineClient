using System;

namespace Scripts.CustomizeScene.dto
{
    [Serializable]
    public class DecorationRequest
    {
        public long decorationId;
        public DecorationRequest(long id) { decorationId = id; }
    }
}