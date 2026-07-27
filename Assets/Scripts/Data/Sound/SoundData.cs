using UnityEngine;

namespace Data.Sound
{
    public class SoundData
    {
        public const int MAX_VOLUME = 100;
        public const int MIN_VOLUME = 0;
        public const int DEFAULT_VOLUME = 50;

        private const string UI_VOLUME_KEY = "sound.uiVolume";
        private const string BGM_VOLUME_KEY = "sound.bgmVolume";
        private const string GAME_VOLUME_KEY = "sound.gameVolume";

        // Sliders move linearly but loudness does not, so amplitude is
        // ratio^CURVE_EXPONENT. Keeps the lower half of the slider usable
        // instead of sitting near full volume already.
        private const float CURVE_EXPONENT = 1.5f;

        public static int uiVolume = DEFAULT_VOLUME;
        public static int bgmVolume = DEFAULT_VOLUME;
        public static int gameVolume = DEFAULT_VOLUME;

        static SoundData()
        {
            Load();
        }

        public static void Load()
        {
            uiVolume = Clamp(PlayerPrefs.GetInt(UI_VOLUME_KEY, DEFAULT_VOLUME));
            bgmVolume = Clamp(PlayerPrefs.GetInt(BGM_VOLUME_KEY, DEFAULT_VOLUME));
            gameVolume = Clamp(PlayerPrefs.GetInt(GAME_VOLUME_KEY, DEFAULT_VOLUME));
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(UI_VOLUME_KEY, uiVolume);
            PlayerPrefs.SetInt(BGM_VOLUME_KEY, bgmVolume);
            PlayerPrefs.SetInt(GAME_VOLUME_KEY, gameVolume);
            PlayerPrefs.Save();
        }

        public static float UiScale => ToScale(uiVolume);
        public static float BgmScale => ToScale(bgmVolume);
        public static float GameScale => ToScale(gameVolume);

        private static int Clamp(int volume)
        {
            return Mathf.Clamp(volume, MIN_VOLUME, MAX_VOLUME);
        }

        private static float ToScale(int volume)
        {
            float ratio = Mathf.Clamp01((float)volume / MAX_VOLUME);
            return ratio <= 0f ? 0f : Mathf.Pow(ratio, CURVE_EXPONENT);
        }
    }
}
