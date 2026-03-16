using UnityEngine;
using UnityEngine.Networking;

namespace Data.Versioning
{
    public class VersionParameter
    {
        private readonly string _prefsKey;

        public VersionParameter(string prefsKey)
        {
            _prefsKey = prefsKey;
        }

        public bool HasVersion => PlayerPrefs.HasKey(_prefsKey);

        public string CurrentVersion => PlayerPrefs.GetString(_prefsKey, null);

        public void Save(string version)
        {
            PlayerPrefs.SetString(_prefsKey, version);
            PlayerPrefs.Save();
        }

        public string ToQueryString()
        {
            if (!HasVersion) return string.Empty;
            return $"?currentVersion={UnityWebRequest.EscapeURL(CurrentVersion)}";
        }
    }
}
