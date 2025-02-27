using System.Text.RegularExpressions;


namespace CopyLine
{
    class ExtractString
    {
        // trim head and end space (' ') and tab (\t)
        static char[] charsToTrim = {' ', '\t'};

        // ignore tag
        static string pattern1 = @"^(#+|\(begin\)|\(end\))";

        // remove leading bullet point symbol
        static string pattern2 = @"^(\(?[A-Za-z0-9]+[\)\.][ \t]+|[-\+\*][ \t]+)?(?<content>.+)";

        // remove last symbols
        static string pattern3 = "(#{2,}|[○×]).*";

        public static string GetExtractString(string str)
        {
            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (Regex.IsMatch(str, pattern1)) {
                return string.Empty;
            }

            string res1 = str.Trim(charsToTrim);
            string res2 = Regex.Match(res1, pattern2).Groups["content"].Value;
            string res3 = Regex.Replace(res2, pattern3, string.Empty);
            string res = res3.TrimEnd(charsToTrim);
            return res;
        }
    }
}
