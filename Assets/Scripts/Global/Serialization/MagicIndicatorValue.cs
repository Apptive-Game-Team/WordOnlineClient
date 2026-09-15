using System.Globalization;

namespace Global.Serialization
{
    /// <summary>
    /// indicator document 의 숫자 자리 하나. JSON 에서는 다섯 가지 모양으로 온다.
    /// <list type="bullet">
    /// <item><description><c>2.5</c> — 그대로 쓰는 숫자.</description></item>
    /// <item><description><c>{"parameter":"radius"}</c> — parameter 표에서, 마법 자신의 game object 에서 읽는 값.</description></item>
    /// <item><description><c>{"parameter":"attack_offset","fallback":0}</c> — 못 읽으면 fallback.</description></item>
    /// <item><description><c>{"object":"electric_shot","parameter":"radius"}</c> — 마법 자신이 아닌 다른 game object 에서 읽는 값.</description></item>
    /// <item><description><c>{"object":"dragon_flame","parameter":"radius","fallback":0.5}</c> — object 를 못 찾아도 못 읽어도 fallback.</description></item>
    /// </list>
    /// <para>
    /// 필드가 아예 없을 때와 <c>0</c> 이 왔을 때를 구분해야 하므로 <see cref="IsPresent"/> 를 둔다.
    /// struct 의 기본값이 곧 "필드 없음" 이라, Json.NET 이 건드리지 않은 자리는 저절로 그 상태가 된다.
    /// </para>
    /// </summary>
    public readonly struct MagicIndicatorValue
    {
        private readonly float number;
        private readonly string objectName;
        private readonly string parameterName;
        private readonly float fallbackNumber;
        private readonly bool hasFallback;
        private readonly bool present;

        private MagicIndicatorValue(float number, string objectName, string parameterName, float fallbackNumber,
            bool hasFallback)
        {
            this.number = number;
            this.objectName = objectName;
            this.parameterName = parameterName;
            this.fallbackNumber = fallbackNumber;
            this.hasFallback = hasFallback;
            present = true;
        }

        /// <summary>JSON 에 이 자리가 있었는지. 없었으면 false 다.</summary>
        public bool IsPresent => present;

        /// <summary>parameter 이름으로 읽어야 하는 값인지. false 면 <see cref="Number"/> 를 그대로 쓴다.</summary>
        public bool IsParameter => parameterName != null;

        /// <summary>
        /// 마법 자신의 game object 가 아닌 다른 game object 에서 읽어야 하는지.
        /// false 면 지금과 같이 그 마법 자신의 game object 에서 읽는다.
        /// </summary>
        public bool HasObject => objectName != null;

        public string ObjectName => objectName;

        public string ParameterName => parameterName;

        public float Number => number;

        public bool HasFallback => hasFallback;

        public float FallbackNumber => fallbackNumber;

        public static MagicIndicatorValue FromNumber(float value)
        {
            return new MagicIndicatorValue(value, null, null, 0f, false);
        }

        public static MagicIndicatorValue FromParameter(string parameterName)
        {
            return new MagicIndicatorValue(0f, null, parameterName, 0f, false);
        }

        public static MagicIndicatorValue FromParameter(string parameterName, float fallback)
        {
            return new MagicIndicatorValue(0f, null, parameterName, fallback, true);
        }

        /// <summary>마법 자신이 아닌 <paramref name="objectName"/> game object 에서 읽는다.</summary>
        public static MagicIndicatorValue FromParameter(string objectName, string parameterName)
        {
            return new MagicIndicatorValue(0f, objectName, parameterName, 0f, false);
        }

        /// <summary>마법 자신이 아닌 <paramref name="objectName"/> game object 에서 읽고, 못 읽으면 fallback.</summary>
        public static MagicIndicatorValue FromParameter(string objectName, string parameterName, float fallback)
        {
            return new MagicIndicatorValue(0f, objectName, parameterName, fallback, true);
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

            var name = HasObject ? $"{objectName}.{parameterName}" : parameterName;
            return hasFallback
                ? $"{name} (fallback {fallbackNumber.ToString(CultureInfo.InvariantCulture)})"
                : name;
        }
    }
}
