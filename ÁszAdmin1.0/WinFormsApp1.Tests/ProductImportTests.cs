namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ProductImportTests
{
    [TestMethod]
    public void ParseProductImportRows_ParsesFilledRowsAndSkipsRowsWithoutSku()
    {
        // Azt teszteli, hogy a termekimport csak a SKU-val rendelkezo sorokat veszi fel,
        // es a kitoltott mezoket helyes tipusokra alakitja.
        WorksheetTable table = CreateWorksheetTable(
            "Termekek",
            ["SKU", "Nev", "Ar", "Keszlet", "TermekTipus", "Leiras"],
            ["SKU-001", "Termek 1", "1234.50", "7", "Alap", "Leiras 1"],
            ["", "Ures SKU", "999", "1", "Alap", "Nem szamit"],
            ["SKU-002", "Termek 2", "", "", "Masik", "Leiras 2"]);

        List<ProductImportRow> rows = ImportProcessing.ParseProductImportRows(table);

        Assert.HasCount(2, rows);
        Assert.AreEqual(2, rows[0].RowNumber);
        Assert.AreEqual("SKU-001", rows[0].Sku);
        Assert.AreEqual("Termek 1", rows[0].Name);
        Assert.AreEqual(1234.50m, rows[0].Price);
        Assert.AreEqual(7, rows[0].Stock);
        Assert.AreEqual("Alap", rows[0].ProductTypeName);
        Assert.AreEqual("Leiras 1", rows[0].Description);
        Assert.AreEqual("SKU-002", rows[1].Sku);
        Assert.IsNull(rows[1].Price);
        Assert.IsNull(rows[1].Stock);
    }

    [TestMethod]
    public void ParseProductImportRows_ThrowsWhenPriceIsInvalid()
    {
        // Azt teszteli, hogy hibas ar esetben mar az import elott ervenyessegi hiba keletkezik.
        WorksheetTable table = CreateWorksheetTable(
            "Termekek",
            ["SKU", "Ar"],
            ["SKU-001", "nem-szam"]);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ImportProcessing.ParseProductImportRows(table));

        StringAssert.Contains(exception.Message, "Ar mezoje nem ervenyes");
    }

    [TestMethod]
    public void BuildImportedProduct_CreatesActiveProductWithStockAwareInventoryMode()
    {
        // Azt teszteli, hogy uj termek letrehozasakor a keszletnek megfelelo inventory mod all be.
        ProductImportRow row = new(
            RowNumber: 2,
            Sku: "SKU-100",
            Name: "Teszt termek",
            Price: 4990m,
            Stock: 3,
            ProductTypeName: "Muszaki",
            Description: "Reszletes leiras");

        HotcakesProduct product = ImportProcessing.BuildImportedProduct(row, "TYPE-001");

        Assert.AreEqual("SKU-100", product.Sku);
        Assert.AreEqual("Teszt termek", product.ProductName);
        Assert.AreEqual("TYPE-001", product.ProductTypeId);
        Assert.AreEqual(4990m, product.ListPrice);
        Assert.AreEqual(4990m, product.SitePrice);
        Assert.AreEqual("Reszletes leiras", product.LongDescription);
        Assert.IsTrue(product.IsSearchable);
        Assert.IsTrue(product.IsAvailableForSale);
        Assert.IsTrue(product.AllowReviews.HasValue && product.AllowReviews.Value);
        Assert.AreEqual(HotcakesProductStatuses.Active, product.Status);
        Assert.AreEqual(HotcakesInventoryModes.WhenOutOfStockShow, product.InventoryMode);
    }

    [TestMethod]
    public void ApplyImportedProductValues_UpdatesOnlyProvidedFieldsForExistingProduct()
    {
        // Azt teszteli, hogy meglevo termek frissitesekor csak a tenylegesen megadott importadatok irjak felul a mezoket.
        HotcakesProduct product = new()
        {
            ProductName = "Meglevo nev",
            ProductTypeId = "OLD-TYPE",
            ListPrice = 1200m,
            SitePrice = 1200m,
            LongDescription = "Regi leiras",
            InventoryMode = HotcakesInventoryModes.AlwayInStock,
            AllowReviews = false,
            IsSearchable = false,
            IsAvailableForSale = false,
            Status = HotcakesProductStatuses.Disabled
        };

        ProductImportRow row = new(
            RowNumber: 3,
            Sku: "SKU-200",
            Name: string.Empty,
            Price: 1599m,
            Stock: 8,
            ProductTypeName: string.Empty,
            Description: string.Empty);

        ImportProcessing.ApplyImportedProductValues(product, row, isNewProduct: false, productTypeId: "NEW-TYPE");

        Assert.AreEqual("Meglevo nev", product.ProductName);
        Assert.AreEqual("NEW-TYPE", product.ProductTypeId);
        Assert.AreEqual(1599m, product.ListPrice);
        Assert.AreEqual(1599m, product.SitePrice);
        Assert.AreEqual("Regi leiras", product.LongDescription);
        Assert.AreEqual(HotcakesInventoryModes.WhenOutOfStockShow, product.InventoryMode);
        Assert.IsFalse(product.AllowReviews.HasValue && product.AllowReviews.Value);
        Assert.IsFalse(product.IsSearchable);
        Assert.IsFalse(product.IsAvailableForSale);
        Assert.AreEqual(HotcakesProductStatuses.Disabled, product.Status);
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
