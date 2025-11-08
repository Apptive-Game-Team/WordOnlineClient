using Script.Data.Sound;
using UnityEngine;

namespace Script.Global
{
    public class SoundVolumeSetter : MonoBehaviour
    {
    
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundType soundType;
    
        public enum SoundType
        {
            UI, BGM, Game
        }
    
        private void Start()
        {
            SetVolume();
        }

        private void SetVolume()
        {
            audioSource.volume = audioSource.volume * GetVolumeByType(soundType) / 100f;
        }
    
        private float GetVolumeByType(SoundType type)
        {
            switch (type)
            {
                case SoundType.UI:
                    return SoundData.uiVolume;
                case SoundType.BGM:
                    return SoundData.bgmVolume;
                case SoundType.Game:
                    return SoundData.gameVolume;
                default:
                    return SoundData.DEFAULT_VOLUME;
            }
        }
    }
}
