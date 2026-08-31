using System.Globalization;
using System.Text;

namespace Data.Util
{
    public static class StringUtils
    {
        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 공백, _, - 기준으로 단어 분리
            var words = input.Split(new char[] { ' ', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (i == 0)
                {
                    // 첫 단어는 소문자
                    sb.Append(word.ToLower(CultureInfo.InvariantCulture));
                }
                else
                {
                    // 나머지 단어는 PascalCase
                    sb.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLower(CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }

        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Normalize camelCase and PascalCase boundaries before rebuilding the name.
            // Without this step, an input such as "StormStag" became "Stormstag",
            // which does not match case-sensitive Resources paths in WebGL builds.
            string normalizedInput = ToSnakeCase(input);
            var words = normalizedInput.Split(
                new char[] { '_' },
                System.StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                sb.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
                if (word.Length > 1)
                    sb.Append(word.Substring(1));
            }

            return sb.ToString();
        }

        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            var previousWasSeparator = false;

            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];
                if (current == ' ' || current == '_' || current == '-')
                {
                    if (sb.Length > 0 && !previousWasSeparator)
                    {
                        sb.Append('_');
                        previousWasSeparator = true;
                    }

                    continue;
                }

                if (char.IsUpper(current) && sb.Length > 0 && !previousWasSeparator)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLower(current, CultureInfo.InvariantCulture));
                previousWasSeparator = false;
            }

            return sb.ToString();
        }
    }

}
