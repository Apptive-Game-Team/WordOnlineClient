using UnityEngine;

namespace Scripts.CustomizeScene
{
    [System.Serializable]
    public class DecorationData
    {
        public long decorationId;
        public DecorationType type;
        public Sprite iconSprite;      
        public Sprite characterSprite; 
    }
}