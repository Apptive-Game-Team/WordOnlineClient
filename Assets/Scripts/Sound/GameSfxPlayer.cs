using System.Collections.Generic;
using Data.Sound;
using Global;
using UnityEngine;

namespace Sound
{
    public enum GameSfxCategory
    {
        Movement,
        SpawnDeath,
        Attack,
        HitHeal
    }

    public class GameSfxPlayer : MonoBehaviour
    {
        private const int OverallCap = 10;
        private static GameSfxPlayer instance;

        private readonly List<Voice> voices = new();

        public static void Play(AudioClip clip, GameSfxCategory category, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                return;
            }

            EnsureInstance().PlayInternal(clip, null, category, volume, pitch);
        }

        public static void PlayLayered(
            AudioClip body,
            AudioClip accent,
            GameSfxCategory category,
            float volume = 1f,
            float pitch = 1f)
        {
            if (body == null && accent == null)
            {
                return;
            }

            EnsureInstance().PlayInternal(body, accent, category, volume, pitch);
        }

        private static GameSfxPlayer EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = SceneContext.Instance.gameObject.GetComponent<GameSfxPlayer>();
            if (instance == null)
            {
                instance = SceneContext.Instance.gameObject.AddComponent<GameSfxPlayer>();
            }

            return instance;
        }

        private void PlayInternal(
            AudioClip body,
            AudioClip accent,
            GameSfxCategory category,
            float volume,
            float pitch)
        {
            RemoveCompletedVoices();
            if (voices.Count >= OverallCap || Count(category) >= GetCategoryCap(category))
            {
                return;
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.pitch = pitch;
            source.volume = Mathf.Clamp01(volume) * SoundData.gameVolume / 100f;
            if (body != null) source.PlayOneShot(body);
            if (accent != null) source.PlayOneShot(accent);
            float duration = Mathf.Max(body != null ? body.length : 0f, accent != null ? accent.length : 0f);
            voices.Add(new Voice(source, category, Time.unscaledTime + duration / Mathf.Abs(pitch)));
        }

        private void Update()
        {
            RemoveCompletedVoices();
        }

        private void RemoveCompletedVoices()
        {
            for (int index = voices.Count - 1; index >= 0; index--)
            {
                Voice voice = voices[index];
                if (voice.Source != null && Time.unscaledTime < voice.EndTime)
                {
                    continue;
                }

                if (voice.Source != null)
                {
                    Destroy(voice.Source);
                }
                voices.RemoveAt(index);
            }
        }

        private int Count(GameSfxCategory category)
        {
            int count = 0;
            foreach (Voice voice in voices)
            {
                if (voice.Category == category)
                {
                    count++;
                }
            }
            return count;
        }

        private static int GetCategoryCap(GameSfxCategory category)
        {
            return category switch
            {
                GameSfxCategory.Movement => 2,
                GameSfxCategory.SpawnDeath => 3,
                GameSfxCategory.Attack => 4,
                GameSfxCategory.HitHeal => 4,
                _ => 1
            };
        }

        private readonly struct Voice
        {
            public readonly AudioSource Source;
            public readonly GameSfxCategory Category;
            public readonly float EndTime;

            public Voice(AudioSource source, GameSfxCategory category, float endTime)
            {
                Source = source;
                Category = category;
                EndTime = endTime;
            }
        }
    }
}
