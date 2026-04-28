namespace WinFormsApp1.Tests;

[TestClass]
public sealed class ImportWorksheetBuilderTests
{
    [TestMethod]
    public void BuildWorksheetTable_UsesHeaderRowAndSkipsEmptyRows()
    {
        // Azt teszteli, hogy a fejlec sorbol keszul oszloptabla, es a teljesen ures adatsorok kiesnek.
        WorksheetPreview worksheet = new(
            "Termekek",
            [
                ["SKU", "Nev", "Ar"],
                ["SKU-001", "Termek 1", "1000"],
                ["", "", ""],
                ["SKU-002", "Termek 2"]
            ]);

        WorksheetTable table = ImportWorksheetBuilder.BuildWorksheetTable(worksheet);

        Assert.AreEqual("Termekek", table.Name);
        Assert.AreEqual(0, table.HeaderIndexes["SKU"]);
        Assert.AreEqual(1, table.HeaderIndexes["NEV"]);
        Assert.AreEqual(2, table.HeaderIndexes["AR"]);
        Assert.HasCount(2, table.Rows);
        Assert.AreEqual(2, table.Rows[0].RowNumber);
        CollectionAssert.AreEqual(new[] { "SKU-001", "Termek 1", "1000" }, table.Rows[0].Values);
        CollectionAssert.AreEqual(new[] { "SKU-002", "Termek 2", "" }, table.Rows[1].Values);
    }

    [TestMethod]
    public void BuildWorksheetTable_ThrowsWhenWorksheetIsEmpty()
    {
        // Azt teszteli, hogy ures munkalapbol nem lehet importtablat epiteni.
        WorksheetPreview worksheet = new("Termekek", []);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ImportWorksheetBuilder.BuildWorksheetTable(worksheet));

        StringAssert.Contains(exception.Message, "munkalap ures");
    }
}
