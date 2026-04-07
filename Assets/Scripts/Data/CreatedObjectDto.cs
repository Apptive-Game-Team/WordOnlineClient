using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class CreatedObjectDto
    {
        public int id;
        public string master;
        public Vector3 position;
        public string type;

        public CreatedObjectDto() { }
    }
}
