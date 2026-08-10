using System;

namespace Data.Net
{
    /// <summary>
    /// 서버 URL 조립을 담당하는 불변 값 타입.
    ///
    /// 스킴 변환, 경로 결합, 쿼리 이스케이프를 타입 안에서 처리해
    /// 호출부가 문자열을 직접 자르거나 잇지 않게 한다.
    /// <see cref="Server.Api"/> / <see cref="Server.WebSocket"/>으로 얻는다.
    /// </summary>
    public readonly struct ServerEndpoint : IEquatable<ServerEndpoint>
    {
        private const string HttpScheme = "http";
        private const string HttpsScheme = "https";
        private const string WsScheme = "ws";
        private const string WssScheme = "wss";

        private readonly Uri uri;

        private ServerEndpoint(Uri uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// 파싱에 실패하면 예외를 던진다. 코드에 박힌 상수 URL에만 쓴다.
        /// 서버가 내려준 값처럼 신뢰할 수 없는 입력에는 <see cref="TryOf"/>를 쓴다.
        /// </summary>
        public static ServerEndpoint Of(string url)
        {
            if (!TryOf(url, out ServerEndpoint endpoint))
            {
                throw new ArgumentException($"Unsupported server URL: {url}", nameof(url));
            }

            return endpoint;
        }

        /// <summary>http, https, ws, wss 절대 URL만 통과시킨다.</summary>
        public static bool TryOf(string url, out ServerEndpoint endpoint)
        {
            endpoint = default;

            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri parsed)) return false;
            if (!IsSupportedScheme(parsed.Scheme)) return false;

            endpoint = new ServerEndpoint(parsed);
            return true;
        }

        /// <summary>기본값으로 만들어진 엔드포인트인지 여부.</summary>
        public bool HasValue => uri != null;

        public string Host => HasValue ? uri.Host : string.Empty;

        /// <summary>세그먼트를 각각 이스케이프해 현재 경로 뒤에 잇는다.</summary>
        public ServerEndpoint Path(params string[] segments)
        {
            RequireValue();

            if (segments == null || segments.Length == 0) return this;

            UriBuilder builder = new UriBuilder(uri);
            string appended = string.Join("/", Array.ConvertAll(segments, Uri.EscapeDataString));
            builder.Path = $"{builder.Path.TrimEnd('/')}/{appended}";
            return new ServerEndpoint(builder.Uri);
        }

        /// <summary>키와 값을 이스케이프해 쿼리에 덧붙인다. ? 와 &amp; 구분은 내부에서 정한다.</summary>
        public ServerEndpoint Query(string key, string value)
        {
            RequireValue();

            UriBuilder builder = new UriBuilder(uri);
            string pair = $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value ?? string.Empty)}";
            string existing = builder.Query.TrimStart('?');
            builder.Query = existing.Length == 0 ? pair : $"{existing}&{pair}";
            return new ServerEndpoint(builder.Uri);
        }

        /// <summary>http를 ws로, https를 wss로 바꾼다. 이미 웹소켓 스킴이면 그대로 둔다.</summary>
        public ServerEndpoint AsWebSocket()
        {
            RequireValue();

            UriBuilder builder = new UriBuilder(uri) { Scheme = ToWebSocketScheme(uri.Scheme) };
            return new ServerEndpoint(builder.Uri);
        }

        public override string ToString() => HasValue ? uri.AbsoluteUri : string.Empty;

        public static implicit operator string(ServerEndpoint endpoint) => endpoint.ToString();

        public bool Equals(ServerEndpoint other) => uri == other.uri;

        public override bool Equals(object obj) => obj is ServerEndpoint other && Equals(other);

        public override int GetHashCode() => HasValue ? uri.GetHashCode() : 0;

        private void RequireValue()
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("ServerEndpoint has no URL. Build it with Of or TryOf.");
            }
        }

        private static bool IsSupportedScheme(string scheme) =>
            scheme == HttpScheme || scheme == HttpsScheme || scheme == WsScheme || scheme == WssScheme;

        private static string ToWebSocketScheme(string scheme) => scheme switch
        {
            HttpsScheme or WssScheme => WssScheme,
            HttpScheme or WsScheme => WsScheme,
            _ => throw new InvalidOperationException($"Unsupported scheme: {scheme}")
        };
    }
}
