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
        public static AudioClip ClickButton => Load(ref _clickButton, "Sound/UI/wood_button_click_v3");
    
        // Game Magic
        private static AudioClip _shootSound;
        private static AudioClip _explosionSound;

        public static AudioClip ShootSound => Load(ref _shootSound, "Sound/Game/Magic/shoot");
        public static AudioClip ExplosionSound => Load(ref _explosionSound, "Sound/Game/Magic/explosion");
    
        // Game Card
        private static AudioClip _drawCard;
        private static AudioClip _touchCard;

        public static AudioClip DrawCard => Load(ref _drawCard, "Sound/Game/Card/draw_card");
        public static AudioClip TouchCard => Load(ref _touchCard, "Sound/Game/Card/card_touch_v2");

        private static AudioClip _fieldConfirm;
        private static AudioClip _creatureSpawn;
        private static AudioClip _buildingSpawn;
        private static AudioClip _creatureMove;
        private static AudioClip _genericAttack;
        private static AudioClip _creatureHit;
        private static AudioClip _buildingHit;
        private static AudioClip _heal;
        private static AudioClip _creatureDeath;
        private static AudioClip _buildingDeath;

        public static AudioClip FieldConfirm => Load(ref _fieldConfirm, "Sound/Game/field_confirm");
        public static AudioClip CreatureSpawn => Load(ref _creatureSpawn, "Sound/Game/Lifecycle/creature_spawn");
        public static AudioClip BuildingSpawn => Load(ref _buildingSpawn, "Sound/Game/Lifecycle/building_spawn");
        public static AudioClip CreatureMove => Load(ref _creatureMove, "Sound/Game/Lifecycle/creature_move");
        public static AudioClip GenericAttack => Load(ref _genericAttack, "Sound/Game/Lifecycle/generic_attack");
        public static AudioClip CreatureHit => Load(ref _creatureHit, "Sound/Game/Lifecycle/creature_hit");
        public static AudioClip BuildingHit => Load(ref _buildingHit, "Sound/Game/Lifecycle/building_hit");
        public static AudioClip Heal => Load(ref _heal, "Sound/Game/Lifecycle/heal");
        public static AudioClip CreatureDeath => Load(ref _creatureDeath, "Sound/Game/Lifecycle/creature_death");
        public static AudioClip BuildingDeath => Load(ref _buildingDeath, "Sound/Game/Lifecycle/building_death");

        private static AudioClip _fireAccent;
        private static AudioClip _waterAccent;
        private static AudioClip _natureAccent;
        private static AudioClip _lightningAccent;
        private static AudioClip _rockAccent;
        private static AudioClip _windAccent;

        public static AudioClip FireAccent => Load(ref _fireAccent, "Sound/Game/Element/fire_accent");
        public static AudioClip WaterAccent => Load(ref _waterAccent, "Sound/Game/Element/water_accent");
        public static AudioClip NatureAccent => Load(ref _natureAccent, "Sound/Game/Element/nature_accent");
        public static AudioClip LightningAccent => Load(ref _lightningAccent, "Sound/Game/Element/lightning_accent");
        public static AudioClip RockAccent => Load(ref _rockAccent, "Sound/Game/Element/rock_accent");
        public static AudioClip WindAccent => Load(ref _windAccent, "Sound/Game/Element/wind_accent");
    }
}
