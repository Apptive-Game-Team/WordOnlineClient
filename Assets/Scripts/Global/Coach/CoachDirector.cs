using System.Collections.Generic;
using Coach;
using Data.Coach;
using TutorialScene;
using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>Closes the current hint. Sits on the panel, so it hides with it.</summary>
        [SerializeField] private Button closeButton;

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
            DrainVerdicts();
        }

        /// <summary>
        /// Closes the hint on screen. Wired to the panel's close button, and
        /// safe to call from anywhere that wants the screen back.
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
                panel.Show(rule.MessageKey, null);

                // TutorialPanel.Show pins the panel to positions tuned for the
                // onboarding flow, which may cover the field. A hint must not,
                // so the placement is overridden right after.
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
        /// Both verdicts retire a hint, for opposite reasons. Following it means
        /// the player has learned the move; closing it means they do not want to
        /// be told. Either way the hint has done all the good it is going to do.
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
