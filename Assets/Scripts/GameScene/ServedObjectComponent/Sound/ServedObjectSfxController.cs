using System;
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
        private static readonly ObjectSfxEventSlot DisabledSlot = new();
        private static ObjectSfxCatalog catalog;
        private static bool catalogLoadAttempted;

        private ServedObject servedObject;
        private ObjectSfxProfile profile;
        private ObjectSfxProfile signature;
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
            if (!TryResolveProfile(
                    runtimeType,
                    out ObjectSfxProfile resolvedProfile,
                    out ObjectSfxProfile resolvedSignature))
            {
                return;
            }

            ServedObjectSfxController controller =
                target.gameObject.AddComponent<ServedObjectSfxController>();
            controller.Initialize(target, resolvedProfile, resolvedSignature, playSpawn);
        }

        private static bool TryResolveProfile(
            string runtimeType,
            out ObjectSfxProfile resolvedProfile,
            out ObjectSfxProfile resolvedSignature)
        {
            resolvedProfile = null;
            resolvedSignature = null;
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

            if (!catalog.TryResolve(runtimeType, out resolvedProfile, out resolvedSignature))
            {
                WarnOnce(
                    runtimeType,
                    $"Object SFX catalog has no row for runtime type '{runtimeType}'. " +
                    "Profile lifecycle SFX will remain silent.");
                return false;
            }

            // A signature can carry an object's lifecycle SFX on its own, so a
            // missing profile is not by itself a reason to skip attaching.
            return resolvedProfile != null || resolvedSignature != null;
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
            ObjectSfxProfile resolvedSignature,
            bool playSpawn)
        {
            servedObject = target;
            profile = resolvedProfile;
            signature = resolvedSignature;
            deathPlayed = false;
            nextMovementTime = 0f;

            ownsMovement = Resolve(p => p.Movement).Enabled;
            if (ownsMovement)
            {
                servedObject.OnMoved += PlayMovement;
            }

            ownsHit = Resolve(p => p.Hit).Enabled;
            if (ownsHit)
            {
                servedObject.OnHpDecreased += PlayHit;
            }

            ownsHeal = Resolve(p => p.Heal).Enabled;
            if (ownsHeal)
            {
                servedObject.OnHpIncreased += PlayHeal;
            }

            ownsDeath = Resolve(p => p.Death).Enabled;
            if (ownsDeath)
            {
                servedObject.OnDestroyed += PlayDeath;
            }

            ObjectSfxEventSlot attackSlot = Resolve(p => p.Attack);
            ownsAttack = attackSlot.Enabled && attackSlot.Clip != null;
            if (ownsAttack)
            {
                DisableLegacyAttackOwners();
                servedObject.OnAttack += PlayAttack;
            }

            if (playSpawn)
            {
                PlaySlot(
                    Resolve(p => p.Spawn),
                    GameSfxCategory.SpawnDeath,
                    GameSfxPriority.Spawn);
            }
        }

        // Signature is a per-slot replacement, not an overlay: when its slot for this
        // event is enabled it fully takes over, otherwise the profile's slot plays.
        // Profile-level values (MovementCooldown, IsStatic, IsProjectileOrTransient)
        // are untouched by this and always come from `profile`.
        private ObjectSfxEventSlot Resolve(Func<ObjectSfxProfile, ObjectSfxEventSlot> pick)
        {
            if (signature != null)
            {
                ObjectSfxEventSlot signatureSlot = pick(signature);
                if (signatureSlot.Enabled)
                {
                    return signatureSlot;
                }
            }

            return profile != null ? pick(profile) : DisabledSlot;
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

            nextMovementTime = Time.unscaledTime + MovementCooldown();
            PlaySlot(
                Resolve(p => p.Movement),
                GameSfxCategory.Movement,
                GameSfxPriority.Movement);
        }

        // Falling back to zero here would let a signature-only object play a step
        // sound on every position update, which is the throttling the ownership
        // matrix exists to prevent. Signature is itself a profile, so it carries
        // the same cooldown field and can answer when no base profile is set.
        private float MovementCooldown()
        {
            if (profile != null)
            {
                return profile.MovementCooldown;
            }

            return signature != null ? signature.MovementCooldown : 0f;
        }

        private void PlayAttack() =>
            PlaySlot(Resolve(p => p.Attack), GameSfxCategory.Attack, GameSfxPriority.Attack);

        private void PlayHeal() =>
            PlaySlot(Resolve(p => p.Heal), GameSfxCategory.HitHeal, GameSfxPriority.HitHeal);

        private void PlayHit() =>
            PlaySlot(Resolve(p => p.Hit), GameSfxCategory.HitHeal, GameSfxPriority.HitHeal);

        private void PlayDeath()
        {
            if (deathPlayed)
            {
                return;
            }

            deathPlayed = true;
            PlaySlot(
                Resolve(p => p.Death),
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
            // profile can legitimately be null for a signature-only object, so only
            // servedObject (set once, in Initialize) gates whether we ever subscribed.
            if (servedObject == null)
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
