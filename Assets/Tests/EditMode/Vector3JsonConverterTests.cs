using Global.Serialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace WordOnline.Tests
{
    public class Vector3JsonConverterTests
    {
        /// <summary>
        /// 컨버터가 없으면 Json.NET 이 <c>normalized</c>, <c>magnitude</c> 같은 파생 프로퍼티까지 직렬화한다.
        /// </summary>
        [Test]
        public void SerializesAxesOnly()
        {
            JObject json = JObject.Parse(JsonCodec.Serialize(new Vector3(1.5f, -2f, 0.25f)));

            Assert.AreEqual(3, json.Count);
            Assert.AreEqual(1.5f, json["x"].Value<float>());
            Assert.AreEqual(-2f, json["y"].Value<float>());
            Assert.AreEqual(0.25f, json["z"].Value<float>());
        }

        [Test]
        public void RoundTripsThroughJson()
        {
            Vector3 original = new Vector3(3.5f, 0f, -7.25f);

            Vector3 restored = JsonCodec.Deserialize<Vector3>(JsonCodec.Serialize(original));

            Assert.AreEqual(original, restored);
        }

        [Test]
        public void OmittedAxisFallsBackToZero()
        {
            Vector3 value = JsonCodec.Deserialize<Vector3>(@"{""x"":4.0}");

            Assert.AreEqual(new Vector3(4f, 0f, 0f), value);
        }

        [Test]
        public void NullAxisFallsBackToZero()
        {
            Vector3 value = JsonCodec.Deserialize<Vector3>(@"{""x"":4.0,""y"":null,""z"":1.0}");

            Assert.AreEqual(new Vector3(4f, 0f, 1f), value);
        }
    }
}
