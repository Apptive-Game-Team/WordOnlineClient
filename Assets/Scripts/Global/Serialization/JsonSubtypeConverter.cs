using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Global.Serialization
{
    /// <summary>
    /// Reads a polymorphic hierarchy by looking at a discriminator field the server already sends.
    /// Derive a concrete converter per hierarchy and put it on the base type with
    /// <see cref="JsonConverterAttribute"/>.
    /// Unknown or missing discriminators yield <c>null</c> so callers can decide how to react.
    /// </summary>
    public abstract class JsonSubtypeConverter<TBase> : JsonConverter<TBase> where TBase : class
    {
        private readonly string discriminatorName;
        private readonly IReadOnlyDictionary<string, Type> subtypesByDiscriminator;

        protected JsonSubtypeConverter(string discriminatorName, IReadOnlyDictionary<string, Type> subtypes)
        {
            this.discriminatorName = discriminatorName;
            subtypesByDiscriminator = new Dictionary<string, Type>(subtypes, StringComparer.OrdinalIgnoreCase);
        }

        // Subtypes are written through the default contract; only reading needs the discriminator.
        public override bool CanWrite => false;

        public override TBase ReadJson(JsonReader reader, Type objectType, TBase existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject json = JObject.Load(reader);

            // Asking for a concrete subtype skips the discriminator: the caller already picked the shape.
            // The attribute on the base type is inherited, so this path would otherwise be unreachable.
            Type subtype = objectType != typeof(TBase) ? objectType : null;

            if (subtype == null && !TryResolveSubtype(json, out subtype))
            {
                return null;
            }

            var value = (TBase)Activator.CreateInstance(subtype);

            using (JsonReader subtypeReader = json.CreateReader())
            {
                // Populate instead of ToObject: the converter attribute on the base type is inherited by
                // every subtype, so ToObject would re-enter this converter forever.
                serializer.Populate(subtypeReader, value);
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, TBase value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{GetType().Name} only reads.");
        }

        private bool TryResolveSubtype(JObject json, out Type subtype)
        {
            subtype = null;

            if (!json.TryGetValue(discriminatorName, StringComparison.OrdinalIgnoreCase, out JToken token))
            {
                return false;
            }

            string discriminator = token.Type == JTokenType.String ? token.Value<string>() : null;

            return discriminator != null && subtypesByDiscriminator.TryGetValue(discriminator, out subtype);
        }
    }
}
