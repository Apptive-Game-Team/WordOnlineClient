using Coach;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>게임 씬 훈수 규칙이 공유하는 기본값.</summary>
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
        /// 씬 싱글턴은 매치 사이에 사라지므로, 캐시해 두면 죽은 오브젝트를 가리키게 된다.
        /// 매 프레임 싱글턴을 새로 읽어 그 상황을 피한다.
        /// </summary>
        protected static bool TryGetInput(out Card.CardInputSender sender)
        {
            sender = Card.CardInputSender.Instance;
            return sender != null;
        }
    }
}
