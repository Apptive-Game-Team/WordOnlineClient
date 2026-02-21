namespace Data
{
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
}