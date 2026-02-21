using System;
using Data.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyScene.SettingPage
{
    public class SoundDataSetter : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider uiSlider;
        [SerializeField] private Slider gameSlider;
        
        public static event Action OnSoundDataChanged;
        
        private void Awake()
        {
            InitSlider();
            InitValueChangedListener();
        }

        private void InitValueChangedListener()
        {
            bgmSlider.onValueChanged.AddListener(value =>
                {
                    SoundData.bgmVolume = Convert.ToInt32(value);
                    OnSoundDataChanged?.Invoke();
                }
                );
            uiSlider.onValueChanged.AddListener(value =>
                {
                    SoundData.uiVolume = Convert.ToInt32(value);
                    OnSoundDataChanged?.Invoke();
                }
                );
            gameSlider.onValueChanged.AddListener(value =>
                {
                    SoundData.gameVolume = Convert.ToInt32(value);
                    OnSoundDataChanged?.Invoke();
                }
                );
        }
        

        private void InitSlider()
        {
            bgmSlider.minValue = SoundData.MIN_VOLUME;
            bgmSlider.maxValue = SoundData.MAX_VOLUME;
            uiSlider.minValue = SoundData.MIN_VOLUME;
            uiSlider.maxValue = SoundData.MAX_VOLUME;
            gameSlider.minValue = SoundData.MIN_VOLUME;
            gameSlider.maxValue = SoundData.MAX_VOLUME;

            bgmSlider.value = SoundData.bgmVolume;
            uiSlider.value = SoundData.uiVolume;
            gameSlider.value = SoundData.gameVolume;
        }
    }
}