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
        private bool deathSoundPlayed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            nextGlobalMoveSoundTime = 0f;
        }

        public void Initialize(ServedObject target, string prefabType)
        {
            Unsubscribe();
            servedObject = target;
            deathSoundPlayed = false;
            lastMoveSoundTime = float.NegativeInfinity;
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            DisableLegacyAttackSounds();
            ResolveConceptClips(prefabType);
            Subscribe();
            Play(SoundAssets.DropSound, 0.45f);
        }

        private void ResolveConceptClips(string prefabType)
        {
            attackClip = SoundAssets.ShootSound;
            deathClip = SoundAssets.ExplosionSound;

            switch (SoundConceptResolver.Resolve(prefabType))
            {
                case SoundConcept.Fire:
                    deathClip = SoundAssets.FireExplosion;
                    break;
                case SoundConcept.Water:
                    deathClip = SoundAssets.WaterExplosion;
                    break;
                case SoundConcept.Nature:
                    attackClip = SoundAssets.ArrowShot;
                    deathClip = SoundAssets.NatureExplosion;
                    break;
                case SoundConcept.Lightning:
                    deathClip = SoundAssets.LightningExplosion;
                    break;
                case SoundConcept.Rock:
                    attackClip = SoundAssets.ArrowShot;
                    deathClip = SoundAssets.RockExplosion;
                    break;
                case SoundConcept.Wind:
                    attackClip = SoundAssets.WindSound;
                    deathClip = SoundAssets.NatureExplosion;
                    break;
            }
        }

        private void DisableLegacyAttackSounds()
        {
            OnAttackSoundPlayer[] legacyPlayers = GetComponentsInChildren<OnAttackSoundPlayer>(true);
            foreach (OnAttackSoundPlayer legacyPlayer in legacyPlayers)
            {
                legacyPlayer.enabled = false;
            }
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
            Play(attackClip);
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
            if (deathSoundPlayed || deathClip == null) return;
            deathSoundPlayed = true;
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
