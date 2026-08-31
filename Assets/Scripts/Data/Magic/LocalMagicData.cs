using System.Collections.Generic;

namespace Data.Magic
{
    public static class LocalMagicData
    {
        public static List<MagicData> dataList = new List<MagicData>()
        {
            new MagicData("Shoot",15,18, 0.5f, "magic"),
            new MagicData("Build",20,6, 0.5f,"magic"),
            new MagicData("Spawn",20,6, 0f,"magic"),
            new MagicData("Explode",10,9, 0.5f,"magic"),
            new MagicData("Fire",10,6, 0.5f,"type"),
            new MagicData("Water",10,6, 0.5f,"type"),
            new MagicData("Nature",10,6, 0.5f,"type"),
            new MagicData("Lightning",10,6, 0.5f,"type"),
            new MagicData("Rock",10,6, 0.5f,"type"),
            new MagicData("Wind",10,6, 0.5f,"type"),
            
            new MagicData("Drop",10,14, 0.5f,"magic"),
        };

        public static MagicData GetMagicData(string name)
        {
            return dataList.Find(x => x.name == name);
        }
    }
}