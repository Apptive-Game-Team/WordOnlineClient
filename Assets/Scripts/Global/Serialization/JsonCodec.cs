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
            value = default;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonConvert.DeserializeObject<T>(json, Settings);
            }
            catch (JsonException)
            {
                return false;
            }

            return value != null;
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
