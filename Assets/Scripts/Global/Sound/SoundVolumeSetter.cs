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
    
        public enum SoundType
        {
            UI, BGM, Game
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
            if (originalVolume < 0)
            {
                originalVolume = audioSource.volume;
            }
            audioSource.volume = originalVolume * GetVolumeByType(soundType) / 100f;
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
