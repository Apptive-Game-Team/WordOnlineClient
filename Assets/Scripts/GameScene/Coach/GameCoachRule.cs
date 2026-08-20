using Coach;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>Shared defaults for the in-game coach rules.</summary>
    public abstract class GameCoachRule : ICoachRule
    {
        public abstract CoachRuleId Id { get; }

        public abstract string MessageKey { get; }

        public abstract int Priority { get; }

        public abstract float DwellSeconds { get; }

        public abstract int MaxShowsPerSession { get; }

        public virtual bool UseAlternatePlacement => false;

        public abstract bool IsActive();

        public virtual Transform[] ResolveTargets() => null;

        public virtual void OnShown()
        {
        }

        public virtual void OnHidden()
        {
        }

        /// <summary>
        /// Scene singletons are torn down between matches, so a rule that
        /// cached one would point at a dead object. Reading through the
        /// singleton every frame keeps that from happening.
        /// </summary>
        protected static bool TryGetInput(out Card.CardInputSender sender)
        {
            sender = Card.CardInputSender.Instance;
            return sender != null;
        }
    }
}
