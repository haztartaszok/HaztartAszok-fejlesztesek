using System.Globalization;
using System.Text;

namespace WinFormsApp1
{
    internal static class ImportUtilities
    {
        public static bool TryResolveImageFilePath(
            string workbookFilePath,
            string imagePath,
            string imageName,
            out string? resolvedPath)
        {
            string workbookDirectory = Path.GetDirectoryName(workbookFilePath) ?? AppContext.BaseDirectory;
            List<string> candidates = [];

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                candidates.Add(imagePath);

                if (!Path.IsPathRooted(imagePath))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imagePath));
                }
            }

            if (!string.IsNullOrWhiteSpace(imageName))
            {
                candidates.Add(imageName);

                if (!Path.IsPathRooted(imageName))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imageName));
                }
            }

            if (!string.IsNullOrWhiteSpace(imagePath) && !string.IsNullOrWhiteSpace(imageName))
            {
                candidates.Add(Path.Combine(imagePath, imageName));

                if (!Path.IsPathRooted(imagePath))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imagePath, imageName));
                }
            }

            foreach (string candidate in candidates.Where(candidate => !string.IsNullOrWhiteSpace(candidate)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string normalizedCandidate = Path.GetFullPath(candidate);

                if (File.Exists(normalizedCandidate))
                {
                    resolvedPath = normalizedCandidate;
                    return true;
                }

                if (Directory.Exists(normalizedCandidate) && !string.IsNullOrWhiteSpace(imageName))
                {
                    string combined = Path.Combine(normalizedCandidate, imageName);

                    if (File.Exists(combined))
                    {
                        resolvedPath = Path.GetFullPath(combined);
                        return true;
                    }
                }
            }

            resolvedPath = null;
            return false;
        }

        public static string BuildImageReference(string imagePath, string imageName)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && !string.IsNullOrWhiteSpace(imageName))
            {
                return $"{imagePath}\\{imageName}";
            }

            return !string.IsNullOrWhiteSpace(imagePath) ? imagePath : imageName;
        }

        public static bool TryParseImportDecimal(string rawValue, out decimal value)
        {
            if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
        }

        public static bool TryParseImportInt(string rawValue, out int value)
        {
            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
            {
                return true;
            }

            if (TryParseImportDecimal(rawValue, out decimal decimalValue) &&
                decimal.Truncate(decimalValue) == decimalValue &&
                decimalValue >= int.MinValue &&
                decimalValue <= int.MaxValue)
            {
                value = (int)decimalValue;
                return true;
            }

            value = 0;
            return false;
        }

        public static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string decomposed = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new();

            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString();
        }
    }
}
