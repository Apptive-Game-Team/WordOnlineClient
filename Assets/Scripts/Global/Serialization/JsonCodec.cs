using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Global.Serialization
{
    /// <summary>
    /// Single entry point for JSON in the client so serializer configuration lives in one place.
    /// </summary>
    public static class JsonCodec
    {
        private static readonly JsonSerializerSettings Settings = CreateSettings();

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        /// <summary>
        /// Deserializes without throwing on malformed payloads. Returns false when nothing usable was parsed.
        /// </summary>
        public static bool TryDeserialize<T>(string json, out T value)
        {
            return TryDeserialize(json, out value, out _);
        }

        /// <summary>
        /// Same as <see cref="TryDeserialize{T}(string, out T)"/> but hands back why the payload was rejected,
        /// so callers can log it next to the endpoint they were reading.
        /// </summary>
        public static bool TryDeserialize<T>(string json, out T value, out string error)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "empty response body";
                return false;
            }

            try
            {
                value = JsonConvert.DeserializeObject<T>(json, Settings);
            }
            // Json.NET wraps its own failures, but a converter reaching a type the IL2CPP linker stripped
            // surfaces as a plain reflection error instead.
            catch (Exception exception) when (exception is JsonException
                                              || exception is MissingMethodException
                                              || exception is InvalidCastException)
            {
                error = exception.Message;
                return false;
            }

            if (value == null)
            {
                error = $"payload parsed to null as {typeof(T).Name}";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Shortens a response body for log lines so a large payload does not flood the console.
        /// </summary>
        public static string Excerpt(string json, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "<empty>";
            }

            return json.Length <= maxLength ? json : json.Substring(0, maxLength) + "…";
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings
            {
                // Server payloads carry fields the client does not model yet.
                MissingMemberHandling = MissingMemberHandling.Ignore,
                // Fields initialized at declaration must be replaced, otherwise Json.NET appends into them.
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                // Server payloads may send null for numeric fields, which non-nullable value types cannot take.
                // Skipping nulls keeps the field default instead of throwing.
                NullValueHandling = NullValueHandling.Ignore,
                Converters =
                {
                    new Vector3JsonConverter(),
                    new StringEnumConverter()
                }
            };
        }
    }
}
