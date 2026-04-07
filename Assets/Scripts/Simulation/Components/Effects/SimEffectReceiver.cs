using System;

namespace Simulation.Core
{
    /// <summary>
    /// Receives effects on collision and applies corresponding status effects.
    /// Mirrors server's CommonEffectReceiver.
    /// </summary>
    public class SimEffectReceiver : SimComponent, ISimCollidable
    {
        private static readonly Fix64 THREE = Fix64.FromInt(3);

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }

        public void OnCollision(SimGameObject other) { }

        public void OnReceive(Effect effect)
        {
            if (SimEffectImmuneChart.IsImmuneTo(GameObject, effect)) return;

            switch (effect)
            {
                case Effect.Wet:
                    ApplyEffect(SimStatusEffectKey.Wet_Receive,
                        () => new SimWetEffect(THREE, SimStatusEffectKey.Wet_Receive),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    if (GameObject.Element.Has(ElementType.NATURE))
                        ApplyEffect(SimStatusEffectKey.DOTHeal_NatureWithWaterField,
                            () => new SimDOTEffect(THREE, -3, ElementType.NONE, SimStatusEffectKey.DOTHeal_NatureWithWaterField),
                            SimEffectApplyPolicy.RefreshDuration, THREE);
                    break;

                case Effect.Burn:
                    ApplyEffect(SimStatusEffectKey.Burn_Receive,
                        () => new SimBurnEffect(THREE, SimStatusEffectKey.Burn_Receive),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    ApplyEffect(SimStatusEffectKey.DOTDeal_Burn,
                        () => new SimDOTEffect(THREE, 3, ElementType.FIRE, SimStatusEffectKey.DOTDeal_Burn),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    break;

                case Effect.Shock:
                    ApplyEffect(SimStatusEffectKey.Shock_Receive,
                        () => new SimShockEffect(Fix64.Half, SimStatusEffectKey.Shock_Receive),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    break;

                case Effect.Snared:
                    ApplyEffect(SimStatusEffectKey.Snared_Receive,
                        () => new SimSnaredEffect(THREE, 5, SimStatusEffectKey.Snared_Receive),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    break;

                case Effect.LeafFieldHeal:
                    ApplyEffect(SimStatusEffectKey.DOTHeal_NatureField,
                        () => new SimDOTEffect(THREE, -1, ElementType.NONE, SimStatusEffectKey.DOTHeal_NatureField),
                        SimEffectApplyPolicy.RefreshDuration, THREE);
                    break;

                case Effect.Sandstorm:
                    ApplyEffect(SimStatusEffectKey.DOT_SandStorm,
                        () => new SimDOTEffect(Fix64.Half, 1, ElementType.NONE, SimStatusEffectKey.DOT_SandStorm),
                        SimEffectApplyPolicy.RefreshDuration, Fix64.Half);
                    break;
            }
        }

        public void OnReceiveKnockback(SimVector3 direction, Fix64 proximity)
        {
            ApplyEffect(SimStatusEffectKey.Knockback_Receive,
                () => new SimKnockbackEffect(direction, proximity, SimStatusEffectKey.Knockback_Receive),
                SimEffectApplyPolicy.Ignore, THREE);
        }

        private void ApplyEffect<T>(SimStatusEffectKey key, Func<T> factory,
            SimEffectApplyPolicy policy, Fix64 duration) where T : SimStatusEffect
        {
            var existing = GetEffectByKey(key);
            if (existing != null)
            {
                switch (policy)
                {
                    case SimEffectApplyPolicy.Ignore: return;
                    case SimEffectApplyPolicy.RefreshDuration: existing.Refresh(duration); return;
                    case SimEffectApplyPolicy.ExtendDuration: existing.Extend(duration); return;
                }
                return;
            }
            var created = factory();
            GameObject.AddComponent(created);
        }

        private SimStatusEffect GetEffectByKey(SimStatusEffectKey key)
        {
            foreach (var c in GameObject.GetComponents<SimStatusEffect>())
                if (c.Key == key) return c;
            return null;
        }
    }
}
