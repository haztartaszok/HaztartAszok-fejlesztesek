namespace WinFormsApp1.Tests;

[TestClass]
public sealed class PropertyImportTests
{
    [TestMethod]
    public void ParsePropertyImportRows_ParsesFilledRowsAndSkipsEmptyRows()
    {
        // Azt teszteli, hogy a tulajdonsagimport csak a tenylegesen kitoltott sorokat tartja meg.
        WorksheetTable table = CreateWorksheetTable(
            "OpciokTulajdonsagok",
            ["SKU", "TulajdonsagNev", "TulajdonsagErtek"],
            ["SKU-001", "Szin", "Piros"],
            ["", "", ""],
            ["SKU-002", "Meret", "XL"]);

        List<PropertyImportRow> rows = ImportProcessing.ParsePropertyImportRows(table);

        Assert.HasCount(2, rows);
        Assert.AreEqual("SKU-001", rows[0].Sku);
        Assert.AreEqual("Szin", rows[0].PropertyName);
        Assert.AreEqual("Piros", rows[0].PropertyValue);
        Assert.AreEqual("SKU-002", rows[1].Sku);
        Assert.AreEqual("Meret", rows[1].PropertyName);
        Assert.AreEqual("XL", rows[1].PropertyValue);
    }

    [TestMethod]
    public void ParsePropertyImportRows_ThrowsWhenRowIsOnlyPartiallyFilled()
    {
        // Azt teszteli, hogy a hianyos tulajdonsagsor ervenyessegi hibara fusson.
        WorksheetTable table = CreateWorksheetTable(
            "OpciokTulajdonsagok",
            ["SKU", "TulajdonsagNev", "TulajdonsagErtek"],
            ["SKU-001", "", "Piros"]);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ImportProcessing.ParsePropertyImportRows(table));

        StringAssert.Contains(exception.Message, "tulajdonsagsor csak reszben van kitoltve");
    }

    [TestMethod]
    public void ValidatePropertySheet_CountsUnknownSkusIncompleteRowsAndInvalidProperties()
    {
        // Azt teszteli, hogy a tulajdonsagvalidacio kulon kezeli az ismeretlen SKU-t,
        // a hianyos sort es a nem importalhato property helyzeteket.
        WorksheetTable table = CreateWorksheetTable(
            "OpciokTulajdonsagok",
            ["SKU", "TulajdonsagNev", "TulajdonsagErtek"],
            ["KNOWN-001", "Szin", "Piros"],
            ["NEW-001", "Anyag", "Fem"],
            ["NO-TYPE-001", "Meret", "L"],
            ["AMB-001", "NemEgyertelmu", "Ertek"],
            ["UNKNOWN-001", "Marka", "Asz"],
            ["KNOWN-001", "", "Hibas"]);

        Dictionary<string, HotcakesProduct> loadedProducts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["KNOWN-001"] = new HotcakesProduct { Sku = "KNOWN-001", ProductTypeId = "TYPE-1" },
            ["NO-TYPE-001"] = new HotcakesProduct { Sku = "NO-TYPE-001", ProductTypeId = string.Empty },
            ["AMB-001"] = new HotcakesProduct { Sku = "AMB-001", ProductTypeId = "TYPE-2" }
        };

        Dictionary<string, string> importedProductTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["NEW-001"] = "TYPE-3"
        };

        PropertySheetValidationResult result = ImportProcessing.ValidatePropertySheet(
            table,
            importedProductSkus: new HashSet<string>(["NEW-001"], StringComparer.OrdinalIgnoreCase),
            loadedProductsBySku: loadedProducts,
            importedProductTypeValuesBySku: importedProductTypes,
            propertyResolver: propertyName =>
            {
                return propertyName switch
                {
                    "Szin" => (true, null),
                    "Anyag" => (false, null),
                    "NemEgyertelmu" => (false, "nem egyertelmu a property feloldasa"),
                    _ => (false, null)
                };
            });

        Assert.AreEqual(5, result.RowCount);
        Assert.AreEqual(1, result.UnknownSkuCount);
        Assert.AreEqual(1, result.IncompleteRowCount);
        Assert.AreEqual(2, result.InvalidPropertyCount);
        Assert.IsFalse(result.CanProceed);
        StringAssert.Contains(result.DetailsMessage, "UNKNOWN-001");
        StringAssert.Contains(result.DetailsMessage, "nincs feloldhato TermekTipus");
        StringAssert.Contains(result.DetailsMessage, "nem egyertelmu a property feloldasa");
        StringAssert.Contains(result.DetailsMessage, "Ujonnan letrehozhato Hotcakes property-k: 1");
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
