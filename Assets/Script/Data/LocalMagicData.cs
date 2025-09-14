using System.Collections.Generic;

namespace Script.Data
{
    public enum CardType
    {
        Dummy,
        Shoot,
        Drop,
        Summon,
        Spawn,
        Explode,
        Fire,
        Water,
        Lightning,
        Rock,
        Leaf,
        Wind,
    }
    public class MagicData
    {
        public string name;
        public int mana;
        public float range;
        public string type;

        public MagicData(string name, int mana, float range, string type)
        {
            this.name = name;
            this.mana = mana;
            this.range = range;
            this.type = type;
        }
    }

    public static class LocalMagicData
    {
        public static List<MagicData> dataList = new List<MagicData>()
        {
            new MagicData("Shoot",15,1f, "magic"),
            new MagicData("Summon",30,1/3f, "magic"),
            new MagicData("Spawn",20,1/3f, "magic"),
            new MagicData("Explode",10,1/2f, "magic"),
            new MagicData("Fire",10,1/4f, "type"),
            new MagicData("Water",10,1/4f, "type"),
            new MagicData("Leaf",10,1/4f, "type"),
            new MagicData("Lightning",10,1/4f, "type"),
            new MagicData("Rock",10,1/4f, "type"),
            new MagicData("Wind",10,1/4f, "type"),
        };

        public static MagicData GetMagicData(string name)
        {
            return dataList.Find(x => x.name == name);
        }
    }
}