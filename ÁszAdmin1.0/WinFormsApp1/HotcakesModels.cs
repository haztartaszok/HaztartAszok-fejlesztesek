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
        public string Bvin { get; init; } = string.Empty;

        public string ParentId { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string RewriteUrl { get; init; } = string.Empty;

        public string CustomPageUrl { get; init; } = string.Empty;

        public bool Hidden { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    internal sealed class HotcakesProductPage
    {
        public List<HotcakesProduct> Products { get; init; } = [];

        public int TotalProductCount { get; init; }
    }

    internal sealed class HotcakesProduct
    {
        public string Bvin { get; init; } = string.Empty;

        public string Sku { get; init; } = string.Empty;

        public string ProductName { get; init; } = string.Empty;

        public decimal ListPrice { get; init; }

        public decimal SitePrice { get; init; }

        public string ShortDescription { get; init; } = string.Empty;

        public string LongDescription { get; init; } = string.Empty;

        public string UrlSlug { get; init; } = string.Empty;

        public bool IsSearchable { get; init; }

        public bool Featured { get; init; }

        public bool IsAvailableForSale { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; init; }
    }
}
