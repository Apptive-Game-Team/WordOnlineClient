using System.Collections.Generic;
using Coach;
using Data.Coach;
using TutorialScene;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// 한 씬의 훈수 시스템을 굴린다. 규칙은 문제가 있는지만 알리고, 무엇을 언제 얼마나
    /// 자주 띄울지는 전부 <see cref="CoachScheduler"/>가 정한다. 이 컴포넌트는 매 프레임
    /// 스케줄러에 상태를 넣어 주는 역할이다.
    /// </summary>
    public class CoachDirector : LocalSingletonObject<CoachDirector>
    {
        [SerializeField] private TutorialPanel panel;
        [SerializeField] private CoachHighlighter highlighter;

        /// <summary>지금 뜬 힌트를 닫는다. 패널에 붙어 있어 패널과 같이 사라진다.</summary>
        [SerializeField] private UnityEngine.UI.Button closeButton;

        [Header("Placement")]
        [SerializeField] private CoachPanelPlacement primaryPlacement = new CoachPanelPlacement
        {
            anchor = new Vector2(0.5f, 1f),
            pivot = new Vector2(0.5f, 1f),
            offset = new Vector2(0f, -40f)
        };

        [SerializeField] private CoachPanelPlacement alternatePlacement = new CoachPanelPlacement
        {
            anchor = new Vector2(1f, 1f),
            pivot = new Vector2(1f, 1f),
            offset = new Vector2(-40f, -40f)
        };

        [Header("Timing")]
        [SerializeField] private float globalCooldownSeconds = CoachTuning.GlobalCooldownSeconds;
        [SerializeField] private float maxVisibleSeconds = CoachTuning.MaxVisibleSeconds;
        [SerializeField] private float startupGraceSeconds = CoachTuning.StartupGraceSeconds;
        [SerializeField] private float satisfyWindowSeconds = CoachTuning.SatisfyWindowSeconds;

        private readonly Dictionary<CoachRuleId, ICoachRule> rules = new Dictionary<CoachRuleId, ICoachRule>();

        private CoachScheduler scheduler;
        private ICoachRule visibleRule;

        protected override void Awake()
        {
            base.Awake();
            BuildRules();
        }

        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(DismissVisible);
            }

            foreach (ICoachRule rule in rules.Values)
            {
                if (rule is ICoachRuleLifecycle lifecycle)
                {
                    lifecycle.Initialize();
                }
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(DismissVisible);
            }

            HideVisible();

            // CardInputSender는 정적 이벤트를 노출한다. 구독이 씬보다 오래 남으면
            // 파괴된 규칙을 계속 붙잡고 있게 된다.
            foreach (ICoachRule rule in rules.Values)
            {
                if (rule is ICoachRuleLifecycle lifecycle)
                {
                    lifecycle.Dispose();
                }
            }
        }

        private void BuildRules()
        {
            scheduler = new CoachScheduler(
                Time.time,
                globalCooldownSeconds,
                maxVisibleSeconds,
                startupGraceSeconds,
                satisfyWindowSeconds,
                CoachTuning.IgnoreBackoffSeconds);

            foreach (ICoachRuleProvider provider in GetComponents<ICoachRuleProvider>())
            {
                foreach (ICoachRule rule in provider.CreateRules())
                {
                    if (rule == null || rules.ContainsKey(rule.Id))
                    {
                        continue;
                    }

                    rules[rule.Id] = rule;

                    scheduler.Register(
                        rule.Id,
                        rule.DwellSeconds,
                        rule.Priority,
                        rule.MaxShowsPerSession,
                        CoachData.IsRetired(rule.Id.ToString()));
                }
            }
        }

        private void Update()
        {
            if (rules.Count == 0)
            {
                return;
            }

            if (IsSuppressed())
            {
                HideVisible();
                return;
            }

            foreach (KeyValuePair<CoachRuleId, ICoachRule> pair in rules)
            {
                scheduler.SetActive(pair.Key, pair.Value.IsActive());
            }

            CoachAction action = scheduler.Tick(Time.time, Time.deltaTime);
            Apply(action);
            DrainVerdicts();
        }

        /// <summary>
        /// 화면에 뜬 힌트를 닫는다. 패널의 닫기 버튼에 연결되며, 화면을 돌려받고 싶은
        /// 곳이면 어디서 불러도 안전하다.
        /// </summary>
        public void DismissVisible()
        {
            if (scheduler == null || !scheduler.HasVisibleHint)
            {
                return;
            }

            Apply(scheduler.Dismiss(Time.time));
            DrainVerdicts();
        }

        /// <summary>
        /// 옵션이 꺼져 있거나, 온보딩 튜토리얼이 화면을 쓰고 있다. 튜토리얼도 패널을
        /// 띄우므로 둘이 겹치면 안 된다.
        /// </summary>
        private bool IsSuppressed()
        {
            if (!CoachData.Enabled)
            {
                return true;
            }

            OnboardingProgress progress = GlobalTutorialManager.GetCurrentProgress();
            return progress != OnboardingProgress.None
                   && progress != OnboardingProgress.Completed
                   && progress != OnboardingProgress.Skipped;
        }

        private void Apply(CoachAction action)
        {
            switch (action.Kind)
            {
                case CoachActionKind.Show:
                    Show(rules[action.RuleId]);
                    break;

                case CoachActionKind.Hide:
                    HideVisible();
                    break;
            }
        }

        private void Show(ICoachRule rule)
        {
            visibleRule = rule;
            rule.OnShown();

            if (highlighter != null)
            {
                highlighter.Highlight(rule.ResolveTargets());
            }

            if (panel != null)
            {
                panel.Show(rule.MessageKey, null);

                // TutorialPanel.Show는 온보딩에 맞춘 위치로 패널을 고정하는데, 그 자리는
                // 필드를 가릴 수 있다. 힌트는 가리면 안 되므로 곧바로 배치를 덮어쓴다.
                CoachPanelPlacement placement = rule.UseAlternatePlacement ? alternatePlacement : primaryPlacement;
                placement.ApplyTo(panel.RootRectTransform);
            }
        }

        private void HideVisible()
        {
            if (visibleRule == null)
            {
                return;
            }

            visibleRule.OnHidden();
            visibleRule = null;

            if (highlighter != null)
            {
                highlighter.Clear();
            }

            if (panel != null)
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// 두 판정 모두 힌트를 은퇴시키지만 이유는 정반대다. 따랐다는 것은 유저가 그 동작을
        /// 익혔다는 뜻이고, 닫았다는 것은 듣고 싶지 않다는 뜻이다. 어느 쪽이든 그 힌트가
        /// 더 해 줄 수 있는 일은 없다.
        /// </summary>
        private void DrainVerdicts()
        {
            while (scheduler.TryDequeueSatisfied(out CoachRuleId satisfiedRule))
            {
                CoachData.IncreaseSatisfied(satisfiedRule.ToString());
                RetireIfEarned(satisfiedRule);
            }

            while (scheduler.TryDequeueDismissed(out CoachRuleId dismissedRule))
            {
                CoachData.IncreaseDismissed(dismissedRule.ToString());
                RetireIfEarned(dismissedRule);
            }
        }

        private void RetireIfEarned(CoachRuleId ruleId)
        {
            if (CoachData.IsRetired(ruleId.ToString()))
            {
                scheduler.SetRetired(ruleId, true);
            }
        }
    }
}
