using UnityEngine;

namespace Global.Sound
{
    /// Owns the single AudioSource used for UI one-shots. UI sounds used to grab
    /// the first AudioSource on the SceneContext object, which is where
    /// GameSfxPlayer keeps its voices, so a click could overwrite a playing game
    /// SFX or get cut off when that voice was recycled.
    public class UiSfxPlayer : MonoBehaviour
    {
        private static UiSfxPlayer instance;

        private AudioSource audioSource;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            instance = null;
        }

        public static void Play(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsureInstance().audioSource.PlayOneShot(clip);
        }

        private static UiSfxPlayer EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject host = new GameObject(nameof(UiSfxPlayer));
            DontDestroyOnLoad(host);
            instance = host.AddComponent<UiSfxPlayer>();
            return instance;
        }

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            SoundVolumeSetter.Attach(audioSource, SoundVolumeSetter.SoundType.UI);
        }
    }
}
