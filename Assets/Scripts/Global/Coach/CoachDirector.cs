using System.Collections.Generic;
using Coach;
using Data.Coach;
using TutorialScene;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// Drives the coach hint system for one scene. Rules only report whether
    /// their problem exists; every decision about what to show, when, and how
    /// often lives in <see cref="CoachScheduler"/>, which this component feeds
    /// once per frame.
    /// </summary>
    public class CoachDirector : LocalSingletonObject<CoachDirector>
    {
        [SerializeField] private TutorialPanel panel;
        [SerializeField] private CoachHighlighter highlighter;

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
            HideVisible();

            // CardInputSender exposes static events, so a subscription that
            // outlived this scene would keep a destroyed rule alive.
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
            DrainSatisfied();
        }

        /// <summary>
        /// The option is off, or the onboarding tutorial owns the screen. The
        /// tutorial already puts a panel up, and two of them would collide.
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
                panel.Show(
                    rule.MessageKey,
                    null,
                    rule.ShowPanelOnRight ? TutorialPanelSide.Right : TutorialPanelSide.Left);
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
        /// A hint the player actually followed counts toward retiring it, so a
        /// player who has learned the move stops being told about it.
        /// </summary>
        private void DrainSatisfied()
        {
            while (scheduler.TryDequeueSatisfied(out CoachRuleId ruleId))
            {
                string key = ruleId.ToString();
                CoachData.IncreaseSatisfied(key);

                if (CoachData.IsRetired(key))
                {
                    scheduler.SetRetired(ruleId, true);
                }
            }
        }
    }
}
