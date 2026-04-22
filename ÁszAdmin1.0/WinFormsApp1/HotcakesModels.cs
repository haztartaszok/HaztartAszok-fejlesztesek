using System.Globalization;
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

        public string ImageFileSmall { get; set; } = string.Empty;

        public string ImageFileSmallAlternateText { get; set; } = string.Empty;

        public string ImageFileMedium { get; set; } = string.Empty;

        public string ImageFileMediumAlternateText { get; set; } = string.Empty;

        public bool IsSearchable { get; set; } = true;

        public bool Featured { get; set; }

        public bool IsAvailableForSale { get; set; } = true;

        public bool TaxExempt { get; set; }

        public bool? AllowReviews { get; set; }

        public long StoreId { get; set; }

        public List<HotcakesCustomProperty> CustomProperties { get; set; } = [];

        public int Status { get; set; } = HotcakesProductStatuses.Active;

        public int InventoryMode { get; set; } = HotcakesInventoryModes.AlwayInStock;

        [JsonConverter(typeof(HotcakesDateTimeConverter))]
        public DateTime CreationDateUtc { get; set; } = DateTime.UtcNow;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    internal sealed class HotcakesCustomProperty
    {
        public string DeveloperId { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    internal sealed class HotcakesProductInventory
    {
        public string Bvin { get; set; } = string.Empty;

        [JsonConverter(typeof(HotcakesDateTimeConverter))]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

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

    internal sealed class HotcakesDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? rawValue = reader.GetString();

                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    return default;
                }

                if (TryParseMicrosoftDate(rawValue, out DateTime microsoftDate))
                {
                    return microsoftDate;
                }

                if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedDate) ||
                    DateTime.TryParse(rawValue, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out parsedDate))
                {
                    return parsedDate;
                }
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long unixMilliseconds))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
            }

            throw new JsonException("A datum formatuma nem tamogatott.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            DateTime outputValue = value == default ? DateTime.UtcNow : value.ToUniversalTime();
            writer.WriteStringValue(outputValue.ToString("O", CultureInfo.InvariantCulture));
        }

        private static bool TryParseMicrosoftDate(string rawValue, out DateTime value)
        {
            const string prefix = "/Date(";
            const string suffix = ")/";

            if (rawValue.StartsWith(prefix, StringComparison.Ordinal) &&
                rawValue.EndsWith(suffix, StringComparison.Ordinal))
            {
                string ticksPart = rawValue.Substring(prefix.Length, rawValue.Length - prefix.Length - suffix.Length);
                int signIndex = ticksPart.IndexOfAny(['+', '-']);

                if (signIndex >= 0)
                {
                    ticksPart = ticksPart[..signIndex];
                }

                if (long.TryParse(ticksPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixMilliseconds))
                {
                    value = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
