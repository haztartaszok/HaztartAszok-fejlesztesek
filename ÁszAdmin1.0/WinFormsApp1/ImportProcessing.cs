using System.Text;

namespace WinFormsApp1
{
    internal static class ImportProcessing
    {
        public static HotcakesProduct BuildImportedProduct(ProductImportRow row, string productTypeId)
        {
            ArgumentNullException.ThrowIfNull(row);

            HotcakesProduct product = new()
            {
                Sku = row.Sku,
                ProductName = row.Name,
                ProductTypeId = productTypeId,
                ListPrice = row.Price ?? 0m,
                SitePrice = row.Price ?? 0m,
                LongDescription = row.Description,
                IsSearchable = true,
                IsAvailableForSale = true,
                AllowReviews = true,
                Status = HotcakesProductStatuses.Active,
                TaxExempt = false,
                InventoryMode = row.Stock.HasValue
                    ? HotcakesInventoryModes.WhenOutOfStockShow
                    : HotcakesInventoryModes.AlwayInStock
            };

            return product;
        }

        public static void ApplyImportedProductValues(
            HotcakesProduct product,
            ProductImportRow row,
            bool isNewProduct,
            string productTypeId)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(row);

            if (isNewProduct || !string.IsNullOrWhiteSpace(row.Name))
            {
                product.ProductName = row.Name;
            }

            if (!string.IsNullOrWhiteSpace(productTypeId))
            {
                product.ProductTypeId = productTypeId;
            }

            if (row.Price.HasValue)
            {
                product.ListPrice = row.Price.Value;
                product.SitePrice = row.Price.Value;
            }

            if (!string.IsNullOrWhiteSpace(row.Description))
            {
                product.LongDescription = row.Description;
            }

            if (row.Stock.HasValue &&
                (product.InventoryMode == HotcakesInventoryModes.NotSet ||
                 product.InventoryMode == HotcakesInventoryModes.Unknown ||
                 product.InventoryMode == HotcakesInventoryModes.AlwayInStock))
            {
                product.InventoryMode = HotcakesInventoryModes.WhenOutOfStockShow;
            }

            if (isNewProduct)
            {
                product.AllowReviews ??= true;
                product.IsSearchable = true;
                product.IsAvailableForSale = true;
                product.Status = HotcakesProductStatuses.Active;
            }
        }

        public static List<ProductImportRow> ParseProductImportRows(WorksheetTable productTable)
        {
            ArgumentNullException.ThrowIfNull(productTable);

            int skuColumnIndex = GetRequiredColumnIndex(productTable, "SKU");
            int nameColumnIndex = GetOptionalColumnIndex(productTable, "Nev", "Name");
            int priceColumnIndex = GetOptionalColumnIndex(productTable, "Ar", "Price");
            int stockColumnIndex = GetOptionalColumnIndex(productTable, "Keszlet", "Inventory", "Stock");
            int productTypeColumnIndex = GetOptionalColumnIndex(productTable, "TermekTipus", "ProductType", "ProductTypeName");
            int descriptionColumnIndex = GetOptionalColumnIndex(productTable, "Leiras", "Description", "LongDescription");

            List<ProductImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in productTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);

                if (string.IsNullOrWhiteSpace(sku))
                {
                    continue;
                }

                string rawPrice = GetCellValue(rowValues, priceColumnIndex);
                decimal? price = null;

                if (!string.IsNullOrWhiteSpace(rawPrice))
                {
                    if (!ImportUtilities.TryParseImportDecimal(rawPrice, out decimal parsedPrice))
                    {
                        throw new InvalidOperationException($"A(z) {rowNumber}. sor Ar mezoje nem ervenyes: {rawPrice}");
                    }

                    price = parsedPrice;
                }

                string rawStock = GetCellValue(rowValues, stockColumnIndex);
                int? stock = null;

                if (!string.IsNullOrWhiteSpace(rawStock))
                {
                    if (!ImportUtilities.TryParseImportInt(rawStock, out int parsedStock))
                    {
                        throw new InvalidOperationException($"A(z) {rowNumber}. sor Keszlet mezoje nem ervenyes: {rawStock}");
                    }

                    stock = parsedStock;
                }

                rows.Add(new ProductImportRow(
                    rowNumber,
                    sku,
                    GetCellValue(rowValues, nameColumnIndex),
                    price,
                    stock,
                    GetCellValue(rowValues, productTypeColumnIndex),
                    GetCellValue(rowValues, descriptionColumnIndex)));
            }

            return rows;
        }

        public static List<CategoryImportRow> ParseCategoryImportRows(WorksheetTable? categoryTable)
        {
            if (categoryTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(categoryTable, "SKU");
            int categorySlugColumnIndex = GetRequiredColumnIndex(categoryTable, "KategoriaSlug", "CategorySlug", "RewriteUrl");

            List<CategoryImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in categoryTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string categorySlug = GetCellValue(rowValues, categorySlugColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(categorySlug))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(categorySlug))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. kategoriarow csak reszben van kitoltve.");
                }

                rows.Add(new CategoryImportRow(rowNumber, sku, categorySlug));
            }

            return rows;
        }

        public static List<ImageImportRow> ParseImageImportRows(WorksheetTable? imageTable)
        {
            if (imageTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(imageTable, "SKU");
            int imagePathColumnIndex = GetOptionalColumnIndex(imageTable, "KepUtvonal", "ImagePath", "ImageFolder");
            int imageNameColumnIndex = GetOptionalColumnIndex(imageTable, "KepNev", "ImageName", "FileName");

            List<ImageImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in imageTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string imagePath = GetCellValue(rowValues, imagePathColumnIndex);
                string imageName = GetCellValue(rowValues, imageNameColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(imagePath) &&
                    string.IsNullOrWhiteSpace(imageName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || (string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName)))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. kepsor csak reszben van kitoltve.");
                }

                rows.Add(new ImageImportRow(rowNumber, sku, imagePath, imageName));
            }

            return rows;
        }

        public static List<PropertyImportRow> ParsePropertyImportRows(WorksheetTable? propertyTable)
        {
            if (propertyTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(propertyTable, "SKU");
            int propertyNameColumnIndex = GetRequiredColumnIndex(propertyTable, "TulajdonsagNev", "PropertyName", "Key");
            int propertyValueColumnIndex = GetOptionalColumnIndex(propertyTable, "TulajdonsagErtek", "PropertyValue", "Value");

            List<PropertyImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in propertyTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string propertyName = GetCellValue(rowValues, propertyNameColumnIndex);
                string propertyValue = GetCellValue(rowValues, propertyValueColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(propertyName) &&
                    string.IsNullOrWhiteSpace(propertyValue))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. tulajdonsagsor csak reszben van kitoltve.");
                }

                rows.Add(new PropertyImportRow(rowNumber, sku, propertyName, propertyValue));
            }

            return rows;
        }

        public static CategorySheetValidationResult ValidateCategorySheet(
            WorksheetTable? categoryTable,
            IReadOnlySet<string> importedProductSkus,
            IReadOnlyDictionary<string, HotcakesProduct> loadedProductsBySku,
            IReadOnlyCollection<HotcakesCategorySnapshot> loadedCategories)
        {
            ArgumentNullException.ThrowIfNull(importedProductSkus);
            ArgumentNullException.ThrowIfNull(loadedProductsBySku);
            ArgumentNullException.ThrowIfNull(loadedCategories);

            if (categoryTable is null)
            {
                return new CategorySheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int categorySkuColumnIndex = GetRequiredColumnIndex(categoryTable, "SKU");
            int categorySlugColumnIndex = GetRequiredColumnIndex(categoryTable, "KategoriaSlug", "CategorySlug", "RewriteUrl");
            HashSet<string> knownCategorySlugs = loadedCategories
                .Select(category => ImportUtilities.NormalizeToken(category.RewriteUrl))
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .ToHashSet(StringComparer.Ordinal);

            int rowCount = 0;
            int unknownCategorySkuCount = 0;
            int incompleteCategoryRowCount = 0;
            HashSet<string> unknownCategorySlugs = new(StringComparer.OrdinalIgnoreCase);
            List<string> unknownCategorySkus = [];

            foreach ((int rowNumber, string[] rowValues) in categoryTable.Rows)
            {
                string sku = GetCellValue(rowValues, categorySkuColumnIndex);
                string categorySlug = GetCellValue(rowValues, categorySlugColumnIndex);

                bool hasSku = !string.IsNullOrWhiteSpace(sku);
                bool hasCategorySlug = !string.IsNullOrWhiteSpace(categorySlug);

                if (!hasSku && !hasCategorySlug)
                {
                    continue;
                }

                if (!hasSku || !hasCategorySlug)
                {
                    incompleteCategoryRowCount++;
                    continue;
                }

                rowCount++;

                if (!knownCategorySlugs.Contains(ImportUtilities.NormalizeToken(categorySlug)))
                {
                    unknownCategorySlugs.Add(categorySlug);
                }

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownCategorySkuCount++;
                    unknownCategorySkus.Add($"{sku} (sor {rowNumber})");
                }
            }

            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Kategoria munkalap: {categoryTable.Name}");
            detailsBuilder.AppendLine($"Kategoriarendeles sorok: {rowCount}");
            detailsBuilder.AppendLine($"Ismeretlen KategoriaSlug ertekek: {unknownCategorySlugs.Count}");
            detailsBuilder.AppendLine($"Nem feloldhato kategoriak SKU alapjan: {unknownCategorySkuCount}");
            detailsBuilder.AppendLine($"Hianyos kategoriarow-k: {incompleteCategoryRowCount}");

            if (unknownCategorySlugs.Count > 0)
            {
                detailsBuilder.AppendLine($"Ismeretlen kategoriak: {string.Join(", ", unknownCategorySlugs.Take(5))}");
            }

            if (unknownCategorySkus.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato kategoriarow-k: {string.Join(", ", unknownCategorySkus.Take(5))}");
            }

            bool canProceed = unknownCategorySlugs.Count == 0 &&
                              unknownCategorySkuCount == 0 &&
                              incompleteCategoryRowCount == 0;

            return new CategorySheetValidationResult(
                rowCount,
                unknownCategorySlugs.Count,
                unknownCategorySkuCount,
                incompleteCategoryRowCount,
                canProceed,
                detailsBuilder.ToString());
        }

        public static ImageSheetValidationResult ValidateImageSheet(
            WorksheetTable? imageTable,
            IReadOnlySet<string> importedProductSkus,
            IReadOnlyDictionary<string, HotcakesProduct> loadedProductsBySku,
            string workbookFilePath)
        {
            ArgumentNullException.ThrowIfNull(importedProductSkus);
            ArgumentNullException.ThrowIfNull(loadedProductsBySku);
            ArgumentException.ThrowIfNullOrWhiteSpace(workbookFilePath);

            if (imageTable is null)
            {
                return new ImageSheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int skuColumnIndex = GetRequiredColumnIndex(imageTable, "SKU");
            int imagePathColumnIndex = GetOptionalColumnIndex(imageTable, "KepUtvonal", "ImagePath", "ImageFolder");
            int imageNameColumnIndex = GetOptionalColumnIndex(imageTable, "KepNev", "ImageName", "FileName");

            int rowCount = 0;
            int missingFileCount = 0;
            int unknownSkuCount = 0;
            int incompleteRowCount = 0;

            List<string> missingFileRows = [];
            List<string> unknownSkuRows = [];
            Dictionary<string, int> uploadableImageRowCountsBySku = new(StringComparer.OrdinalIgnoreCase);

            foreach ((int rowNumber, string[] rowValues) in imageTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string imagePath = GetCellValue(rowValues, imagePathColumnIndex);
                string imageName = GetCellValue(rowValues, imageNameColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || (string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName)))
                {
                    incompleteRowCount++;
                    continue;
                }

                rowCount++;

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownSkuCount++;
                    unknownSkuRows.Add($"{sku} (sor {rowNumber})");
                    continue;
                }

                if (!ImportUtilities.TryResolveImageFilePath(workbookFilePath, imagePath, imageName, out string? resolvedPath))
                {
                    missingFileCount++;
                    missingFileRows.Add($"{sku} (sor {rowNumber}) -> {ImportUtilities.BuildImageReference(imagePath, imageName)}");
                    continue;
                }

                if (!File.Exists(resolvedPath))
                {
                    missingFileCount++;
                    missingFileRows.Add($"{sku} (sor {rowNumber}) -> {resolvedPath}");
                    continue;
                }

                uploadableImageRowCountsBySku[sku] = uploadableImageRowCountsBySku.TryGetValue(sku, out int existingCount)
                    ? existingCount + 1
                    : 1;
            }

            int uploadableMainImageCount = uploadableImageRowCountsBySku.Count;
            int uploadableAdditionalImageCount = uploadableImageRowCountsBySku.Values.Sum(static count => Math.Max(0, count - 1));
            int multiImageSkuCount = uploadableImageRowCountsBySku.Values.Count(static count => count > 1);
            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Kepek munkalap: {imageTable.Name}");
            detailsBuilder.AppendLine($"Kepsorok: {rowCount}");
            detailsBuilder.AppendLine($"Hianyzo vagy nem feloldhato kepfajlok: {missingFileCount}");
            detailsBuilder.AppendLine($"Nem feloldhato SKU-k: {unknownSkuCount}");
            detailsBuilder.AppendLine($"Hianyos kepsorok: {incompleteRowCount}");
            detailsBuilder.AppendLine($"Tobb kepet kapo SKU-k: {multiImageSkuCount}");
            detailsBuilder.AppendLine($"Feltoltheto kepek bontasa: {uploadableMainImageCount} fokep, {uploadableAdditionalImageCount} tovabbi kep");

            if (missingFileRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Pelda hianyzo kepfajlok: {string.Join(", ", missingFileRows.Take(5))}");
            }

            if (unknownSkuRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato kep SKU-k: {string.Join(", ", unknownSkuRows.Take(5))}");
            }

            if (rowCount > 0)
            {
                detailsBuilder.AppendLine("SKU-nkent az elso sikeresen feloldott kep fokepkent, a tobbi tovabbi kepkent kerul feltoltesre.");
            }

            bool canProceed = missingFileCount == 0 &&
                              unknownSkuCount == 0 &&
                              incompleteRowCount == 0;

            return new ImageSheetValidationResult(
                rowCount,
                missingFileCount,
                unknownSkuCount,
                incompleteRowCount,
                canProceed,
                detailsBuilder.ToString());
        }

        public static PropertySheetValidationResult ValidatePropertySheet(
            WorksheetTable? propertyTable,
            IReadOnlySet<string> importedProductSkus,
            IReadOnlyDictionary<string, HotcakesProduct> loadedProductsBySku,
            IReadOnlyDictionary<string, string> importedProductTypeValuesBySku,
            Func<string, (bool HasExistingProperty, string? FailureReason)> propertyResolver)
        {
            ArgumentNullException.ThrowIfNull(importedProductSkus);
            ArgumentNullException.ThrowIfNull(loadedProductsBySku);
            ArgumentNullException.ThrowIfNull(importedProductTypeValuesBySku);
            ArgumentNullException.ThrowIfNull(propertyResolver);

            if (propertyTable is null)
            {
                return new PropertySheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int skuColumnIndex = GetRequiredColumnIndex(propertyTable, "SKU");
            int propertyNameColumnIndex = GetRequiredColumnIndex(propertyTable, "TulajdonsagNev", "PropertyName", "Key");
            int propertyValueColumnIndex = GetOptionalColumnIndex(propertyTable, "TulajdonsagErtek", "PropertyValue", "Value");

            int rowCount = 0;
            int unknownSkuCount = 0;
            int incompleteRowCount = 0;
            int invalidPropertyCount = 0;
            List<string> unknownSkuRows = [];
            List<string> invalidPropertyRows = [];
            HashSet<string> createablePropertyTokens = new(StringComparer.Ordinal);

            foreach ((int rowNumber, string[] rowValues) in propertyTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string propertyName = GetCellValue(rowValues, propertyNameColumnIndex);
                string propertyValue = GetCellValue(rowValues, propertyValueColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(propertyName) &&
                    string.IsNullOrWhiteSpace(propertyValue))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(propertyName))
                {
                    incompleteRowCount++;
                    continue;
                }

                rowCount++;

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownSkuCount++;
                    unknownSkuRows.Add($"{sku} (sor {rowNumber})");
                    continue;
                }

                bool hasProductType = loadedProductsBySku.TryGetValue(sku, out HotcakesProduct? existingProduct) &&
                                      !string.IsNullOrWhiteSpace(existingProduct.ProductTypeId);

                if (!hasProductType &&
                    importedProductTypeValuesBySku.TryGetValue(sku, out string? importedProductTypeValue) &&
                    !string.IsNullOrWhiteSpace(importedProductTypeValue))
                {
                    hasProductType = true;
                }

                if (!hasProductType)
                {
                    invalidPropertyCount++;
                    invalidPropertyRows.Add($"{sku} (sor {rowNumber}) -> a termekhez nincs feloldhato TermekTipus.");
                    continue;
                }

                (bool hasExistingProperty, string? propertyFailureReason) = propertyResolver(propertyName);

                if (!string.IsNullOrWhiteSpace(propertyFailureReason))
                {
                    invalidPropertyCount++;
                    invalidPropertyRows.Add($"{sku} (sor {rowNumber}) -> {propertyName} ({propertyFailureReason})");
                    continue;
                }

                if (!hasExistingProperty)
                {
                    createablePropertyTokens.Add(ImportUtilities.NormalizeToken(propertyName));
                }
            }

            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Tulajdonsag munkalap: {propertyTable.Name}");
            detailsBuilder.AppendLine($"Tulajdonsagsorok: {rowCount}");
            detailsBuilder.AppendLine($"Nem feloldhato SKU-k: {unknownSkuCount}");
            detailsBuilder.AppendLine($"Hianyos tulajdonsagsorok: {incompleteRowCount}");
            detailsBuilder.AppendLine($"Nem importalhato tulajdonsagsorok: {invalidPropertyCount}");
            detailsBuilder.AppendLine($"Ujonnan letrehozhato Hotcakes property-k: {createablePropertyTokens.Count}");

            if (unknownSkuRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato tulajdonsag SKU-k: {string.Join(", ", unknownSkuRows.Take(5))}");
            }

            if (invalidPropertyRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem importalhato tulajdonsagsorok: {string.Join(", ", invalidPropertyRows.Take(5))}");
            }

            bool canProceed = unknownSkuCount == 0 &&
                              incompleteRowCount == 0 &&
                              invalidPropertyCount == 0;

            return new PropertySheetValidationResult(
                rowCount,
                unknownSkuCount,
                incompleteRowCount,
                invalidPropertyCount,
                canProceed,
                detailsBuilder.ToString());
        }

        private static int GetRequiredColumnIndex(WorksheetTable worksheet, params string[] aliases)
        {
            int index = GetOptionalColumnIndex(worksheet, aliases);

            if (index >= 0)
            {
                return index;
            }

            throw new InvalidOperationException(
                $"A '{worksheet.Name}' munkalaprol hianyzik a(z) {string.Join(" / ", aliases)} oszlop.");
        }

        private static int GetOptionalColumnIndex(WorksheetTable worksheet, params string[] aliases)
        {
            foreach (string alias in aliases)
            {
                if (worksheet.HeaderIndexes.TryGetValue(ImportUtilities.NormalizeToken(alias), out int index))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string GetCellValue(string[] rowValues, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= rowValues.Length)
            {
                return string.Empty;
            }

            return rowValues[columnIndex].Trim();
        }
    }
}
