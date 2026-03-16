namespace Data.Util
{
    public class VersionParameter
    {
        private readonly long? _currentVersion;

        public VersionParameter(long? currentVersion = null)
        {
            _currentVersion = currentVersion;
        }

        public string AppendToUrl(string baseUrl)
        {
            if (_currentVersion.HasValue)
            {
                string separator = baseUrl.Contains("?") ? "&" : "?";
                return $"{baseUrl}{separator}currentVersion={_currentVersion.Value}";
            }
            return baseUrl;
        }
    }
}
