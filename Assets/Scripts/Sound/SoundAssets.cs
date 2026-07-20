using UnityEngine;

namespace Sound
{
    public abstract class SoundAssets
    {
        private static AudioClip Load(ref AudioClip clip, string path)
        {
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(path);
            }

            return clip;
        }

        // UI
        private static AudioClip _clickButton;
        public static AudioClip ClickButton => Load(ref _clickButton, "Sound/UI/wood_button_click");
    
        // Game Magic
        private static AudioClip _shootSound;
        private static AudioClip _explosionSound;

        public static AudioClip ShootSound => Load(ref _shootSound, "Sound/Game/Magic/shoot");
        public static AudioClip ExplosionSound => Load(ref _explosionSound, "Sound/Game/Magic/explosion");
    
        // Game Card
        private static AudioClip _drawCard;
        private static AudioClip _touchCard;

        public static AudioClip DrawCard => Load(ref _drawCard, "Sound/Game/Card/draw_card");
        public static AudioClip TouchCard => Load(ref _touchCard, "Sound/Game/Card/touch_card");
    }
}
