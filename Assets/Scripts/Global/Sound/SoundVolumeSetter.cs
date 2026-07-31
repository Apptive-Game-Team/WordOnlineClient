using Data.Sound;
using LobbyScene.SettingPage;
using UnityEngine;

namespace Global.Sound
{
    public class SoundVolumeSetter : MonoBehaviour
    {

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundType soundType;

        private float originalVolume = -1;
        private float multiplier = 1f;

        public enum SoundType
        {
            UI, BGM, Game
        }

        /// Binds an AudioSource created at runtime so later slider changes keep
        /// applying to it.
        public static SoundVolumeSetter Attach(
            AudioSource source,
            SoundType type,
            float baseVolume = 1f)
        {
            SoundVolumeSetter setter = source.gameObject.AddComponent<SoundVolumeSetter>();
            setter.audioSource = source;
            setter.soundType = type;
            setter.originalVolume = Mathf.Clamp01(baseVolume);
            setter.SetVolume();
            return setter;
        }

        /// Extra factor owned by the caller (BGM fades). Kept apart from the user
        /// volume so neither overwrites the other.
        public float Multiplier
        {
            get => multiplier;
            set
            {
                multiplier = Mathf.Clamp01(value);
                SetVolume();
            }
        }

        private void Awake()
        {
            SoundDataSetter.OnSoundDataChanged += SetVolume;
        }

        private void Start()
        {
            SetVolume();
        }

        private void OnDestroy()
        {
            SoundDataSetter.OnSoundDataChanged -= SetVolume;
        }

        private void SetVolume()
        {
            if (audioSource == null)
            {
                return;
            }

            if (originalVolume < 0)
            {
                originalVolume = audioSource.volume;
            }
            audioSource.volume = originalVolume * multiplier * GetScaleByType(soundType);
        }

        private float GetScaleByType(SoundType type)
        {
            switch (type)
            {
                case SoundType.UI:
                    return SoundData.UiScale;
                case SoundType.BGM:
                    return SoundData.BgmScale;
                case SoundType.Game:
                    return SoundData.GameScale;
                default:
                    return 1f;
            }
        }
    }
}
