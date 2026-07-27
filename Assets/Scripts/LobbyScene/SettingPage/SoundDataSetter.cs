using System;
using System.Collections;
using Data.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyScene.SettingPage
{
    public class SoundDataSetter : MonoBehaviour
    {
        private const float SaveDelay = 0.5f;

        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider uiSlider;
        [SerializeField] private Slider gameSlider;

        public static event Action OnSoundDataChanged;

        private Coroutine saveRoutine;

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
                    ApplyChange();
                }
                );
            uiSlider.onValueChanged.AddListener(value =>
                {
                    SoundData.uiVolume = Convert.ToInt32(value);
                    ApplyChange();
                }
                );
            gameSlider.onValueChanged.AddListener(value =>
                {
                    SoundData.gameVolume = Convert.ToInt32(value);
                    ApplyChange();
                }
                );
        }

        private void ApplyChange()
        {
            OnSoundDataChanged?.Invoke();

            // Dragging a slider fires per frame, and PlayerPrefs.Save flushes to
            // IndexedDB on WebGL, so coalesce writes into one save per drag.
            if (saveRoutine != null)
            {
                StopCoroutine(saveRoutine);
            }
            saveRoutine = StartCoroutine(SaveAfterDelay());
        }

        private IEnumerator SaveAfterDelay()
        {
            yield return new WaitForSecondsRealtime(SaveDelay);
            saveRoutine = null;
            SoundData.Save();
        }

        private void OnDisable()
        {
            if (saveRoutine == null)
            {
                return;
            }

            StopCoroutine(saveRoutine);
            saveRoutine = null;
            SoundData.Save();
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