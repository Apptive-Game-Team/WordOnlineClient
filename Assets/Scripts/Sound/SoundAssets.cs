using UnityEngine;

namespace Scripts.Sound
{
    public abstract class SoundAssets
    {
        // UI
        public static AudioClip _clickButton = null;
        public static AudioClip ClickButton
        {
            get
            {
                if (_clickButton == null)
                {
                    _clickButton = Resources.Load<AudioClip>("Sound/UI/wood_button_click");
                }
                return _clickButton;
            }
        }
    
        // Game Magic
        private static AudioClip _shootSound = null;

        public static AudioClip ShootSound
        {
            get
            {
                if (_shootSound == null)
                {
                    _shootSound = Resources.Load<AudioClip>("Sound/Game/Magic/shoot");
                }
                return _shootSound;
            }
        }
        private static AudioClip _explosionSound = null;
        public static AudioClip ExplosionSound
        {
            get 
            {
                if (_explosionSound == null)
                {
                    _explosionSound = Resources.Load<AudioClip>("Sound/Game/Magic/explosion");
                }
                return _explosionSound;
            }
        }
    
        // Game Card
        private static AudioClip _drawCard = null;
        public static AudioClip DrawCard
        {
            get 
            {
                if (_drawCard == null)
                {
                    _drawCard = Resources.Load<AudioClip>("Sound/Game/Card/draw_card");
                }
                return _drawCard;
            }
        }
    }
}
