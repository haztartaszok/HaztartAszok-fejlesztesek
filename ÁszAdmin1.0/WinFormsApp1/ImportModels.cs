namespace WinFormsApp1
{
    internal sealed record WorksheetPreview(string Name, List<string[]> Rows);

    internal sealed record WorksheetTable(
        string Name,
        Dictionary<string, int> HeaderIndexes,
        List<(int RowNumber, string[] Values)> Rows);

    internal sealed record ProductImportRow(
        int RowNumber,
        string Sku,
        string Name,
        decimal? Price,
        int? Stock,
        string ProductTypeName,
        string Description);

    internal sealed record CategoryImportRow(
        int RowNumber,
        string Sku,
        string CategorySlug);

    internal sealed record ImageImportRow(
        int RowNumber,
        string Sku,
        string ImagePath,
        string ImageName);

    internal sealed record PropertyImportRow(
        int RowNumber,
        string Sku,
        string PropertyName,
        string PropertyValue);

    internal sealed record CategorySheetValidationResult(
        int RowCount,
        int UnknownCategorySlugCount,
        int UnknownCategorySkuCount,
        int IncompleteRowCount,
        bool CanProceed,
        string DetailsMessage);

    internal sealed record ImageSheetValidationResult(
        int RowCount,
        int MissingFileCount,
        int UnknownSkuCount,
        int IncompleteRowCount,
        bool CanProceed,
        string DetailsMessage);

    internal sealed record PropertySheetValidationResult(
        int RowCount,
        int UnknownSkuCount,
        int IncompleteRowCount,
        int InvalidPropertyCount,
        bool CanProceed,
        string DetailsMessage);
}
