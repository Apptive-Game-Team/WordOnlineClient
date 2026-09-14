using System;
using System.Collections.Generic;
using Data.Util;

namespace Data.Magic
{
    /// <summary>
    /// 서버가 준 마법 목록을 클라이언트가 쓰는 <see cref="CombinedMagicData"/> 로 바꿔서 돌려준다.
    /// 목록이 아직 도착하지 않았으면 빈 목록을 돌려준다. 내장 fallback 표는 없다.
    /// </summary>
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> GetEffectiveDataList()
        {
            var versionedMagics = MagicInfoDataSource.GetCachedMagics();
            if (versionedMagics != null && versionedMagics.Count > 0)
            {
                return BuildCombinedMagicData(versionedMagics);
            }

            return emptyDataList;
        }

        /// <summary>이름으로 마법 카드를 찾는다. 못 찾으면 null.</summary>
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            TryGetByName(name, out CombinedMagicData match);
            return match;
        }

        /// <summary>
        /// 이름으로 마법 카드를 찾는다. 같은 마법이 서버 이름(<c>evil_ent</c>), 번역 키(<c>evilEnt</c>),
        /// 리소스 이름(<c>EvilEnt</c>) 세 철자로 돌아다니므로 셋 다 받는다.
        /// </summary>
        public static bool TryGetByName(string name, out CombinedMagicData match)
        {
            match = null;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            foreach (CombinedMagicData data in GetEffectiveDataList())
            {
                if (IsSameName(data.serverName, name) ||
                    IsSameName(data.localizationKey, name) ||
                    IsSameName(data.resourceName, name))
                {
                    match = data;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetById(long id, out CombinedMagicData match)
        {
            match = null;
            foreach (CombinedMagicData data in GetEffectiveDataList())
            {
                if (data.id == id)
                {
                    match = data;
                    return true;
                }
            }

            return false;
        }

        /// <summary>서버가 보낸 원소 문자열을 enum 으로 바꾼다. 모르는 값은 None 이다.</summary>
        public static ElementType ParseElement(string element)
        {
            if (string.IsNullOrWhiteSpace(element))
            {
                return ElementType.None;
            }

            return Enum.TryParse(element, true, out ElementType parsed) ? parsed : ElementType.None;
        }

        private static List<CombinedMagicData> BuildCombinedMagicData(IReadOnlyList<MagicInfoDto> source)
        {
            var result = new List<CombinedMagicData>(source.Count);
            foreach (MagicInfoDto magic in source)
            {
                if (magic == null || string.IsNullOrWhiteSpace(magic.name))
                {
                    continue;
                }

                result.Add(new CombinedMagicData
                {
                    id = magic.id,
                    serverName = magic.name,
                    localizationKey = StringUtils.ToCamelCase(magic.name),
                    textLocalizationKey = string.IsNullOrWhiteSpace(magic.text)
                        ? StringUtils.ToSnakeCase(magic.name)
                        : magic.text,
                    resourceName = StringUtils.ToPascalCase(magic.name),
                    element = ParseElement(magic.element),
                    manaCost = magic.manaCost,
                    aimShape = magic.aimShape,
                });
            }

            return result;
        }

        private static bool IsSameName(string left, string right)
        {
            return !string.IsNullOrEmpty(left) &&
                   string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly List<CombinedMagicData> emptyDataList = new();
    }
}
