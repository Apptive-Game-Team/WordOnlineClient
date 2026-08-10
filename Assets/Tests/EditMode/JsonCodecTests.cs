using Global.Serialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace WordOnline.Tests
{
    /// <summary>
    /// JsonUtility 에서 Json.NET 으로 넘어오며 깨진 서버 페이로드 형태들을 고정한다.
    /// JsonUtility 는 null 을 조용히 0 으로 바꾸고 생성자 없이 객체를 할당했지만, Json.NET 은 둘 다 예외를 던진다.
    /// </summary>
    public class JsonCodecTests
    {
        private class NumericPayload
        {
            public string name;
            public float value;
            public int count;
        }

        /// <summary>
        /// 생성자를 하나라도 선언하면 암묵적 기본 생성자가 사라진다.
        /// 인자 있는 생성자가 둘이면 Json.NET 이 어느 쪽도 고르지 못하므로 기본 생성자가 반드시 필요하다.
        /// </summary>
        private class MultiConstructorPayload
        {
            public long id;
            public string name;

            public MultiConstructorPayload() { }

            public MultiConstructorPayload(long id)
            {
                this.id = id;
            }

            public MultiConstructorPayload(long id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        [Test]
        public void NullNumberFallsBackToDefault()
        {
            NumericPayload payload = JsonCodec.Deserialize<NumericPayload>(
                @"{""name"":""MaxHP"",""value"":null,""count"":null}");

            Assert.AreEqual("MaxHP", payload.name);
            Assert.AreEqual(0f, payload.value);
            Assert.AreEqual(0, payload.count);
        }

        [Test]
        public void NullNumberInsideArrayDoesNotAbortTheWholePayload()
        {
            NumericPayload[] payloads = JsonCodec.Deserialize<NumericPayload[]>(
                @"[{""name"":""a"",""value"":1.5},{""name"":""b"",""value"":null},{""name"":""c"",""value"":2.5}]");

            Assert.AreEqual(3, payloads.Length);
            Assert.AreEqual(1.5f, payloads[0].value);
            Assert.AreEqual(0f, payloads[1].value);
            Assert.AreEqual(2.5f, payloads[2].value);
        }

        [Test]
        public void PresentNumberStillWins()
        {
            NumericPayload payload = JsonCodec.Deserialize<NumericPayload>(@"{""value"":42.5}");

            Assert.AreEqual(42.5f, payload.value);
        }

        [Test]
        public void NullStringFallsBackToDefault()
        {
            NumericPayload payload = JsonCodec.Deserialize<NumericPayload>(@"{""name"":null,""value"":1.0}");

            Assert.IsNull(payload.name);
            Assert.AreEqual(1f, payload.value);
        }

        [Test]
        public void TypeWithMultipleConstructorsDeserializes()
        {
            MultiConstructorPayload payload = JsonCodec.Deserialize<MultiConstructorPayload>(
                @"{""id"":7,""name"":""player""}");

            Assert.AreEqual(7L, payload.id);
            Assert.AreEqual("player", payload.name);
        }

        /// <summary>
        /// NullValueHandling 은 양방향이라 직렬화 쪽에서는 null 멤버가 아예 빠진다.
        /// 요청 바디에서 "필드 없음" 과 "명시적 null" 을 구분하는 서버가 생기면 이 테스트가 먼저 깨져야 한다.
        /// </summary>
        [Test]
        public void SerializationOmitsNullMembers()
        {
            JObject json = JObject.Parse(JsonCodec.Serialize(new NumericPayload { name = null, value = 1f }));

            Assert.IsFalse(json.ContainsKey("name"));
            Assert.AreEqual(1f, json["value"].Value<float>());
        }
    }
}
