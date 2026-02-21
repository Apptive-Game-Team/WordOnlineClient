using Data.Sound;
using Sound;
using UnityEngine;

namespace Global.Button
{
    public abstract class ButtonBase : MonoBehaviour
    {

        private AudioSource _audioSource;
        private AudioSource AudioSource
        {
            get {
                _audioSource = SceneContext.Instance.gameObject.GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = SceneContext.Instance.gameObject.AddComponent<AudioSource>();
                }
                return _audioSource;
            }
        }

        protected abstract void OnClickButton();

        public virtual void ButtonEvent()
        {
            PlayUISound();
            OnClickButton();
        }

        private void PlayUISound()
        {
            AudioSource.clip = SoundAssets.ClickButton;
            AudioSource.volume = SoundData.uiVolume / 100f;
            AudioSource.Play();
        }
    }
}
