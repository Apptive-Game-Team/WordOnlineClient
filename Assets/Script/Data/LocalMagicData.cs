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
            new MagicData("Shoot",25,1f, "magic"),
            new MagicData("Summon",15,1/3f, "magic"),
            new MagicData("Spawn",15,1/3f, "magic"),
            new MagicData("Explode",40,1/2f, "magic"),
            new MagicData("Fire",0,1/4f, "type"),
            new MagicData("Water",0,1/4f, "type"),
            new MagicData("Leaf",0,1/4f, "type"),
            new MagicData("Lightning",0,1/4f, "type"),
            new MagicData("Rock",0,1/4f, "type"),
        };

        public static MagicData GetMagicData(string name)
        {
            return dataList.Find(x => x.name == name);
        }
    }
}