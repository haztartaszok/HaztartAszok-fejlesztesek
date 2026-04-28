namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ImageImportTests
{
    [TestMethod]
    public void ParseImageImportRows_ParsesValidRowsAndSkipsCompletelyEmptyRows()
    {
        // Azt teszteli, hogy a kepimport csak a kitoltott sorokat tartja meg, a teljesen ureseket kihagyja.
        WorksheetTable table = CreateWorksheetTable(
            "Kepek",
            ["SKU", "KepUtvonal", "KepNev"],
            ["SKU-001", "kepek", "egy.jpg"],
            ["", "", ""],
            ["SKU-002", "", "masik.jpg"]);

        List<ImageImportRow> rows = ImportProcessing.ParseImageImportRows(table);

        Assert.HasCount(2, rows);
        Assert.AreEqual(2, rows[0].RowNumber);
        Assert.AreEqual("SKU-001", rows[0].Sku);
        Assert.AreEqual("kepek", rows[0].ImagePath);
        Assert.AreEqual("egy.jpg", rows[0].ImageName);
        Assert.AreEqual("SKU-002", rows[1].Sku);
        Assert.AreEqual(string.Empty, rows[1].ImagePath);
        Assert.AreEqual("masik.jpg", rows[1].ImageName);
    }

    [TestMethod]
    public void ParseImageImportRows_ThrowsWhenRowIsOnlyPartiallyFilled()
    {
        // Azt teszteli, hogy a felig kitoltott kepsor nem mehet at csendben az importon.
        WorksheetTable table = CreateWorksheetTable(
            "Kepek",
            ["SKU", "KepUtvonal", "KepNev"],
            ["", "kepek", "egy.jpg"]);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ImportProcessing.ParseImageImportRows(table));

        StringAssert.Contains(exception.Message, "kepsor csak reszben van kitoltve");
    }

    [TestMethod]
    public void ValidateImageSheet_CountsMissingFilesUnknownSkusAndIncompleteRows()
    {
        // Azt teszteli, hogy a kepvalidacio egyszerre szamolja a hianyzo fajlt, ismeretlen SKU-t es hianyos sort.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string workbookPath = Path.Combine(tempDirectory, "import.xlsx");
            string imagesDirectory = Path.Combine(tempDirectory, "kepek");
            string validImagePath = Path.Combine(imagesDirectory, "letezo.jpg");

            Directory.CreateDirectory(imagesDirectory);
            File.WriteAllText(workbookPath, "teszt");
            File.WriteAllText(validImagePath, "kep");

            WorksheetTable table = CreateWorksheetTable(
                "Kepek",
                ["SKU", "KepUtvonal", "KepNev"],
                ["KNOWN-001", "kepek", "letezo.jpg"],
                ["UNKNOWN-001", "kepek", "letezo.jpg"],
                ["KNOWN-001", "kepek", "hianyzik.jpg"],
                ["KNOWN-001", "", ""]);

            Dictionary<string, HotcakesProduct> loadedProducts = new(StringComparer.OrdinalIgnoreCase)
            {
                ["KNOWN-001"] = new HotcakesProduct { Sku = "KNOWN-001" }
            };

            ImageSheetValidationResult result = ImportProcessing.ValidateImageSheet(
                table,
                importedProductSkus: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                loadedProductsBySku: loadedProducts,
                workbookFilePath: workbookPath);

            Assert.AreEqual(3, result.RowCount);
            Assert.AreEqual(1, result.MissingFileCount);
            Assert.AreEqual(1, result.UnknownSkuCount);
            Assert.AreEqual(1, result.IncompleteRowCount);
            Assert.IsFalse(result.CanProceed);
            StringAssert.Contains(result.DetailsMessage, "hianyzik.jpg");
            StringAssert.Contains(result.DetailsMessage, "UNKNOWN-001");
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void ValidateImageSheet_AllowsMultipleImagesForImportedSku()
    {
        // Azt teszteli, hogy ugyanahhoz az uj SKU-hoz tobb kep eseten az elso fokepkent, a tobbi tovabbi kepkent szamolodik.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string workbookPath = Path.Combine(tempDirectory, "import.xlsx");
            string imagesDirectory = Path.Combine(tempDirectory, "kepek");
            string firstImagePath = Path.Combine(imagesDirectory, "elso.jpg");
            string secondImagePath = Path.Combine(imagesDirectory, "masodik.jpg");

            Directory.CreateDirectory(imagesDirectory);
            File.WriteAllText(workbookPath, "teszt");
            File.WriteAllText(firstImagePath, "kep1");
            File.WriteAllText(secondImagePath, "kep2");

            WorksheetTable table = CreateWorksheetTable(
                "Kepek",
                ["SKU", "KepUtvonal", "KepNev"],
                ["NEW-001", "kepek", "elso.jpg"],
                ["NEW-001", "kepek", "masodik.jpg"]);

            ImageSheetValidationResult result = ImportProcessing.ValidateImageSheet(
                table,
                importedProductSkus: new HashSet<string>(["NEW-001"], StringComparer.OrdinalIgnoreCase),
                loadedProductsBySku: new Dictionary<string, HotcakesProduct>(StringComparer.OrdinalIgnoreCase),
                workbookFilePath: workbookPath);

            Assert.AreEqual(2, result.RowCount);
            Assert.AreEqual(0, result.MissingFileCount);
            Assert.AreEqual(0, result.UnknownSkuCount);
            Assert.AreEqual(0, result.IncompleteRowCount);
            Assert.IsTrue(result.CanProceed);
            StringAssert.Contains(result.DetailsMessage, "1 fokep, 1 tovabbi kep");
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    private static WorksheetTable CreateWorksheetTable(string name, string[] headers, params string[][] rows)
    {
        Dictionary<string, int> headerIndexes = headers
            .Select((header, index) => new { header, index })
            .ToDictionary(
                item => ImportUtilities.NormalizeToken(item.header),
                item => item.index,
                StringComparer.Ordinal);

        List<(int RowNumber, string[] Values)> dataRows = rows
            .Select((row, index) => (RowNumber: index + 2, Values: row))
            .ToList();

        return new WorksheetTable(name, headerIndexes, dataRows);
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
