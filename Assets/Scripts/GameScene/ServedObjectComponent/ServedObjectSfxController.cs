using Data.Sound;
using GameScene.ServedObjectComponent.OnAttack;
using Sound;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectSfxController : MonoBehaviour
    {
        private const float MoveSoundInterval = 0.45f;
        private const float GlobalMoveSoundInterval = 0.08f;
        private static float nextGlobalMoveSoundTime;

        private ServedObject servedObject;
        private AudioSource audioSource;
        private AudioClip attackClip;
        private AudioClip deathClip;
        private float lastMoveSoundTime = float.NegativeInfinity;
        private bool hasDedicatedAttackSound;

        public void Initialize(ServedObject target, string prefabType)
        {
            Unsubscribe();
            servedObject = target;
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            hasDedicatedAttackSound = GetComponentInChildren<OnAttackSoundPlayer>(true) != null;
            ResolveConceptClips(prefabType);
            Subscribe();
            Play(SoundAssets.DropSound, 0.45f);
        }

        private void ResolveConceptClips(string prefabType)
        {
            string type = prefabType ?? string.Empty;
            attackClip = SoundAssets.ShootSound;
            deathClip = SoundAssets.ExplosionSound;

            if (ContainsAny(type, "Fire", "Magma", "Ember", "Meteor", "Crater"))
            {
                deathClip = SoundAssets.FireExplosion;
            }
            else if (ContainsAny(type, "Water", "Aqua", "Bubble", "Tide", "Rain"))
            {
                deathClip = SoundAssets.WaterExplosion;
            }
            else if (ContainsAny(type, "Leaf", "Nature", "Vine", "Tree", "Seed", "Overgrowth"))
            {
                attackClip = SoundAssets.ArrowShot;
                deathClip = SoundAssets.NatureExplosion;
            }
            else if (ContainsAny(type, "Electric", "Lightning", "Thunder", "Zap", "Shock"))
            {
                deathClip = SoundAssets.LightningExplosion;
            }
            else if (ContainsAny(type, "Rock", "Ground", "Stone", "Sand"))
            {
                attackClip = SoundAssets.ArrowShot;
                deathClip = SoundAssets.RockExplosion;
            }
            else if (ContainsAny(type, "Wind", "Cloud", "Tornado", "Gale", "Storm"))
            {
                attackClip = SoundAssets.WindSound;
                deathClip = SoundAssets.NatureExplosion;
            }
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (value.IndexOf(candidate, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void Subscribe()
        {
            if (servedObject == null) return;
            servedObject.OnAttack += PlayAttack;
            servedObject.OnMoved += PlayMove;
            servedObject.OnHpDecreased += PlayHit;
            servedObject.OnHpIncreased += PlayHeal;
            servedObject.OnDestroyed += PlayDeath;
        }

        private void Unsubscribe()
        {
            if (servedObject == null) return;
            servedObject.OnAttack -= PlayAttack;
            servedObject.OnMoved -= PlayMove;
            servedObject.OnHpDecreased -= PlayHit;
            servedObject.OnHpIncreased -= PlayHeal;
            servedObject.OnDestroyed -= PlayDeath;
        }

        private void PlayAttack()
        {
            if (!hasDedicatedAttackSound) Play(attackClip);
        }

        private void PlayMove()
        {
            if (Time.time - lastMoveSoundTime < MoveSoundInterval || Time.time < nextGlobalMoveSoundTime) return;
            lastMoveSoundTime = Time.time;
            nextGlobalMoveSoundTime = Time.time + GlobalMoveSoundInterval;
            Play(SoundAssets.WindSound, 0.12f);
        }

        private void PlayHit() => Play(SoundAssets.Hit, 0.75f);
        private void PlayHeal() => Play(SoundAssets.Heal, 0.75f);

        private void PlayDeath()
        {
            if (deathClip == null) return;
            GameObject detachedAudio = new GameObject("DetachedDeathSfx");
            AudioSource detachedSource = detachedAudio.AddComponent<AudioSource>();
            detachedSource.spatialBlend = 0f;
            detachedSource.volume = SoundData.gameVolume / 100f;
            detachedSource.PlayOneShot(deathClip);
            Destroy(detachedAudio, deathClip.length + 0.1f);
        }

        private void Play(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || audioSource == null) return;
            audioSource.volume = SoundData.gameVolume / 100f;
            audioSource.PlayOneShot(clip, volumeScale);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
