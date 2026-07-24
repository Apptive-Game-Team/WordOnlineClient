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

    public enum GameSfxPriority
    {
        Movement,
        Spawn,
        Attack,
        HitHeal,
        Death
    }

    public class GameSfxPlayer : MonoBehaviour
    {
        private const int OverallCap = 10;
        private static GameSfxPlayer instance;

        private readonly List<Voice> voices = new();

        public static void Play(
            AudioClip clip,
            GameSfxCategory category,
            GameSfxPriority priority,
            float volume = 1f,
            float pitch = 1f)
        {
            if (clip == null)
            {
                return;
            }

            EnsureInstance().PlayInternal(clip, category, priority, volume, pitch);
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
            AudioClip clip,
            GameSfxCategory category,
            GameSfxPriority priority,
            float volume,
            float pitch)
        {
            RemoveCompletedVoices();

            Voice categoryVictim = null;
            if (Count(category) >= GetCategoryCap(category))
            {
                categoryVictim = FindOldestLowerPriorityVoice(category, priority, null);
                if (categoryVictim == null)
                {
                    return;
                }
            }

            Voice overallVictim = null;
            int countAfterCategoryEviction = voices.Count - (categoryVictim != null ? 1 : 0);
            if (countAfterCategoryEviction >= OverallCap)
            {
                overallVictim = FindOldestLowerPriorityVoice(null, priority, categoryVictim);
                if (overallVictim == null)
                {
                    return;
                }
            }

            RemoveVoice(categoryVictim);
            RemoveVoice(overallVictim);

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.pitch = pitch;
            source.volume = Mathf.Clamp01(volume) * SoundData.gameVolume / 100f;
            source.PlayOneShot(clip);
            voices.Add(new Voice(
                source,
                category,
                priority,
                Time.unscaledTime + clip.length / Mathf.Max(Mathf.Abs(pitch), 0.01f)));
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

        private Voice FindOldestLowerPriorityVoice(
            GameSfxCategory? category,
            GameSfxPriority requestedPriority,
            Voice excluded)
        {
            foreach (Voice voice in voices)
            {
                if (voice == excluded ||
                    (category.HasValue && voice.Category != category.Value) ||
                    (int)voice.Priority >= (int)requestedPriority)
                {
                    continue;
                }

                return voice;
            }

            return null;
        }

        private void RemoveVoice(Voice voice)
        {
            if (voice == null)
            {
                return;
            }

            voices.Remove(voice);
            if (voice.Source != null)
            {
                voice.Source.Stop();
                Destroy(voice.Source);
            }
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

        private sealed class Voice
        {
            public readonly AudioSource Source;
            public readonly GameSfxCategory Category;
            public readonly GameSfxPriority Priority;
            public readonly float EndTime;

            public Voice(
                AudioSource source,
                GameSfxCategory category,
                GameSfxPriority priority,
                float endTime)
            {
                Source = source;
                Category = category;
                Priority = priority;
                EndTime = endTime;
            }
        }
    }
}
