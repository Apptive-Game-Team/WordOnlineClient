using System.Collections.Generic;
using GameScene.ServedObjectComponent.OnAttack;
using Sound;
using Sound.Config;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Sound
{
    public class ServedObjectSfxController : MonoBehaviour
    {
        private static readonly HashSet<string> WarnedRuntimeTypes = new();
        private static ObjectSfxCatalog catalog;
        private static bool catalogLoadAttempted;

        private ServedObject servedObject;
        private ObjectSfxProfile profile;
        private float nextMovementTime;
        private bool deathPlayed;
        private bool ownsAttack;
        private bool ownsMovement;
        private bool ownsHit;
        private bool ownsHeal;
        private bool ownsDeath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            catalog = null;
            catalogLoadAttempted = false;
            WarnedRuntimeTypes.Clear();
        }

        public static void Attach(ServedObject target, string runtimeType, bool playSpawn)
        {
            if (!TryResolveProfile(runtimeType, out ObjectSfxProfile resolvedProfile))
            {
                return;
            }

            ServedObjectSfxController controller =
                target.gameObject.AddComponent<ServedObjectSfxController>();
            controller.Initialize(target, resolvedProfile, playSpawn);
        }

        private static bool TryResolveProfile(
            string runtimeType,
            out ObjectSfxProfile resolvedProfile)
        {
            resolvedProfile = null;
            if (!catalogLoadAttempted)
            {
                catalog = Resources.Load<ObjectSfxCatalog>(ObjectSfxCatalog.ResourcesPath);
                catalogLoadAttempted = true;
            }

            if (catalog == null)
            {
                WarnOnce(
                    "__MissingCatalog__",
                    $"Object SFX catalog is missing at Resources/{ObjectSfxCatalog.ResourcesPath}. " +
                    "Profile lifecycle SFX will remain silent.");
                return false;
            }

            if (!catalog.TryResolve(runtimeType, out resolvedProfile))
            {
                WarnOnce(
                    runtimeType,
                    $"Object SFX catalog has no row for runtime type '{runtimeType}'. " +
                    "Profile lifecycle SFX will remain silent.");
                return false;
            }

            return resolvedProfile != null;
        }

        private static void WarnOnce(string key, string message)
        {
            string normalizedKey = string.IsNullOrEmpty(key) ? "__EmptyRuntimeType__" : key;
            if (WarnedRuntimeTypes.Add(normalizedKey))
            {
                Debug.LogWarning(message);
            }
        }

        private void Initialize(
            ServedObject target,
            ObjectSfxProfile resolvedProfile,
            bool playSpawn)
        {
            servedObject = target;
            profile = resolvedProfile;
            deathPlayed = false;
            nextMovementTime = 0f;

            ownsMovement = profile.Movement.Enabled;
            if (ownsMovement)
            {
                servedObject.OnMoved += PlayMovement;
            }

            ownsHit = profile.Hit.Enabled;
            if (ownsHit)
            {
                servedObject.OnHpDecreased += PlayHit;
            }

            ownsHeal = profile.Heal.Enabled;
            if (ownsHeal)
            {
                servedObject.OnHpIncreased += PlayHeal;
            }

            ownsDeath = profile.Death.Enabled;
            if (ownsDeath)
            {
                servedObject.OnDestroyed += PlayDeath;
            }

            ownsAttack = profile.Attack.Enabled && profile.Attack.Clip != null;
            if (ownsAttack)
            {
                DisableLegacyAttackOwners();
                servedObject.OnAttack += PlayAttack;
            }

            if (playSpawn)
            {
                PlaySlot(
                    profile.Spawn,
                    GameSfxCategory.SpawnDeath,
                    GameSfxPriority.Spawn);
            }
        }

        private void DisableLegacyAttackOwners()
        {
            foreach (OnAttackSoundPlayer legacyOwner in
                     GetComponentsInChildren<OnAttackSoundPlayer>(true))
            {
                legacyOwner.enabled = false;
            }
        }

        private void PlayMovement()
        {
            if (Time.unscaledTime < nextMovementTime)
            {
                return;
            }

            nextMovementTime = Time.unscaledTime + profile.MovementCooldown;
            PlaySlot(
                profile.Movement,
                GameSfxCategory.Movement,
                GameSfxPriority.Movement);
        }

        private void PlayAttack() =>
            PlaySlot(profile.Attack, GameSfxCategory.Attack, GameSfxPriority.Attack);

        private void PlayHeal() =>
            PlaySlot(profile.Heal, GameSfxCategory.HitHeal, GameSfxPriority.HitHeal);

        private void PlayHit() =>
            PlaySlot(profile.Hit, GameSfxCategory.HitHeal, GameSfxPriority.HitHeal);

        private void PlayDeath()
        {
            if (deathPlayed)
            {
                return;
            }

            deathPlayed = true;
            PlaySlot(
                profile.Death,
                GameSfxCategory.SpawnDeath,
                GameSfxPriority.Death);
        }

        private static void PlaySlot(
            ObjectSfxEventSlot slot,
            GameSfxCategory category,
            GameSfxPriority priority)
        {
            if (!slot.Enabled || slot.Clip == null)
            {
                return;
            }

            GameSfxPlayer.Play(slot.Clip, category, priority, slot.Volume, slot.Pitch);
        }

        private void OnDestroy()
        {
            if (servedObject == null || profile == null)
            {
                return;
            }

            if (ownsMovement)
            {
                servedObject.OnMoved -= PlayMovement;
            }

            if (ownsHit)
            {
                servedObject.OnHpDecreased -= PlayHit;
            }

            if (ownsHeal)
            {
                servedObject.OnHpIncreased -= PlayHeal;
            }

            if (ownsDeath)
            {
                servedObject.OnDestroyed -= PlayDeath;
            }

            if (ownsAttack)
            {
                servedObject.OnAttack -= PlayAttack;
            }
        }
    }
}
