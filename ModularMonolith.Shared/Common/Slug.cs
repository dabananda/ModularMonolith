using System.Text;
using System.Text.RegularExpressions;

namespace ModularMonolith.Shared.Common
{
    public static class Slug
    {
        public static string Generate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            name = name.ToLowerInvariant().Trim();

            name = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in name)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            name = sb.ToString().Normalize(NormalizationForm.FormC);

            name = Regex.Replace(name, @"[^a-z0-9]+", "-");

            return name.Trim('-');
        }
    }
}
