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
        public static AudioClip FieldClick => ClickButton;
    
        // Game Magic
        private static AudioClip _shootSound;
        private static AudioClip _explosionSound;
        private static AudioClip _dropSound;
        private static AudioClip _windSound;
        private static AudioClip _fireExplosion;
        private static AudioClip _waterExplosion;
        private static AudioClip _natureExplosion;
        private static AudioClip _lightningExplosion;
        private static AudioClip _rockExplosion;

        public static AudioClip ShootSound => Load(ref _shootSound, "Sound/Game/Magic/shoot");
        public static AudioClip ExplosionSound => Load(ref _explosionSound, "Sound/Game/Magic/explosion");
        public static AudioClip DropSound => Load(ref _dropSound, "Sound/Game/Magic/drop");
        public static AudioClip WindSound => Load(ref _windSound, "Sound/Game/Magic/wind");
        public static AudioClip FireExplosion => Load(ref _fireExplosion, "Sound/Game/Magic/explode/fire_explode");
        public static AudioClip WaterExplosion => Load(ref _waterExplosion, "Sound/Game/Magic/explode/water_explode");
        public static AudioClip NatureExplosion => Load(ref _natureExplosion, "Sound/Game/Magic/explode/wind_explode");
        public static AudioClip LightningExplosion => Load(ref _lightningExplosion, "Sound/Game/Magic/explode/lightning_explode");
        public static AudioClip RockExplosion => Load(ref _rockExplosion, "Sound/Game/Magic/explode/rock_explode");

        private static AudioClip _hit;
        private static AudioClip _heal;
        private static AudioClip _arrowShot;

        public static AudioClip Hit => Load(ref _hit, "Sound/Game/hit");
        public static AudioClip Heal => Load(ref _heal, "Sound/Game/heal");
        public static AudioClip ArrowShot => Load(ref _arrowShot, "Sound/Game/arrow_shot");
    
        // Game Card
        private static AudioClip _drawCard;
        private static AudioClip _touchCard;

        public static AudioClip DrawCard => Load(ref _drawCard, "Sound/Game/Card/draw_card");
        public static AudioClip TouchCard => Load(ref _touchCard, "Sound/Game/Card/touch_card");
    }
}
