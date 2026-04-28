using System.Text;

namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ImportHistoryStoreTests
{
    [TestMethod]
    public void LoadAll_ReturnsEmptyCollectionWhenHistoryFileDoesNotExist()
    {
        // Azt teszteli, hogy hianyzo elozmenyfajlra ures lista erkezik vissza.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string historyPath = Path.Combine(tempDirectory, "import-history.json");
            ImportHistoryStore store = new(historyPath);

            IReadOnlyList<ImportHistoryEntry> entries = store.LoadAll();

            Assert.IsEmpty(entries);
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void Append_PersistsEntryThatCanBeLoadedBack()
    {
        // Azt teszteli, hogy a lementett importbejegyzes visszaolvashato marad.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string historyPath = Path.Combine(tempDirectory, "import-history.json");
            ImportHistoryStore store = new(historyPath);
            DateTimeOffset importedAt = new(2026, 4, 24, 8, 30, 0, TimeSpan.Zero);

            store.Append(new ImportHistoryEntry
            {
                ImportedAt = importedAt,
                ImportTypeLabel = "Termek import",
                SourceFilePath = @"C:\forras\termekek.xlsx",
                CreatedCount = 4,
                UpdatedCount = 2,
                StatusMessage = "Sikeres import"
            });

            IReadOnlyList<ImportHistoryEntry> entries = store.LoadAll();

            Assert.HasCount(1, entries);
            Assert.AreEqual(importedAt, entries[0].ImportedAt);
            Assert.AreEqual("Termek import", entries[0].ImportTypeLabel);
            Assert.AreEqual("termekek.xlsx", entries[0].SourceFileName);
            Assert.AreEqual(4, entries[0].CreatedCount);
            Assert.AreEqual(2, entries[0].UpdatedCount);
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void Append_CreatesTheTargetDirectoryAutomatically()
    {
        // Azt teszteli, hogy menteskor a hianyzo celmappak automatikusan letrejonnek.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string historyPath = Path.Combine(tempDirectory, "nested", "history", "import-history.json");
            ImportHistoryStore store = new(historyPath);

            store.Append(new ImportHistoryEntry
            {
                ImportTypeLabel = "Kepek import"
            });

            Assert.IsTrue(File.Exists(historyPath));
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void LoadAll_ThrowsInvalidOperationExceptionWhenJsonIsInvalid()
    {
        // Azt teszteli, hogy serult JSON eseten ertelmes InvalidOperationException keletkezik.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string historyPath = Path.Combine(tempDirectory, "import-history.json");
            File.WriteAllText(historyPath, "{ not valid json", Encoding.UTF8);
            ImportHistoryStore store = new(historyPath);

            InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
                () => store.LoadAll());

            StringAssert.Contains(exception.Message, "nem olvashato be");
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"WinFormsApp1.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
