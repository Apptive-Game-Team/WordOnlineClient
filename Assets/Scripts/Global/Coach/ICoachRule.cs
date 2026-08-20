using Coach;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// One coaching situation. A rule only reports whether its problem exists
    /// right now; <see cref="CoachDirector"/> and <see cref="CoachScheduler"/>
    /// own all timing, cooldown and mastery bookkeeping.
    /// </summary>
    public interface ICoachRule
    {
        CoachRuleId Id { get; }

        /// <summary>Localization key in the Onboarding table.</summary>
        string MessageKey { get; }

        /// <summary>Lower wins when several rules are ready in the same frame.</summary>
        int Priority { get; }

        /// <summary>How long the problem must persist before the hint fires.</summary>
        float DwellSeconds { get; }

        /// <summary>Upper bound on how often this hint may appear in one scene visit.</summary>
        int MaxShowsPerSession { get; }

        /// <summary>Which side of the screen the message panel sits on.</summary>
        bool ShowPanelOnRight { get; }

        /// <summary>
        /// True while the problem exists. Going false resets the dwell timer,
        /// and going false while the hint is up counts as the player following it.
        /// </summary>
        bool IsActive();

        /// <summary>Transforms to outline, or null for a message-only hint.</summary>
        Transform[] ResolveTargets();

        /// <summary>Runs once when the hint appears. Used for side effects such as refreshing suggestions.</summary>
        void OnShown();

        /// <summary>Runs once when the hint goes away, however it went away.</summary>
        void OnHidden();
    }
}
