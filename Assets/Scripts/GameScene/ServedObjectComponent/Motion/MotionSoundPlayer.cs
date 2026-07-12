using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Motion
{
    [DisallowMultipleComponent]
    public class MotionSoundPlayer : MonoBehaviour
    {
        private const float GlobalCooldown = 0.08f;
        private const float PlaybackChance = 0.45f;
        private const float BaseVolume = 0.12f;
        private const float PitchVariation = 0.08f;

        private static readonly Dictionary<MotionSoundProfile, AudioClip[]> ClipCache = new();
        private static float nextGlobalPlaybackTime;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = BaseVolume;
        }

        public void Play(MotionSoundProfile profile, float initialDelay, float interval)
        {
            if (profile == MotionSoundProfile.Wind)
            {
                PlayLoop(profile);
                return;
            }

            if (interval <= 0f)
            {
                return;
            }

            StartCoroutine(PlayAtMotionContacts(profile, initialDelay, interval));
        }

        private void PlayLoop(MotionSoundProfile profile)
        {
            AudioClip[] clips = GetClips(profile);
            if (clips.Length == 0)
            {
                return;
            }

            audioSource.clip = clips[0];
            audioSource.loop = true;
            audioSource.volume = BaseVolume * 0.35f;
            audioSource.time = Random.Range(0f, audioSource.clip.length);
            audioSource.Play();
        }

        private IEnumerator PlayAtMotionContacts(MotionSoundProfile profile, float initialDelay, float interval)
        {
            yield return new WaitForSeconds(initialDelay);

            while (enabled)
            {
                TryPlay(profile);
                yield return new WaitForSeconds(interval);
            }
        }

        private void TryPlay(MotionSoundProfile profile)
        {
            if (Time.unscaledTime < nextGlobalPlaybackTime || Random.value > PlaybackChance)
            {
                return;
            }

            AudioClip[] clips = GetClips(profile);
            if (clips.Length == 0)
            {
                return;
            }

            audioSource.pitch = 1f + Random.Range(-PitchVariation, PitchVariation);
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            nextGlobalPlaybackTime = Time.unscaledTime + GlobalCooldown;
        }

        private static AudioClip[] GetClips(MotionSoundProfile profile)
        {
            if (ClipCache.TryGetValue(profile, out AudioClip[] clips))
            {
                return clips;
            }

            clips = profile == MotionSoundProfile.Wind
                ? new[] { Resources.Load<AudioClip>("Sound/Game/Magic/wind") }
                : Resources.LoadAll<AudioClip>($"Sound/Game/Motion/{profile}");

            if (clips.Length == 1 && clips[0] == null)
            {
                clips = new AudioClip[0];
            }

            ClipCache.Add(profile, clips);
            return clips;
        }
    }
}
