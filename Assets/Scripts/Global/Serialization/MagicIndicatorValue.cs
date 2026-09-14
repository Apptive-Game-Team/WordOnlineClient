using System.Globalization;

namespace Global.Serialization
{
    /// <summary>
    /// indicator document 의 숫자 자리 하나. JSON 에서는 세 가지 모양으로 온다.
    /// <list type="bullet">
    /// <item><description><c>2.5</c> — 그대로 쓰는 숫자.</description></item>
    /// <item><description><c>{"parameter":"radius"}</c> — parameter 표에서 읽는 값.</description></item>
    /// <item><description><c>{"parameter":"attack_offset","fallback":0}</c> — 못 읽으면 fallback.</description></item>
    /// </list>
    /// <para>
    /// 필드가 아예 없을 때와 <c>0</c> 이 왔을 때를 구분해야 하므로 <see cref="IsPresent"/> 를 둔다.
    /// struct 의 기본값이 곧 "필드 없음" 이라, Json.NET 이 건드리지 않은 자리는 저절로 그 상태가 된다.
    /// </para>
    /// </summary>
    public readonly struct MagicIndicatorValue
    {
        private readonly float number;
        private readonly string parameterName;
        private readonly float fallbackNumber;
        private readonly bool hasFallback;
        private readonly bool present;

        private MagicIndicatorValue(float number, string parameterName, float fallbackNumber, bool hasFallback)
        {
            this.number = number;
            this.parameterName = parameterName;
            this.fallbackNumber = fallbackNumber;
            this.hasFallback = hasFallback;
            present = true;
        }

        /// <summary>JSON 에 이 자리가 있었는지. 없었으면 false 다.</summary>
        public bool IsPresent => present;

        /// <summary>parameter 이름으로 읽어야 하는 값인지. false 면 <see cref="Number"/> 를 그대로 쓴다.</summary>
        public bool IsParameter => parameterName != null;

        public string ParameterName => parameterName;

        public float Number => number;

        public bool HasFallback => hasFallback;

        public float FallbackNumber => fallbackNumber;

        public static MagicIndicatorValue FromNumber(float value)
        {
            return new MagicIndicatorValue(value, null, 0f, false);
        }

        public static MagicIndicatorValue FromParameter(string parameterName)
        {
            return new MagicIndicatorValue(0f, parameterName, 0f, false);
        }

        public static MagicIndicatorValue FromParameter(string parameterName, float fallback)
        {
            return new MagicIndicatorValue(0f, parameterName, fallback, true);
        }

        public override string ToString()
        {
            if (!present)
            {
                return "<absent>";
            }

            if (!IsParameter)
            {
                return number.ToString(CultureInfo.InvariantCulture);
            }

            return hasFallback
                ? $"{parameterName} (fallback {fallbackNumber.ToString(CultureInfo.InvariantCulture)})"
                : parameterName;
        }
    }
}
