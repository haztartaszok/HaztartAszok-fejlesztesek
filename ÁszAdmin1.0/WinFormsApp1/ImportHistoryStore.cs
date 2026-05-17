using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    internal sealed class ImportHistoryStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private readonly string historyFilePath;

        public ImportHistoryStore()
            : this(GetDefaultHistoryFilePath())
        {
        }

        public ImportHistoryStore(string historyFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(historyFilePath);
            this.historyFilePath = historyFilePath;
        }

        public IReadOnlyList<ImportHistoryEntry> LoadAll()
        {
            return LoadEntries();
        }

        public void Append(ImportHistoryEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            List<ImportHistoryEntry> entries = LoadEntries();
            entries.Add(entry);
            SaveEntries(entries);
        }

        private List<ImportHistoryEntry> LoadEntries()
        {
            if (!File.Exists(historyFilePath))
            {
                return [];
            }

            try
            {
                string json = File.ReadAllText(historyFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }

                return JsonSerializer.Deserialize<List<ImportHistoryEntry>>(json, JsonOptions) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Az importelozmenyek fajlja nem olvashato be.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Az importelozmenyek fajlja nem erheto el.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException("Az importelozmenyek fajlja nem olvashato a jelenlegi jogosultsagokkal.", ex);
            }
        }

        private void SaveEntries(IReadOnlyList<ImportHistoryEntry> entries)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(historyFilePath) ?? AppContext.BaseDirectory;
                Directory.CreateDirectory(directoryPath);
                string json = JsonSerializer.Serialize(entries, JsonOptions);
                File.WriteAllText(historyFilePath, json);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Az importelozmenyek fajlja nem mentheto.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException("Az importelozmenyek fajlja nem mentheto a jelenlegi jogosultsagokkal.", ex);
            }
        }

        private static string GetDefaultHistoryFilePath()
        {
            string applicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AszAdmin2.0");

            string historyFilePath = Path.Combine(applicationDataPath, "import-history.json");

            if (File.Exists(historyFilePath))
            {
                return historyFilePath;
            }

            string legacyApplicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AszAdmin1.0");
            string legacyHistoryFilePath = Path.Combine(legacyApplicationDataPath, "import-history.json");

            if (File.Exists(legacyHistoryFilePath))
            {
                Directory.CreateDirectory(applicationDataPath);
                File.Copy(legacyHistoryFilePath, historyFilePath, overwrite: false);
            }

            return historyFilePath;
        }
    }

    internal sealed class ImportHistoryEntry
    {
        public DateTimeOffset ImportedAt { get; init; }

        public string ImportTypeLabel { get; init; } = string.Empty;

        public string SourceFilePath { get; init; } = string.Empty;

        public int CreatedCount { get; init; }

        public int UpdatedCount { get; init; }

        public int SkippedExistingCount { get; init; }

        public int ProductTypeAppliedCount { get; init; }

        public int CategoryLinkedCount { get; init; }

        public int CategoryAlreadyLinkedCount { get; init; }

        public int ImageUploadedCount { get; init; }

        public int MainImageUploadedCount { get; init; }

        public int AdditionalImageUploadedCount { get; init; }

        public int PropertyAppliedCount { get; init; }

        public int ErrorCount { get; init; }

        public string StatusMessage { get; init; } = string.Empty;

        public string DetailsMessage { get; init; } = string.Empty;

        [JsonIgnore]
        public string ImportedAtDisplay => ImportedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

        [JsonIgnore]
        public string SourceFileName => string.IsNullOrWhiteSpace(SourceFilePath)
            ? string.Empty
            : Path.GetFileName(SourceFilePath);
    }
}
