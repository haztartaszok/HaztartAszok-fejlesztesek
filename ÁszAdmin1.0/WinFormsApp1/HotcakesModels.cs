using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    internal sealed class HotcakesApiResponse<T>
    {
        public T? Content { get; init; }

        public List<HotcakesApiError> Errors { get; init; } = [];
    }

    internal sealed class HotcakesApiError
    {
        public string Code { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Code)
                ? Description
                : $"{Code}: {Description}";
        }
    }

    internal sealed class HotcakesCategorySnapshot
    {
        public string Bvin { get; set; } = string.Empty;

        public string ParentId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RewriteUrl { get; set; } = string.Empty;

        public string CustomPageUrl { get; set; } = string.Empty;

        public bool Hidden { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    internal sealed class HotcakesProductPage
    {
        public List<HotcakesProduct> Products { get; init; } = [];

        public int TotalProductCount { get; init; }
    }

    internal sealed class HotcakesProduct
    {
        public string Bvin { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string ProductTypeId { get; set; } = string.Empty;

        public decimal ListPrice { get; set; }

        public decimal SitePrice { get; set; }

        public string ShortDescription { get; set; } = string.Empty;

        public string LongDescription { get; set; } = string.Empty;

        public string UrlSlug { get; set; } = string.Empty;

        public bool IsSearchable { get; set; } = true;

        public bool Featured { get; set; }

        public bool IsAvailableForSale { get; set; } = true;

        public bool TaxExempt { get; set; }

        public bool? AllowReviews { get; set; }

        public long StoreId { get; set; }

        public int Status { get; set; } = HotcakesProductStatuses.Active;

        public int InventoryMode { get; set; } = HotcakesInventoryModes.AlwayInStock;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    internal sealed class HotcakesProductInventory
    {
        public string Bvin { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; }

        public string ProductBvin { get; set; } = string.Empty;

        public string VariantId { get; set; } = string.Empty;

        public int QuantityOnHand { get; set; }

        public int QuantityReserved { get; set; }

        public int LowStockPoint { get; set; }

        public int OutOfStockPoint { get; set; }
    }

    internal sealed class HotcakesCategoryProductAssociation
    {
        public long Id { get; set; }

        public string CategoryId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public long StoreId { get; set; }
    }

    internal static class HotcakesInventoryModes
    {
        public const int NotSet = -1;
        public const int Unknown = 0;
        public const int AlwayInStock = 100;
        public const int WhenOutOfStockHide = 101;
        public const int WhenOutOfStockShow = 102;
        public const int WhenOutOfStockAllowBackorders = 103;
    }

    internal static class HotcakesProductStatuses
    {
        public const int Disabled = 0;
        public const int Active = 1;
        public const int NotSet = -1;
    }
}
