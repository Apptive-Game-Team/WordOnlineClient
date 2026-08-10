using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Global.Serialization
{
    /// <summary>
    /// Serializes <see cref="Vector3"/> as its three axes only. Without this converter Json.NET walks
    /// derived properties such as <c>normalized</c> and <c>magnitude</c>.
    /// </summary>
    public sealed class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Vector3.zero;
            }

            JObject json = JObject.Load(reader);

            return new Vector3(ReadAxis(json, "x"), ReadAxis(json, "y"), ReadAxis(json, "z"));
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        private static float ReadAxis(JObject json, string axis)
        {
            if (!json.TryGetValue(axis, StringComparison.OrdinalIgnoreCase, out JToken token))
            {
                return 0f;
            }

            return token.Type == JTokenType.Null ? 0f : token.Value<float>();
        }
    }
}
