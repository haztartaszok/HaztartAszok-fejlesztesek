namespace WinFormsApp1
{
    internal static class ImportWorksheetBuilder
    {
        public static WorksheetTable BuildWorksheetTable(WorksheetPreview worksheet)
        {
            ArgumentNullException.ThrowIfNull(worksheet);

            if (worksheet.Rows.Count == 0)
            {
                throw new InvalidOperationException($"A '{worksheet.Name}' munkalap ures.");
            }

            string[] headers = NormalizeRowLength(worksheet.Rows[0], worksheet.Rows[0].Length);

            if (!headers.Any(header => !string.IsNullOrWhiteSpace(header)))
            {
                throw new InvalidOperationException($"A '{worksheet.Name}' munkalapon nem talalhato fejlec sor.");
            }

            Dictionary<string, int> headerIndexes = new(StringComparer.Ordinal);

            for (int i = 0; i < headers.Length; i++)
            {
                string normalizedHeader = ImportUtilities.NormalizeToken(headers[i]);

                if (!string.IsNullOrWhiteSpace(normalizedHeader) && !headerIndexes.ContainsKey(normalizedHeader))
                {
                    headerIndexes[normalizedHeader] = i;
                }
            }

            List<(int RowNumber, string[] Values)> rows = [];

            for (int rowIndex = 1; rowIndex < worksheet.Rows.Count; rowIndex++)
            {
                string[] rowValues = NormalizeRowLength(worksheet.Rows[rowIndex], headers.Length);

                if (rowValues.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                rows.Add((rowIndex + 1, rowValues));
            }

            return new WorksheetTable(worksheet.Name, headerIndexes, rows);
        }

        private static string[] NormalizeRowLength(string[] sourceRow, int columnCount)
        {
            string[] normalizedRow = new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                normalizedRow[i] = i < sourceRow.Length ? sourceRow[i] : string.Empty;
            }

            return normalizedRow;
        }
    }
}
