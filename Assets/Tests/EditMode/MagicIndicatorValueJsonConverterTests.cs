using Global.Serialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace WordOnline.Tests
{
    /// <summary>
    /// indicator document 의 값 한 칸은 JSON number 로도, <c>{"parameter":...}</c> object 로도 온다.
    /// 이 자리가 PlayerPrefs cache 로 나갔다 다시 들어오므로 왕복도 함께 본다.
    /// </summary>
    public class MagicIndicatorValueJsonConverterTests
    {
        private class Holder
        {
            public MagicIndicatorValue value;
            public float after;
        }

        [Test]
        public void ReadsPlainNumber()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{""value"":0.08}").value;

            Assert.IsTrue(value.IsPresent);
            Assert.IsFalse(value.IsParameter);
            Assert.AreEqual(0.08f, value.Number, 1e-6f);
        }

        [Test]
        public void ReadsInteger()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{""value"":3}").value;

            Assert.IsTrue(value.IsPresent);
            Assert.IsFalse(value.IsParameter);
            Assert.AreEqual(3f, value.Number, 1e-6f);
        }

        [Test]
        public void ReadsParameterWithoutFallback()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{""value"":{""parameter"":""radius""}}").value;

            Assert.IsTrue(value.IsPresent);
            Assert.IsTrue(value.IsParameter);
            Assert.AreEqual("radius", value.ParameterName);
            Assert.IsFalse(value.HasFallback);
        }

        [Test]
        public void ReadsParameterWithFallback()
        {
            MagicIndicatorValue value = JsonCodec
                .Deserialize<Holder>(@"{""value"":{""parameter"":""attack_offset"",""fallback"":0}}").value;

            Assert.IsTrue(value.IsParameter);
            Assert.AreEqual("attack_offset", value.ParameterName);
            Assert.IsTrue(value.HasFallback);
            Assert.AreEqual(0f, value.FallbackNumber, 1e-6f);
        }

        [Test]
        public void ReadsParameterWithObject()
        {
            MagicIndicatorValue value = JsonCodec
                .Deserialize<Holder>(@"{""value"":{""object"":""electric_shot"",""parameter"":""radius""}}").value;

            Assert.IsTrue(value.IsPresent);
            Assert.IsTrue(value.IsParameter);
            Assert.IsTrue(value.HasObject);
            Assert.AreEqual("electric_shot", value.ObjectName);
            Assert.AreEqual("radius", value.ParameterName);
            Assert.IsFalse(value.HasFallback);
        }

        [Test]
        public void ReadsParameterWithObjectAndFallback()
        {
            MagicIndicatorValue value = JsonCodec
                .Deserialize<Holder>(
                    @"{""value"":{""object"":""dragon_flame"",""parameter"":""radius"",""fallback"":0.5}}").value;

            Assert.IsTrue(value.HasObject);
            Assert.AreEqual("dragon_flame", value.ObjectName);
            Assert.AreEqual("radius", value.ParameterName);
            Assert.IsTrue(value.HasFallback);
            Assert.AreEqual(0.5f, value.FallbackNumber, 1e-6f);
        }

        /// <summary>object 가 계약대로 문자열이 아니면 parameter 가 문자열이 아닐 때와 같이 "없음" 이 된다.</summary>
        [Test]
        public void ObjectNotStringIsNotPresent()
        {
            MagicIndicatorValue value = JsonCodec
                .Deserialize<Holder>(@"{""value"":{""object"":3,""parameter"":""radius""}}").value;

            Assert.IsFalse(value.IsPresent);
        }

        /// <summary>필드가 없는 것과 0 이 온 것은 달라야 한다. forwardOffset 의 기본값이 여기에 걸린다.</summary>
        [Test]
        public void OmittedValueIsNotPresent()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{}").value;

            Assert.IsFalse(value.IsPresent);
        }

        [Test]
        public void ZeroIsPresent()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{""value"":0}").value;

            Assert.IsTrue(value.IsPresent);
            Assert.AreEqual(0f, value.Number, 1e-6f);
        }

        [Test]
        public void NullValueIsNotPresent()
        {
            MagicIndicatorValue value = JsonCodec.Deserialize<Holder>(@"{""value"":null}").value;

            Assert.IsFalse(value.IsPresent);
        }

        /// <summary>계약에 없는 모양은 throw 하지 않고 "없음" 으로 떨어져야 한다.</summary>
        [Test]
        public void UnreadableShapeIsNotPresent()
        {
            Assert.IsFalse(JsonCodec.Deserialize<Holder>(@"{""value"":[1,2]}").value.IsPresent);
            Assert.IsFalse(JsonCodec.Deserialize<Holder>(@"{""value"":""radius""}").value.IsPresent);
            Assert.IsFalse(JsonCodec.Deserialize<Holder>(@"{""value"":{""fallback"":2}}").value.IsPresent);
        }

        /// <summary>배열을 만나고도 뒤에 오는 필드를 놓치지 않아야 한다.</summary>
        [Test]
        public void UnreadableShapeDoesNotSwallowLaterFields()
        {
            Holder holder = JsonCodec.Deserialize<Holder>(@"{""value"":[1,2],""after"":7}");

            Assert.IsFalse(holder.value.IsPresent);
            Assert.AreEqual(7f, holder.after, 1e-6f);
        }

        [Test]
        public void RoundTripsPlainNumber()
        {
            MagicIndicatorValue restored = RoundTrip(MagicIndicatorValue.FromNumber(1.25f));

            Assert.IsTrue(restored.IsPresent);
            Assert.IsFalse(restored.IsParameter);
            Assert.AreEqual(1.25f, restored.Number, 1e-6f);
        }

        [Test]
        public void RoundTripsParameterWithFallback()
        {
            MagicIndicatorValue restored = RoundTrip(MagicIndicatorValue.FromParameter("attack_offset", 0.5f));

            Assert.IsTrue(restored.IsParameter);
            Assert.AreEqual("attack_offset", restored.ParameterName);
            Assert.IsTrue(restored.HasFallback);
            Assert.AreEqual(0.5f, restored.FallbackNumber, 1e-6f);
        }

        [Test]
        public void RoundTripsParameterWithoutFallback()
        {
            MagicIndicatorValue restored = RoundTrip(MagicIndicatorValue.FromParameter("radius"));

            Assert.IsTrue(restored.IsParameter);
            Assert.AreEqual("radius", restored.ParameterName);
            Assert.IsFalse(restored.HasFallback);
        }

        [Test]
        public void RoundTripsParameterWithObject()
        {
            MagicIndicatorValue restored = RoundTrip(MagicIndicatorValue.FromParameter("electric_shot", "radius"));

            Assert.IsTrue(restored.IsParameter);
            Assert.IsTrue(restored.HasObject);
            Assert.AreEqual("electric_shot", restored.ObjectName);
            Assert.AreEqual("radius", restored.ParameterName);
            Assert.IsFalse(restored.HasFallback);
        }

        [Test]
        public void RoundTripsParameterWithObjectAndFallback()
        {
            MagicIndicatorValue restored =
                RoundTrip(MagicIndicatorValue.FromParameter("dragon_flame", "radius", 0.5f));

            Assert.IsTrue(restored.HasObject);
            Assert.AreEqual("dragon_flame", restored.ObjectName);
            Assert.AreEqual("radius", restored.ParameterName);
            Assert.IsTrue(restored.HasFallback);
            Assert.AreEqual(0.5f, restored.FallbackNumber, 1e-6f);
        }

        /// <summary>PlayerPrefs cache 로 나갔다 와도 "없음" 이 0 으로 바뀌면 안 된다.</summary>
        [Test]
        public void RoundTripsAbsentValue()
        {
            Assert.IsFalse(RoundTrip(default).IsPresent);
        }

        /// <summary>없던 값은 null 로 적히고, 다시 읽으면 "없음" 으로 돌아와야 한다.</summary>
        [Test]
        public void AbsentValueSerializesAsNull()
        {
            JObject json = JObject.Parse(JsonCodec.Serialize(new Holder()));

            Assert.AreEqual(JTokenType.Null, json["value"].Type);
        }

        private static MagicIndicatorValue RoundTrip(MagicIndicatorValue value)
        {
            return JsonCodec.Deserialize<Holder>(JsonCodec.Serialize(new Holder { value = value })).value;
        }
    }
}
