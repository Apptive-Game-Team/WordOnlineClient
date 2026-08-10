using System.Collections.Generic;
using GameScene.Dto.Event;
using GameScene.Object;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Effect;

namespace GameScene.Handler
{
    /// <summary>
    /// Applies the one-off events of a frame. Runs before object updates so the attacker of a hit is
    /// still on the client when its position is read; that frame's update may destroy it.
    /// </summary>
    public class GameEventHandler : IFrameInfoHandler<List<GameEvent>>
    {
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
            }
        }

        private static void HandleHit(HitEvent hit)
        {
            ServedObject target = ObjectContainer.Instance.FindById(hit.targetId);
            if (target == null)
            {
                return;
            }

            HitEffectController hitEffect = target.GetComponentInChildren<HitEffectController>();
            if (hitEffect == null)
            {
                return;
            }

            ServedObject actor = ObjectContainer.Instance.FindById(hit.actorId);
            if (actor == null)
            {
                hitEffect.PlayHit();
                return;
            }

            hitEffect.PlayHitFrom(actor.GetActualTransform().position);
        }
    }
}
