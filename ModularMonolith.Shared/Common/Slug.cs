using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ModularMonolith.Shared.Common
{
    public static partial class Slug
    {
        [GeneratedRegex(@"[^a-z0-9]+")]
        private static partial Regex NonAlphaNumericRegex();

        public static string Generate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            name = name.ToLowerInvariant().Trim();
            name = name.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder(name.Length);

            foreach (char c in name)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            name = sb.ToString().Normalize(NormalizationForm.FormC);

            name = NonAlphaNumericRegex().Replace(name, "-");

            return name.Trim('-');
        }
    }
}
