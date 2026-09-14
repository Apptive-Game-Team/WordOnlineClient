using System;
using System.Collections.Generic;
using Global;

namespace Data.Magic
{
    /// <summary>
    /// <c>GET /api/data/magics</c> 항목의 <c>indicator</c> jsonb document.
    /// 조준 표시를 layer 목록으로 적는다. <c>layers</c> 는 순서가 있고, 뒤 layer 가 앞 layer 위에 그려진다.
    /// </summary>
    [Serializable]
    public class MagicIndicatorDocument
    {
        /// <summary>지금 읽을 줄 아는 document version. 다른 값은 통째로 건너뛴다.</summary>
        public const int SupportedVersion = 1;

        public int version;
        public List<MagicIndicatorLayer> layers;

        // Prepare 가 한 번 채운다. private 이라 Json.NET 이 직렬화하지 않는다.
        private bool prepared;
        private bool usable;

        /// <summary>
        /// layer 문자열을 enum 으로 해석하고, 이 document 로 그릴 것이 하나라도 있는지 알려준다.
        /// 매 프레임 불리지만 실제 작업은 document 당 한 번뿐이고, 읽을 수 없는 부분의 경고도 그때 한 번만 남는다.
        /// </summary>
        /// <param name="magicName">경고 log 에 적을 마법 이름.</param>
        public bool Prepare(string magicName)
        {
            if (prepared)
            {
                return usable;
            }

            prepared = true;
            usable = Interpret(magicName);
            return usable;
        }

        private bool Interpret(string magicName)
        {
            if (version != SupportedVersion)
            {
                WDebug.LogWarning(
                    $"[MagicIndicator] {magicName}: unsupported indicator version {version}, expected {SupportedVersion}.");
                return false;
            }

            if (layers == null || layers.Count == 0)
            {
                WDebug.LogWarning($"[MagicIndicator] {magicName}: indicator document has no layers.");
                return false;
            }

            int readableCount = 0;
            for (int i = 0; i < layers.Count; i++)
            {
                MagicIndicatorLayer layer = layers[i];
                if (layer == null)
                {
                    WDebug.LogWarning($"[MagicIndicator] {magicName}: layer {i} is null, skipping it.");
                    continue;
                }

                if (layer.Prepare(out string problem))
                {
                    readableCount++;
                    continue;
                }

                WDebug.LogWarning($"[MagicIndicator] {magicName}: layer {i} skipped, {problem}.");
            }

            if (readableCount == 0)
            {
                WDebug.LogWarning($"[MagicIndicator] {magicName}: no readable layer out of {layers.Count}.");
                return false;
            }

            return true;
        }
    }
}
