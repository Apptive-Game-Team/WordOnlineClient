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
        public static AudioClip ClickButton => Load(ref _clickButton, "Sound/UI/wood_button_click_v4");
    
        // Game Magic
        private static AudioClip _shootSound;
        private static AudioClip _explosionSound;

        public static AudioClip ShootSound => Load(ref _shootSound, "Sound/Game/Magic/shoot");
        public static AudioClip ExplosionSound => Load(ref _explosionSound, "Sound/Game/Magic/explosion");
    
        // Game Card
        private static AudioClip _drawCard;
        private static AudioClip _cardHover;
        private static AudioClip _cardSelect;
        private static AudioClip _cardDeselect;

        public static AudioClip DrawCard => Load(ref _drawCard, "Sound/Game/Card/draw_card");
        public static AudioClip CardHover => Load(ref _cardHover, "Sound/Game/Card/card_hover_v2");
        public static AudioClip CardSelect => Load(ref _cardSelect, "Sound/Game/Card/card_select_v1");
        public static AudioClip CardDeselect => Load(ref _cardDeselect, "Sound/Game/Card/card_deselect_v1");

        private static AudioClip _fieldConfirm;

        public static AudioClip FieldConfirm => Load(ref _fieldConfirm, "Sound/Game/field_confirm_v2");
    }
}
