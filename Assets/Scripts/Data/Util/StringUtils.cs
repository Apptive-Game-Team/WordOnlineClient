using System.Globalization;
using System.Text;

namespace Scripts.Data.Util
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
    }

}