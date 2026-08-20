using System.Collections.Generic;

namespace Coach
{
    /// <summary>스케줄러가 이번 틱에 요구하는 동작.</summary>
    public enum CoachActionKind
    {
        None,
        Show,
        Hide
    }

    /// <summary>Tick 한 번의 결과.</summary>
    public readonly struct CoachAction
    {
        public readonly CoachActionKind Kind;
        public readonly CoachRuleId RuleId;

        private CoachAction(CoachActionKind kind, CoachRuleId ruleId)
        {
            Kind = kind;
            RuleId = ruleId;
        }

        public static readonly CoachAction None = new CoachAction(CoachActionKind.None, default);

        public static CoachAction Show(CoachRuleId ruleId) => new CoachAction(CoachActionKind.Show, ruleId);

        public static CoachAction Hide(CoachRuleId ruleId) => new CoachAction(CoachActionKind.Hide, ruleId);
    }

    /// <summary>
    /// 어떤 훈수를 언제 띄우고 언제 내릴지 결정한다. 엔진에 의존하지 않고 시간을 인자로만
    /// 받으므로 규칙 하나하나의 조건 판정과 분리해서 검증할 수 있다.
    ///
    /// 억제 장치가 네 겹이다. 조건이 DwellSeconds 동안 이어져야 발동하고, 힌트 사이에는
    /// 전역 쿨다운이 있으며, 무시당할수록 다음 노출이 뒤로 밀리고, 씬 방문당 노출 횟수에
    /// 상한이 있다. 여기에 유저가 힌트를 따랐다는 판정이 쌓이면 바깥에서 규칙을 은퇴시킨다.
    /// </summary>
    public class CoachScheduler
    {
        private class Entry
        {
            public float DwellSeconds;
            public int Priority;
            public int MaxShowsPerSession;

            public bool Active;
            public float DwellTimer;
            public float NextAllowedTime;
            public int ShownThisSession;
            public int IgnoredStreak;
            public bool Retired;
        }

        private readonly Dictionary<CoachRuleId, Entry> entries = new Dictionary<CoachRuleId, Entry>();
        private readonly List<CoachRuleId> order = new List<CoachRuleId>();
        private readonly Queue<CoachRuleId> satisfied = new Queue<CoachRuleId>();

        private readonly float globalCooldownSeconds;
        private readonly float maxVisibleSeconds;
        private readonly float startupGraceSeconds;
        private readonly float satisfyWindowSeconds;
        private readonly float[] ignoreBackoffSeconds;

        private readonly float startTime;

        private bool hasShowing;
        private CoachRuleId showing;
        private float shownAt;
        private float lastHiddenAt;

        private bool hasPending;
        private CoachRuleId pending;
        private float pendingDeadline;

        public CoachScheduler(
            float startTime,
            float globalCooldownSeconds,
            float maxVisibleSeconds,
            float startupGraceSeconds,
            float satisfyWindowSeconds,
            float[] ignoreBackoffSeconds)
        {
            this.startTime = startTime;
            this.globalCooldownSeconds = globalCooldownSeconds;
            this.maxVisibleSeconds = maxVisibleSeconds;
            this.startupGraceSeconds = startupGraceSeconds;
            this.satisfyWindowSeconds = satisfyWindowSeconds;
            this.ignoreBackoffSeconds = ignoreBackoffSeconds != null && ignoreBackoffSeconds.Length > 0
                ? ignoreBackoffSeconds
                : new[] { 0f };

            // 씬 진입 유예가 끝나기 전에는 쿨다운이 발동을 막지 않도록 과거로 밀어 둔다.
            lastHiddenAt = startTime - globalCooldownSeconds;
        }

        public bool HasVisibleHint => hasShowing;

        public void Register(CoachRuleId ruleId, float dwellSeconds, int priority, int maxShowsPerSession, bool retired)
        {
            Entry entry = new Entry
            {
                DwellSeconds = dwellSeconds,
                Priority = priority,
                MaxShowsPerSession = maxShowsPerSession,
                NextAllowedTime = startTime,
                Retired = retired
            };

            if (!entries.ContainsKey(ruleId))
            {
                order.Add(ruleId);
            }

            entries[ruleId] = entry;
        }

        /// <summary>규칙의 문제 상황이 지금 존재하는지 알린다. Tick 전에 매 프레임 갱신한다.</summary>
        public void SetActive(CoachRuleId ruleId, bool active)
        {
            if (entries.TryGetValue(ruleId, out Entry entry))
            {
                entry.Active = active;
            }
        }

        public void SetRetired(CoachRuleId ruleId, bool retired)
        {
            if (entries.TryGetValue(ruleId, out Entry entry))
            {
                entry.Retired = retired;
            }
        }

        /// <summary>힌트를 따른 것으로 판정된 규칙을 하나씩 꺼낸다.</summary>
        public bool TryDequeueSatisfied(out CoachRuleId ruleId)
        {
            if (satisfied.Count == 0)
            {
                ruleId = default;
                return false;
            }

            ruleId = satisfied.Dequeue();
            return true;
        }

        public CoachAction Tick(float now, float deltaTime)
        {
            AccumulateDwell(deltaTime);
            ResolvePending(now);

            if (hasShowing)
            {
                return TickVisible(now);
            }

            if (now - startTime < startupGraceSeconds)
            {
                return CoachAction.None;
            }

            if (now - lastHiddenAt < globalCooldownSeconds)
            {
                return CoachAction.None;
            }

            return TryShow(now);
        }

        private void AccumulateDwell(float deltaTime)
        {
            foreach (CoachRuleId ruleId in order)
            {
                Entry entry = entries[ruleId];
                entry.DwellTimer = entry.Active ? entry.DwellTimer + deltaTime : 0f;
            }
        }

        /// <summary>
        /// 힌트가 내려간 직후에도 유예 시간 안에 조건이 풀리면 따른 것으로 센다.
        /// </summary>
        private void ResolvePending(float now)
        {
            if (!hasPending)
            {
                return;
            }

            if (!entries[pending].Active)
            {
                entries[pending].IgnoredStreak = 0;
                satisfied.Enqueue(pending);
                hasPending = false;
                return;
            }

            if (now > pendingDeadline)
            {
                hasPending = false;
            }
        }

        private CoachAction TickVisible(float now)
        {
            Entry entry = entries[showing];

            if (!entry.Active)
            {
                entry.IgnoredStreak = 0;
                satisfied.Enqueue(showing);
                return HideCurrent(now);
            }

            if (now - shownAt >= maxVisibleSeconds)
            {
                entry.IgnoredStreak++;
                int index = entry.IgnoredStreak - 1;
                if (index >= ignoreBackoffSeconds.Length)
                {
                    index = ignoreBackoffSeconds.Length - 1;
                }
                entry.NextAllowedTime = now + ignoreBackoffSeconds[index];

                hasPending = true;
                pending = showing;
                pendingDeadline = now + satisfyWindowSeconds;

                return HideCurrent(now);
            }

            return CoachAction.None;
        }

        private CoachAction HideCurrent(float now)
        {
            CoachRuleId hidden = showing;
            hasShowing = false;
            lastHiddenAt = now;
            return CoachAction.Hide(hidden);
        }

        private CoachAction TryShow(float now)
        {
            bool found = false;
            CoachRuleId best = default;
            int bestPriority = 0;

            foreach (CoachRuleId ruleId in order)
            {
                Entry entry = entries[ruleId];

                if (entry.Retired || entry.ShownThisSession >= entry.MaxShowsPerSession)
                {
                    continue;
                }

                if (!entry.Active || entry.DwellTimer < entry.DwellSeconds || now < entry.NextAllowedTime)
                {
                    continue;
                }

                if (found && entry.Priority >= bestPriority)
                {
                    continue;
                }

                found = true;
                best = ruleId;
                bestPriority = entry.Priority;
            }

            if (!found)
            {
                return CoachAction.None;
            }

            entries[best].ShownThisSession++;
            hasShowing = true;
            showing = best;
            shownAt = now;

            // 방금 띄운 힌트에 대한 이전 판정 대기는 의미가 없어진다.
            if (hasPending && pending == best)
            {
                hasPending = false;
            }

            return CoachAction.Show(best);
        }
    }
}
