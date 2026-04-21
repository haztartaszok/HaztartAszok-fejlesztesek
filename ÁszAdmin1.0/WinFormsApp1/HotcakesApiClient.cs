using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace WinFormsApp1
{
    internal sealed class HotcakesApiClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient httpClient;
        private readonly Uri apiBaseUri;
        private readonly string apiKey;

        public HotcakesApiClient(HotcakesSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            string baseUrl = settings.BaseUrl.Trim();

            if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
            {
                baseUrl += "/";
            }

            apiBaseUri = new Uri(new Uri(baseUrl, UriKind.Absolute), "DesktopModules/Hotcakes/API/rest/v1/");
            apiKey = settings.ApiKey.Trim();
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(45)
            };
        }

        public async Task<IReadOnlyList<HotcakesCategorySnapshot>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            HotcakesApiResponse<List<HotcakesCategorySnapshot>> response = await GetAsync<List<HotcakesCategorySnapshot>>(
                "categories/",
                null,
                cancellationToken);

            return response.Content ?? [];
        }

        public async Task<IReadOnlyList<HotcakesCategorySnapshot>> GetCategoriesForProductAsync(
            string productBvin,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productBvin);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["byproduct"] = productBvin.Trim()
            };

            HotcakesApiResponse<List<HotcakesCategorySnapshot>> response = await GetAsync<List<HotcakesCategorySnapshot>>(
                "categories/",
                queryParameters,
                cancellationToken);

            return response.Content ?? [];
        }

        public Task<HotcakesProductPage> GetProductsPageAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["page"] = pageNumber.ToString(CultureInfo.InvariantCulture),
                ["pagesize"] = pageSize.ToString(CultureInfo.InvariantCulture)
            };

            return GetContentAsync(
                "products/",
                queryParameters,
                static () => new HotcakesProductPage(),
                cancellationToken);
        }

        public async Task<IReadOnlyList<HotcakesProduct>> GetAllProductsAsync(
            int pageSize = 250,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            List<HotcakesProduct> products = [];

            for (int pageNumber = 1; pageNumber <= 500; pageNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HotcakesProductPage page = await GetProductsPageAsync(pageNumber, pageSize, cancellationToken);

                if (page.Products.Count == 0)
                {
                    break;
                }

                products.AddRange(page.Products);

                if (page.Products.Count < pageSize)
                {
                    break;
                }
            }

            return products;
        }

        public async Task<HotcakesProduct?> GetProductBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sku);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["bysku"] = sku.Trim()
            };

            HotcakesApiResponse<HotcakesProduct> response = await GetAsync<HotcakesProduct>(
                "products/lookup",
                queryParameters,
                cancellationToken,
                allowApiErrors: true);

            if (response.Errors.Count == 0)
            {
                return response.Content;
            }

            if (response.Errors.All(static error => string.Equals(error.Code, "NULL", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            throw BuildApiErrorException(
                "A Hotcakes API nem tudta lekerdezni a keresett termeket SKU alapjan.",
                BuildRequestUri("products/lookup", queryParameters),
                HttpStatusCode.OK,
                response.Errors);
        }

        public Task<HotcakesProduct> CreateProductAsync(
            HotcakesProduct product,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(product);

            return PostContentAsync(
                "products/",
                product,
                static () => new HotcakesProduct(),
                cancellationToken);
        }

        public Task<HotcakesProduct> UpdateProductAsync(
            HotcakesProduct product,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentException.ThrowIfNullOrWhiteSpace(product.Bvin);

            return PostContentAsync(
                $"products/{Uri.EscapeDataString(product.Bvin)}",
                product,
                static () => new HotcakesProduct(),
                cancellationToken);
        }

        public async Task<IReadOnlyList<HotcakesProductInventory>> GetProductInventoriesAsync(
            string productBvin,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productBvin);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["byproduct"] = productBvin.Trim()
            };

            HotcakesApiResponse<List<HotcakesProductInventory>> response = await GetAsync<List<HotcakesProductInventory>>(
                "productinventory/",
                queryParameters,
                cancellationToken);

            return response.Content ?? [];
        }

        public Task<HotcakesProductInventory> UpsertProductInventoryAsync(
            HotcakesProductInventory inventory,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inventory);

            string relativePath = string.IsNullOrWhiteSpace(inventory.Bvin)
                ? "productinventory/"
                : $"productinventory/{Uri.EscapeDataString(inventory.Bvin)}";

            return PostContentAsync(
                relativePath,
                inventory,
                static () => new HotcakesProductInventory(),
                cancellationToken);
        }

        public Task<HotcakesCategoryProductAssociation> CreateCategoryProductAssociationAsync(
            HotcakesCategoryProductAssociation association,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(association);

            return PostContentAsync(
                "categoryproductassociations/",
                association,
                static () => new HotcakesCategoryProductAssociation(),
                cancellationToken);
        }

        public Task<bool> UploadProductMainImageAsync(
            string productBvin,
            string fileName,
            byte[] fileContent,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productBvin);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(fileContent);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["filename"] = fileName.Trim()
            };

            return PostContentAsync(
                $"productmainimage/{Uri.EscapeDataString(productBvin.Trim())}",
                queryParameters,
                fileContent,
                static () => false,
                cancellationToken);
        }

        public Task<bool> UploadProductAdditionalImageAsync(
            string productBvin,
            string fileName,
            byte[] fileContent,
            string imageBvin = "",
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productBvin);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(fileContent);

            Dictionary<string, string?> queryParameters = new(StringComparer.Ordinal)
            {
                ["filename"] = fileName.Trim()
            };

            string relativePath = string.IsNullOrWhiteSpace(imageBvin)
                ? $"productimagesupload/{Uri.EscapeDataString(productBvin.Trim())}/"
                : $"productimagesupload/{Uri.EscapeDataString(productBvin.Trim())}/{Uri.EscapeDataString(imageBvin.Trim())}";

            return PostContentAsync(
                relativePath,
                queryParameters,
                fileContent,
                static () => false,
                cancellationToken);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private async Task<T> GetContentAsync<T>(
            string relativePath,
            IReadOnlyDictionary<string, string?>? queryParameters,
            Func<T> emptyFactory,
            CancellationToken cancellationToken)
        {
            HotcakesApiResponse<T> response = await GetAsync<T>(relativePath, queryParameters, cancellationToken);
            return response.Content ?? emptyFactory();
        }

        private async Task<TResponse> PostContentAsync<TRequest, TResponse>(
            string relativePath,
            TRequest payload,
            Func<TResponse> emptyFactory,
            CancellationToken cancellationToken)
        {
            return await PostContentAsync(
                relativePath,
                null,
                payload,
                emptyFactory,
                cancellationToken);
        }

        private async Task<TResponse> PostContentAsync<TRequest, TResponse>(
            string relativePath,
            IReadOnlyDictionary<string, string?>? queryParameters,
            TRequest payload,
            Func<TResponse> emptyFactory,
            CancellationToken cancellationToken)
        {
            HotcakesApiResponse<TResponse> response = await SendAsync<TRequest, TResponse>(
                HttpMethod.Post,
                relativePath,
                queryParameters,
                payload,
                cancellationToken);

            return response.Content ?? emptyFactory();
        }

        private Task<HotcakesApiResponse<T>> GetAsync<T>(
            string relativePath,
            IReadOnlyDictionary<string, string?>? queryParameters,
            CancellationToken cancellationToken,
            bool allowApiErrors = false)
        {
            return SendAsync<object?, T>(
                HttpMethod.Get,
                relativePath,
                queryParameters,
                null,
                cancellationToken,
                allowApiErrors);
        }

        private async Task<HotcakesApiResponse<TResponse>> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string relativePath,
            IReadOnlyDictionary<string, string?>? queryParameters,
            TRequest? payload,
            CancellationToken cancellationToken,
            bool allowApiErrors = false)
        {
            Uri requestUri = BuildRequestUri(relativePath, queryParameters);

            using HttpRequestMessage request = new(method, requestUri);

            if (payload is not null)
            {
                string json = SerializePayload(payload);
                request.Content = new StringContent(json, Encoding.UTF8, GetContentType(payload));
            }

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw BuildRequestException(requestUri, response.StatusCode, body);
            }

            HotcakesApiResponse<TResponse>? parsedResponse;

            try
            {
                parsedResponse = JsonSerializer.Deserialize<HotcakesApiResponse<TResponse>>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new HotcakesApiException(
                    $"A Hotcakes valasz nem olvashato be ({relativePath}).",
                    requestUri,
                    response.StatusCode,
                    [],
                    ex);
            }

            if (parsedResponse is null)
            {
                throw new HotcakesApiException(
                    $"A Hotcakes valasz ures vagy ismeretlen ({relativePath}).",
                    requestUri,
                    response.StatusCode,
                    []);
            }

            if (!allowApiErrors && parsedResponse.Errors.Count > 0)
            {
                throw BuildApiErrorException(
                    $"A Hotcakes API hibat adott vissza ({relativePath}).",
                    requestUri,
                    response.StatusCode,
                    parsedResponse.Errors);
            }

            return parsedResponse;
        }

        private Uri BuildRequestUri(string relativePath, IReadOnlyDictionary<string, string?>? queryParameters)
        {
            Uri requestUri = new(apiBaseUri, relativePath);
            StringBuilder queryBuilder = new();

            AppendQueryParameter(queryBuilder, "key", apiKey);

            if (queryParameters is not null)
            {
                foreach ((string key, string? value) in queryParameters)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        AppendQueryParameter(queryBuilder, key, value);
                    }
                }
            }

            UriBuilder uriBuilder = new(requestUri)
            {
                Query = queryBuilder.ToString()
            };

            return uriBuilder.Uri;
        }

        private static void AppendQueryParameter(StringBuilder builder, string key, string value)
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
        }

        private static HotcakesApiException BuildRequestException(Uri requestUri, HttpStatusCode statusCode, string body)
        {
            try
            {
                HotcakesApiResponse<JsonElement>? errorResponse = JsonSerializer.Deserialize<HotcakesApiResponse<JsonElement>>(body, JsonOptions);

                if (errorResponse is not null && errorResponse.Errors.Count > 0)
                {
                    return BuildApiErrorException(
                        $"A Hotcakes keres nem sikerult ({(int)statusCode}).",
                        requestUri,
                        statusCode,
                        errorResponse.Errors);
                }
            }
            catch (JsonException)
            {
            }

            return BuildApiErrorException(
                BuildFallbackErrorMessage(statusCode, body),
                requestUri,
                statusCode,
                []);
        }

        private static string SerializePayload<TRequest>(TRequest payload)
        {
            if (payload is byte[] bytes)
            {
                int[] numericBytes = bytes.Select(static value => (int)value).ToArray();
                return JsonSerializer.Serialize(numericBytes, JsonOptions);
            }

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        private static string GetContentType<TRequest>(TRequest payload)
        {
            return payload is byte[]
                ? "application/x-www-form-urlencoded"
                : "application/json";
        }

        private static string BuildFallbackErrorMessage(HttpStatusCode statusCode, string body)
        {
            string message = $"A Hotcakes keres nem sikerult ({(int)statusCode}).";
            string trimmedBody = body.Trim();

            if (string.IsNullOrWhiteSpace(trimmedBody))
            {
                return message;
            }

            string singleLineBody = string.Join(" ", trimmedBody
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(static line => line.Trim()));

            if (singleLineBody.Length > 240)
            {
                singleLineBody = singleLineBody[..240] + "...";
            }

            return $"{message} Valasz: {singleLineBody}";
        }

        private static HotcakesApiException BuildApiErrorException(
            string message,
            Uri requestUri,
            HttpStatusCode? statusCode,
            IReadOnlyList<HotcakesApiError> errors)
        {
            string details = errors.Count == 0
                ? message
                : $"{message} {string.Join(" | ", errors.Select(static error => error.ToString()))}";

            return new HotcakesApiException(details, requestUri, statusCode, errors);
        }
    }

    internal sealed class HotcakesApiException : Exception
    {
        public HotcakesApiException(
            string message,
            Uri requestUri,
            HttpStatusCode? statusCode,
            IReadOnlyList<HotcakesApiError> errors,
            Exception? innerException = null)
            : base(message, innerException)
        {
            RequestUri = requestUri;
            StatusCode = statusCode;
            Errors = errors;
        }

        public IReadOnlyList<HotcakesApiError> Errors { get; }

        public Uri RequestUri { get; }

        public HttpStatusCode? StatusCode { get; }
    }
}
