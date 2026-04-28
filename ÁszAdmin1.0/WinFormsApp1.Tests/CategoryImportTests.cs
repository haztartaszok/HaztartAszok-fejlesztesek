namespace WinFormsApp1.Tests;

[TestClass]
public sealed class CategoryImportTests
{
    [TestMethod]
    public void ParseCategoryImportRows_ParsesFilledRowsAndSkipsEmptyRows()
    {
        // Azt teszteli, hogy a kategoriarendeles csak a valoban kitoltott sorokat tartja meg.
        WorksheetTable table = CreateWorksheetTable(
            "Kategoriak",
            ["SKU", "KategoriaSlug"],
            ["SKU-001", "haztartasi-gep"],
            ["", ""],
            ["SKU-002", "akcios"]);

        List<CategoryImportRow> rows = ImportProcessing.ParseCategoryImportRows(table);

        Assert.HasCount(2, rows);
        Assert.AreEqual("SKU-001", rows[0].Sku);
        Assert.AreEqual("haztartasi-gep", rows[0].CategorySlug);
        Assert.AreEqual("SKU-002", rows[1].Sku);
        Assert.AreEqual("akcios", rows[1].CategorySlug);
    }

    [TestMethod]
    public void ParseCategoryImportRows_ThrowsWhenRowIsOnlyPartiallyFilled()
    {
        // Azt teszteli, hogy hianyos kategoriarendelesi sor eseten az import hibaval leall.
        WorksheetTable table = CreateWorksheetTable(
            "Kategoriak",
            ["SKU", "KategoriaSlug"],
            ["SKU-001", ""]);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ImportProcessing.ParseCategoryImportRows(table));

        StringAssert.Contains(exception.Message, "kategoriarow csak reszben van kitoltve");
    }

    [TestMethod]
    public void ValidateCategorySheet_CountsUnknownSlugsUnknownSkusAndIncompleteRows()
    {
        // Azt teszteli, hogy a kategoriavalidacio kulon szamolja az ismeretlen slugokat, SKU-kat es a hianyos sorokat.
        WorksheetTable table = CreateWorksheetTable(
            "Kategoriak",
            ["SKU", "KategoriaSlug"],
            ["KNOWN-001", "haztartasi-gep"],
            ["UNKNOWN-001", "haztartasi-gep"],
            ["KNOWN-001", "nem-letezo-slug"],
            ["KNOWN-001", ""]);

        Dictionary<string, HotcakesProduct> loadedProducts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["KNOWN-001"] = new HotcakesProduct { Sku = "KNOWN-001" }
        };

        List<HotcakesCategorySnapshot> categories =
        [
            new HotcakesCategorySnapshot { RewriteUrl = "haztartasi-gep" }
        ];

        CategorySheetValidationResult result = ImportProcessing.ValidateCategorySheet(
            table,
            importedProductSkus: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            loadedProductsBySku: loadedProducts,
            loadedCategories: categories);

        Assert.AreEqual(3, result.RowCount);
        Assert.AreEqual(1, result.UnknownCategorySlugCount);
        Assert.AreEqual(1, result.UnknownCategorySkuCount);
        Assert.AreEqual(1, result.IncompleteRowCount);
        Assert.IsFalse(result.CanProceed);
        StringAssert.Contains(result.DetailsMessage, "nem-letezo-slug");
        StringAssert.Contains(result.DetailsMessage, "UNKNOWN-001");
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
}
