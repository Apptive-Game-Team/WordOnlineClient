using Scripts.Global;

namespace Scripts.Data.Util
{
    public class UrlUtil
    {
        public static string httpToWebSocket(string url, string token)
        {
            if (url.StartsWith("https://"))
            {
                return "wss://" + url.Substring(8) + "/ws?token=" + token;
            }
            else if (url.StartsWith("http://"))
            {
                return "ws://" + url.Substring(7) + "/ws?token=" + token;
            }
            else
            {
                WDebug.LogError($"Invalid URL scheme: {url}");
                return string.Empty;
            }
        }
        public static string httpToWs(string url)
        {
            if (url.StartsWith("https://"))
            {
                return "wss://" + url.Substring(8);
            }
            else if (url.StartsWith("http://"))
            {
                return "ws://" + url.Substring(7);
            }
            else
            {
                WDebug.LogError($"Invalid URL scheme: {url}");
                return string.Empty;
            }
        }
        
        public static string urlToDomain(string url)
        {
            try
            {
                var uri = new System.Uri(url);
                return uri.Host;
            }
            catch (System.Exception e)
            {
                WDebug.LogError($"Invalid URL: {url}, Exception: {e.Message}");
                return string.Empty;
            }
        }
    }
}