using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class UpdatedObjectDto
    {
        public int id;
        public Vector3 position;
        public int hp;
        public int maxHp;
        public string status;
        public string effect;

        public UpdatedObjectDto() { }
    }
}
