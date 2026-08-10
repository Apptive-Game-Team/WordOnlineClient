using Global.Serialization;
using NUnit.Framework;

namespace WordOnline.Tests
{
    /// <summary>
    /// 서버 응답 경로가 예외 대신 false 를 받는지 고정한다. 코루틴 안에서 예외가 나면 코루틴이 죽고
    /// 완료 콜백이 호출되지 않아 호출자가 영원히 기다리게 된다.
    /// </summary>
    public class JsonCodecTryDeserializeTests
    {
        private class Payload
        {
            public string name;
            public float value;
        }

        /// <summary>인자 있는 생성자만 둘이면 Json.NET 이 어느 쪽도 고르지 못한다.</summary>
        private class UnconstructablePayload
        {
            public long id;

            public UnconstructablePayload(long id)
            {
                this.id = id;
            }

            public UnconstructablePayload(long id, string _)
            {
                this.id = id;
            }
        }

        [Test]
        public void ReturnsFalseInsteadOfThrowingOnMalformedJson()
        {
            Assert.IsFalse(JsonCodec.TryDeserialize(@"{""name"": ", out Payload value, out string error));
            Assert.IsNull(value);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void ReturnsFalseWhenNoConstructorCanBeUsed()
        {
            Assert.IsFalse(JsonCodec.TryDeserialize(@"{""id"":1}", out UnconstructablePayload value, out string error));
            Assert.IsNull(value);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void ReturnsFalseOnEmptyBody()
        {
            Assert.IsFalse(JsonCodec.TryDeserialize("   ", out Payload value, out string error));
            Assert.IsNull(value);
            Assert.AreEqual("empty response body", error);
        }

        [Test]
        public void ReturnsFalseWhenBodyIsJsonNull()
        {
            Assert.IsFalse(JsonCodec.TryDeserialize("null", out Payload value, out string error));
            Assert.IsNull(value);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void SucceedsWithNoErrorOnValidPayload()
        {
            Assert.IsTrue(JsonCodec.TryDeserialize(@"{""name"":""a"",""value"":1.5}", out Payload value, out string error));
            Assert.AreEqual("a", value.name);
            Assert.AreEqual(1.5f, value.value);
            Assert.IsNull(error);
        }

        [Test]
        public void ExcerptTruncatesLongBodies()
        {
            string body = new string('x', 500);

            string excerpt = JsonCodec.Excerpt(body);

            Assert.Less(excerpt.Length, body.Length);
            Assert.IsTrue(excerpt.StartsWith("xxx"));
        }

        [Test]
        public void ExcerptKeepsShortBodiesIntact()
        {
            Assert.AreEqual(@"{""a"":1}", JsonCodec.Excerpt(@"{""a"":1}"));
            Assert.AreEqual("<empty>", JsonCodec.Excerpt(null));
        }
    }
}
