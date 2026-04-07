using System.Collections.Generic;

namespace GameScene.Dto
{
    [System.Serializable]
    public class PveScriptEventInfo
    {
        public string type;
        public string key;
        public int speakerObjectId;
        public List<string> lines;
    }
}