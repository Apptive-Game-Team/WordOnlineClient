using GameScene.ServedObjectComponent.OnAttack;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Sound
{
    public class ServedObjectSfxController : MonoBehaviour
    {
        private const float MovementCooldown = 0.45f;

        private ServedObject servedObject;
        private RuntimeSfxArchetype archetype;
        private RuntimeSfxElement element;
        private float nextMovementTime;
        private bool deathPlayed;
        private bool ownsAttack;

        public static void Attach(ServedObject target, string runtimeType, bool playSpawn)
        {
            RuntimeSfxArchetype resolved = ObjectSfxRuntimeTypeCatalog.Resolve(runtimeType);
            if (resolved == RuntimeSfxArchetype.Silent || resolved == RuntimeSfxArchetype.Transient)
            {
                return;
            }

            ServedObjectSfxController controller = target.gameObject.AddComponent<ServedObjectSfxController>();
            controller.Initialize(target, resolved, ObjectSfxRuntimeTypeCatalog.ResolveElement(runtimeType), playSpawn);
        }

        private void Initialize(
            ServedObject target,
            RuntimeSfxArchetype resolved,
            RuntimeSfxElement resolvedElement,
            bool playSpawn)
        {
            servedObject = target;
            archetype = resolved;
            element = resolvedElement;
            deathPlayed = false;
            nextMovementTime = 0f;
            ownsAttack = GetComponentInChildren<OnAttackSoundPlayer>(true) == null;
            servedObject.OnMoved += PlayMovement;
            servedObject.OnHpDecreased += PlayHit;
            servedObject.OnHpIncreased += PlayHeal;
            servedObject.OnDestroyed += PlayDeath;
            if (ownsAttack)
            {
                servedObject.OnAttack += PlayAttack;
            }

            if (playSpawn)
            {
                PlayWithElement(archetype == RuntimeSfxArchetype.Building
                    ? global::Sound.SoundAssets.BuildingSpawn
                    : global::Sound.SoundAssets.CreatureSpawn,
                    global::Sound.GameSfxCategory.SpawnDeath);
            }
        }

        private void PlayMovement()
        {
            if (archetype == RuntimeSfxArchetype.Building || Time.unscaledTime < nextMovementTime)
            {
                return;
            }

            nextMovementTime = Time.unscaledTime + MovementCooldown;
            Play(global::Sound.SoundAssets.CreatureMove, global::Sound.GameSfxCategory.Movement);
        }

        private void PlayAttack()
        {
            PlayWithElement(global::Sound.SoundAssets.GenericAttack, global::Sound.GameSfxCategory.Attack);
        }
        private void PlayHeal() => Play(global::Sound.SoundAssets.Heal, global::Sound.GameSfxCategory.HitHeal);

        private void PlayHit()
        {
            PlayWithElement(archetype == RuntimeSfxArchetype.Building
                ? global::Sound.SoundAssets.BuildingHit
                : global::Sound.SoundAssets.CreatureHit,
                global::Sound.GameSfxCategory.HitHeal);
        }

        private void PlayDeath()
        {
            if (deathPlayed)
            {
                return;
            }

            deathPlayed = true;
            PlayWithElement(archetype == RuntimeSfxArchetype.Building
                ? global::Sound.SoundAssets.BuildingDeath
                : global::Sound.SoundAssets.CreatureDeath,
                global::Sound.GameSfxCategory.SpawnDeath);
        }

        private void PlayWithElement(AudioClip body, global::Sound.GameSfxCategory category)
        {
            AudioClip clip = element switch
            {
                RuntimeSfxElement.Fire => global::Sound.SoundAssets.FireAccent,
                RuntimeSfxElement.Water => global::Sound.SoundAssets.WaterAccent,
                RuntimeSfxElement.Nature => global::Sound.SoundAssets.NatureAccent,
                RuntimeSfxElement.Lightning => global::Sound.SoundAssets.LightningAccent,
                RuntimeSfxElement.Rock => global::Sound.SoundAssets.RockAccent,
                RuntimeSfxElement.Wind => global::Sound.SoundAssets.WindAccent,
                _ => null
            };
            global::Sound.GameSfxPlayer.PlayLayered(body, clip, category);
        }

        private static void Play(AudioClip clip, global::Sound.GameSfxCategory category)
        {
            global::Sound.GameSfxPlayer.Play(clip, category);
        }

        private void OnDestroy()
        {
            if (servedObject == null)
            {
                return;
            }

            servedObject.OnMoved -= PlayMovement;
            servedObject.OnHpDecreased -= PlayHit;
            servedObject.OnHpIncreased -= PlayHeal;
            servedObject.OnDestroyed -= PlayDeath;
            if (ownsAttack)
            {
                servedObject.OnAttack -= PlayAttack;
            }
        }
    }
}
