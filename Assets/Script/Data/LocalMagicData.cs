using System.Collections.Generic;

namespace Script.Data
{
    public enum CardType
    {
        Dummy,
        Shoot,
        Drop,
        Build,
        Spawn,
        Explode,
        Fire,
        Water,
        Lightning,
        Rock,
        Nature,
        Wind,
    }
    public class MagicData
    {
        public string name;
        public int mana;
        public int range;
        public float radius;
        public string type;

        public MagicData(string name, int mana, int range, float radius, string type)
        {
            this.name = name;
            this.mana = mana;
            this.range = range;
            this.radius = radius;
            this.type = type;
        }
    }

    public static class LocalMagicData
    {
        public static List<MagicData> dataList = new List<MagicData>()
        {
            new MagicData("Shoot",15,18, 0.5f, "magic"),
            new MagicData("Build",30,6, 0.5f,"magic"),
            new MagicData("Spawn",20,6, 0f,"magic"),
            new MagicData("Explode",10,9, 0.5f,"magic"),
            new MagicData("Fire",10,6, 0.5f,"type"),
            new MagicData("Water",10,6, 0.5f,"type"),
            new MagicData("Nature",10,6, 0.5f,"type"),
            new MagicData("Lightning",10,6, 0.5f,"type"),
            new MagicData("Rock",10,6, 0.5f,"type"),
            new MagicData("Wind",10,6, 0.5f,"type"),
            
            new MagicData("Drop",10,18, 0.5f,"magic"),
        };

        public static MagicData GetMagicData(string name)
        {
            return dataList.Find(x => x.name == name);
        }
    }
}