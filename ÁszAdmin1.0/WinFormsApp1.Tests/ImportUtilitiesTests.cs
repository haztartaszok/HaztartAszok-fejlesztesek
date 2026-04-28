namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ImportUtilitiesTests
{
    [TestMethod]
    public void NormalizeToken_RemovesAccentsWhitespaceAndSymbols()
    {
        // Azt teszteli, hogy az oszlop es ertekazonositasnal az ekezetek es elvalaszto jelek nem szamitanak.
        string result = ImportUtilities.NormalizeToken("Ar / keszlet-or 123");

        Assert.AreEqual("ARKESZLETOR123", result);
    }

    [TestMethod]
    public void TryParseImportDecimal_ParsesInvariantNumbers()
    {
        // Azt teszteli, hogy a ponttal irt decimalis ertekek beolvashatok import kozben.
        bool success = ImportUtilities.TryParseImportDecimal("1234.50", out decimal value);

        Assert.IsTrue(success);
        Assert.AreEqual(1234.50m, value);
    }

    [TestMethod]
    public void TryParseImportInt_AcceptsWholeDecimalValues()
    {
        // Azt teszteli, hogy egesz erteku decimalisbol is kepes darabszamot olvasni.
        bool success = ImportUtilities.TryParseImportInt("42.0", out int value);

        Assert.IsTrue(success);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryResolveImageFilePath_FindsRelativeImageNextToWorkbook()
    {
        // Azt teszteli, hogy a workbook melle tett relativ kepfajl eleresi utvonala feloldhato.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string workbookPath = Path.Combine(tempDirectory, "import.xlsx");
            string imagesDirectory = Path.Combine(tempDirectory, "kepek");
            string imagePath = Path.Combine(imagesDirectory, "termek.jpg");

            Directory.CreateDirectory(imagesDirectory);
            File.WriteAllText(workbookPath, "teszt");
            File.WriteAllText(imagePath, "kep");

            bool success = ImportUtilities.TryResolveImageFilePath(
                workbookPath,
                "kepek",
                "termek.jpg",
                out string? resolvedPath);

            Assert.IsTrue(success);
            Assert.AreEqual(Path.GetFullPath(imagePath), resolvedPath);
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void BuildImageReference_CombinesPathAndFileName()
    {
        // Azt teszteli, hogy naplozashoz a mappa es a fajlnev egy kozos hivatkozasba all ossze.
        string result = ImportUtilities.BuildImageReference("kepek", "termek.jpg");

        Assert.AreEqual(@"kepek\termek.jpg", result);
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
