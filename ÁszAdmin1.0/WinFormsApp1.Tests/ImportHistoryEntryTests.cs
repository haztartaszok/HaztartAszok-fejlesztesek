namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ImportHistoryEntryTests
{
    [TestMethod]
    public void ImportedAtDisplay_UsesLocalTimeFormatting()
    {
        // Azt teszteli, hogy a megjelenitett datum helyi idoben es elvart formatumban jelenik meg.
        ImportHistoryEntry entry = new()
        {
            ImportedAt = new DateTimeOffset(2026, 4, 24, 8, 30, 0, TimeSpan.Zero)
        };

        string display = entry.ImportedAtDisplay;

        Assert.AreEqual(
            entry.ImportedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            display);
    }

    [TestMethod]
    public void SourceFileName_ReturnsOnlyTheFileName()
    {
        // Azt teszteli, hogy az eleresi utbol csak a fajlnev marad meg.
        ImportHistoryEntry entry = new()
        {
            SourceFilePath = @"C:\import\mappak\termekek.xlsx"
        };

        Assert.AreEqual("termekek.xlsx", entry.SourceFileName);
    }

    [TestMethod]
    public void SourceFileName_ReturnsEmptyStringWhenPathIsBlank()
    {
        // Azt teszteli, hogy ures vagy whitespace utvonal eseten ures sztring jon vissza.
        ImportHistoryEntry entry = new()
        {
            SourceFilePath = " "
        };

        Assert.AreEqual(string.Empty, entry.SourceFileName);
    }
}
