using System.Collections.Generic;
using Global;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Holds every matching server an admin may switch to and the one currently in use.
    /// <see cref="ServerList.MatchingServer"/> reads <see cref="Current"/>, so switching here
    /// redirects every later matching server request.
    ///
    /// Non-admin sessions never call <see cref="Select"/> and therefore always stay on
    /// <see cref="BuildDefault"/>.
    /// </summary>
    public static class MatchingServerCatalog
    {
        private const string SelectedKeyPreference = "MatchingServer.SelectedKey";

        public static readonly MatchingServerOption Local =
            new MatchingServerOption("local", "localhost", new Server("localhost", "localhost", 6209));

        public static readonly MatchingServerOption Dev =
            new MatchingServerOption("dev", "dev", new Server("dev", "dev.lobby.ac.yunseong.dev", 443, true));

        public static readonly MatchingServerOption Deploy =
            new MatchingServerOption("deploy", "deploy", new Server("deploy", "lobby.ac.yunseong.dev", 443, true));

        /// <summary>
        /// Options in auto-select priority order: the first healthy one wins when the admin
        /// has no stored choice.
        /// </summary>
        public static readonly IReadOnlyList<MatchingServerOption> Options = new[] { Local, Dev, Deploy };

        /// <summary>
        /// Server used before an admin picks anything. Builds have always pointed at the dev
        /// matching server (the DEV_BUILD switch in ServerList was disabled), so that stays the
        /// default and the dropdown is the only way to reach the other servers.
        /// </summary>
        private static readonly MatchingServerOption BuildDefault = Dev;

        public static MatchingServerOption Current { get; private set; } = BuildDefault;

        public static void Select(MatchingServerOption option)
        {
            if (option == null || option == Current)
                return;

            Current = option;
            PlayerPrefs.SetString(SelectedKeyPreference, option.Key);
            PlayerPrefs.Save();
            WDebug.Log($"[MatchingServerCatalog] matching server switched to {option.DisplayName} ({option.Server.url})");
        }

        /// <summary>
        /// Returns the option the admin last picked on this device, or null when there is none.
        /// </summary>
        public static MatchingServerOption FindStoredOption()
        {
            string storedKey = PlayerPrefs.GetString(SelectedKeyPreference, null);
            if (string.IsNullOrEmpty(storedKey))
                return null;

            foreach (MatchingServerOption option in Options)
            {
                if (option.Key == storedKey)
                    return option;
            }

            return null;
        }
    }
}
