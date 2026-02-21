using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Global.Sound.BGM
{
    public class BGMPlayer : SingletonObject<BGMPlayer>
    {
        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            PlayBGM();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayBGM();
        }
        
        private void PlayBGM()
        {
            var bgmClipContainer = FindObjectOfType<BGMClipContainer>();
            var audioSource = GetComponent<AudioSource>();
            audioSource.pitch = 1.0f;
            if (bgmClipContainer != null)
            {
                AudioClip audioClip = bgmClipContainer.GetBGMClip();
                
                if (audioSource.clip.name == audioClip.name) return;
                
                audioSource.clip = bgmClipContainer.GetBGMClip();
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
        
        public void SetPitch(float pitch)
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.pitch = pitch;
        }
    }
}