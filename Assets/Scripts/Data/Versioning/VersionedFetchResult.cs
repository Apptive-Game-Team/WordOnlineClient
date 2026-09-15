namespace Data.Versioning
{
    /// <summary>
    /// How a versioned fetch ended. The failures are kept apart because they call for different
    /// handling: nothing reached the server on a timeout or a connection failure, the server
    /// refused the request on an HTTP error, and the body it did send was unusable on a parse
    /// failure.
    /// </summary>
    public enum VersionedFetchOutcome
    {
        Success,
        Timeout,
        ConnectionFailed,
        HttpError,
        ParseFailed
    }

    /// <summary>
    /// Outcome of one <see cref="VersionedApiClient{TResponse}.Get"/> call. It replaces handing the
    /// caller a default payload on failure, which made every failure look the same as a response
    /// that carried nothing.
    /// </summary>
    public class VersionedFetchResult<TResponse>
    {
        private VersionedFetchResult(VersionedFetchOutcome outcome, TResponse response, long statusCode, string error)
        {
            Outcome = outcome;
            Response = response;
            StatusCode = statusCode;
            Error = error;
        }

        public VersionedFetchOutcome Outcome { get; }

        /// <summary>Payload of a successful fetch, and <c>default</c> on every failure.</summary>
        public TResponse Response { get; }

        /// <summary>HTTP status the server answered with, or 0 when no response arrived.</summary>
        public long StatusCode { get; }

        /// <summary>Message for the log. Empty on success.</summary>
        public string Error { get; }

        public bool IsSuccess => Outcome == VersionedFetchOutcome.Success;

        public static VersionedFetchResult<TResponse> Succeeded(TResponse response, long statusCode)
        {
            return new VersionedFetchResult<TResponse>(VersionedFetchOutcome.Success, response, statusCode, string.Empty);
        }

        public static VersionedFetchResult<TResponse> TimedOut(string error)
        {
            return new VersionedFetchResult<TResponse>(VersionedFetchOutcome.Timeout, default, 0, error);
        }

        public static VersionedFetchResult<TResponse> ConnectionFailed(string error)
        {
            return new VersionedFetchResult<TResponse>(VersionedFetchOutcome.ConnectionFailed, default, 0, error);
        }

        public static VersionedFetchResult<TResponse> HttpError(long statusCode, string error)
        {
            return new VersionedFetchResult<TResponse>(VersionedFetchOutcome.HttpError, default, statusCode, error);
        }

        public static VersionedFetchResult<TResponse> ParseFailed(long statusCode, string error)
        {
            return new VersionedFetchResult<TResponse>(VersionedFetchOutcome.ParseFailed, default, statusCode, error);
        }
    }
}
