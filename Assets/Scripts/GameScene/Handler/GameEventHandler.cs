using System.Collections.Generic;
using GameScene.Dto;
using GameScene.Dto.Event;
using GameScene.Object;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Effect;
using Global;
using UnityEngine;

namespace GameScene.Handler
{
    /// <summary>
    /// Applies the one-off events of a frame. Runs before object updates so the attacker of a hit is
    /// still on the client when its position is read; that frame's update may destroy it.
    /// </summary>
    public class GameEventHandler : IFrameInfoHandler<List<GameEvent>>
    {
        private const float ImpactEdgeBias = 0.6f;
        private const string ShockBurstEffectName = "ShockBurst";
        private const string DetectionRangeGizmoCategory = "DetectionRange";

        public void Handler(List<GameEvent> events)
        {
            if (events == null)
            {
                return;
            }

            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent is HitEvent hit)
                {
                    HandleHit(hit);
                }
                else if (gameEvent is ShockEvent shock)
                {
                    HandleShock(shock);
                }
            }
        }

        private static void HandleHit(HitEvent hit)
        {
            ServedObject target = ObjectContainer.Instance.FindById(hit.targetId);
            if (target == null)
            {
                return;
            }

            ServedObject actor = ObjectContainer.Instance.FindById(hit.actorId);
            if (actor != null && StormStagChargeImpactRules.ShouldPlay(actor.ActiveEffects))
            {
                DamagedObjectEffect.SetSelfDestroyEffect(
                    StormStagChargeImpactRules.EffectResourceName,
                    target.GetEdgeWorldPositionTowards(
                        actor.GetActualTransform().position,
                        ImpactEdgeBias));
                return;
            }

            HitEffectController hitEffect = target.GetComponentInChildren<HitEffectController>();
            if (hitEffect == null)
            {
                return;
            }

            if (actor == null)
            {
                hitEffect.PlayHit();
                return;
            }

            hitEffect.PlayHitFrom(actor.GetActualTransform().position);
        }

        private static void HandleShock(ShockEvent shock)
        {
            ServedObject trap = ObjectContainer.Instance.FindById(shock.actorId);
            if (trap == null)
            {
                return;
            }

            GameObject burstPrefab = (GameObject) Resources.Load($"Prefabs/Effects/{ShockBurstEffectName}");
            if (burstPrefab == null)
            {
                WDebug.LogWarning($"Effect prefab '{ShockBurstEffectName}' not found.");
                return;
            }

            GameObject burst = UnityEngine.Object.Instantiate(
                burstPrefab,
                trap.GetActualTransform().position,
                Quaternion.identity);

            // If the trap sent no DetectionRange gizmo, leave the prefab's own scale rather than
            // skip the burst entirely.
            if (trap.TryGetGizmoRadius(DetectionRangeGizmoCategory, out float radius))
            {
                ScaleBurstToDiameter(burst, radius * 2f);
            }
        }

        /// <summary>Scales the burst so the sprite's width matches <paramref name="targetDiameter"/>,
        /// following the same sprite-bounds convention as <c>CircleSkillIndicator.GetScaleForRadius</c>.</summary>
        private static void ScaleBurstToDiameter(GameObject burst, float targetDiameter)
        {
            SpriteRenderer spriteRenderer = burst.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            if (spriteWidth <= 0f)
            {
                return;
            }

            float scale = targetDiameter / spriteWidth;
            Vector3 currentScale = burst.transform.localScale;
            burst.transform.localScale = new Vector3(scale, scale, currentScale.z);
        }
    }
}
