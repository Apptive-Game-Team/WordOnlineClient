using System;
using Script.Data.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Script.LobbyScene.SettingPage
{
    public class SoundDataSetter : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider uiSlider;
        [SerializeField] private Slider gameSlider;
        
        private void Awake()
        {
            initSlider();
            initValueChangedListener();
        }

        private void initValueChangedListener()
        {
            bgmSlider.onValueChanged.AddListener(
                value => { SoundData.bgmVolume = Convert.ToInt32(value); }
                );
            uiSlider.onValueChanged.AddListener(
                value => { SoundData.uiVolume = Convert.ToInt32(value); }
                );
            gameSlider.onValueChanged.AddListener(
                value => { SoundData.gameVolume = Convert.ToInt32(value); }
                );
        }
        

        private void initSlider()
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