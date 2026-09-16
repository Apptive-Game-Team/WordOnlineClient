using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Global.Serialization
{
    /// <summary>
    /// <see cref="MagicIndicatorValue"/> 를 JSON number 나 <c>{"parameter":...}</c> object 둘 다에서 읽는다.
    /// object 안에는 <c>object</c> 자리로 다른 game object 이름을 더 적을 수 있다.
    /// 읽을 수 없는 모양은 throw 하지 않고 "필드 없음" 으로 돌려준다. indicator document 하나가
    /// 이상해도 조준 화면 전체가 죽으면 안 되기 때문이다.
    /// </summary>
    public sealed class MagicIndicatorValueJsonConverter : JsonConverter<MagicIndicatorValue>
    {
        private const string ObjectProperty = "object";
        private const string ParameterProperty = "parameter";
        private const string FallbackProperty = "fallback";

        public override MagicIndicatorValue ReadJson(JsonReader reader, Type objectType,
            MagicIndicatorValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return default;

                case JsonToken.Integer:
                case JsonToken.Float:
                    return MagicIndicatorValue.FromNumber(Convert.ToSingle(reader.Value));

                case JsonToken.StartObject:
                    return ReadObject(JObject.Load(reader));

                default:
                    // 배열이나 문자열처럼 계약에 없는 모양. 남은 토큰을 소비해야 다음 필드가 밀리지 않는다.
                    reader.Skip();
                    return default;
            }
        }

        public override void WriteJson(JsonWriter writer, MagicIndicatorValue value, JsonSerializer serializer)
        {
            // PlayerPrefs cache 로 다시 나갈 때 쓰인다. 없던 자리는 null 로 적고, 읽을 때 다시 "없음" 이 된다.
            if (!value.IsPresent)
            {
                writer.WriteNull();
                return;
            }

            if (!value.IsParameter)
            {
                writer.WriteValue(value.Number);
                return;
            }

            writer.WriteStartObject();
            if (value.HasObject)
            {
                writer.WritePropertyName(ObjectProperty);
                writer.WriteValue(value.ObjectName);
            }

            writer.WritePropertyName(ParameterProperty);
            writer.WriteValue(value.ParameterName);
            if (value.HasFallback)
            {
                writer.WritePropertyName(FallbackProperty);
                writer.WriteValue(value.FallbackNumber);
            }

            writer.WriteEndObject();
        }

        private static MagicIndicatorValue ReadObject(JObject json)
        {
            if (!json.TryGetValue(ParameterProperty, StringComparison.OrdinalIgnoreCase, out JToken parameterToken) ||
                parameterToken.Type != JTokenType.String)
            {
                return default;
            }

            var parameterName = parameterToken.Value<string>();
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return default;
            }

            // object 는 선택 항목이지만, 있는데 문자열이 아니면 parameter 가 계약을 어긴 것과 같은 취급이다.
            string objectName = null;
            if (json.TryGetValue(ObjectProperty, StringComparison.OrdinalIgnoreCase, out JToken objectToken))
            {
                if (objectToken.Type != JTokenType.String)
                {
                    return default;
                }

                objectName = objectToken.Value<string>();
                if (string.IsNullOrWhiteSpace(objectName))
                {
                    return default;
                }
            }

            float? fallback = null;
            if (json.TryGetValue(FallbackProperty, StringComparison.OrdinalIgnoreCase, out JToken fallbackToken) &&
                (fallbackToken.Type == JTokenType.Integer || fallbackToken.Type == JTokenType.Float))
            {
                fallback = fallbackToken.Value<float>();
            }

            if (objectName == null)
            {
                return fallback.HasValue
                    ? MagicIndicatorValue.FromParameter(parameterName, fallback.Value)
                    : MagicIndicatorValue.FromParameter(parameterName);
            }

            return fallback.HasValue
                ? MagicIndicatorValue.FromParameter(objectName, parameterName, fallback.Value)
                : MagicIndicatorValue.FromParameter(objectName, parameterName);
        }
    }
}
