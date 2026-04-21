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
                Timeout = TimeSpan.FromSeconds(30)
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

        private async Task<HotcakesApiResponse<T>> GetAsync<T>(
            string relativePath,
            IReadOnlyDictionary<string, string?>? queryParameters,
            CancellationToken cancellationToken)
        {
            Uri requestUri = BuildRequestUri(relativePath, queryParameters);

            using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw BuildRequestException(requestUri, response.StatusCode, body);
            }

            HotcakesApiResponse<T>? parsedResponse;

            try
            {
                parsedResponse = JsonSerializer.Deserialize<HotcakesApiResponse<T>>(body, JsonOptions);
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

            if (parsedResponse.Errors.Count > 0)
            {
                throw new HotcakesApiException(
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
                    return new HotcakesApiException(
                        $"A Hotcakes keres nem sikerult ({(int)statusCode}).",
                        requestUri,
                        statusCode,
                        errorResponse.Errors);
                }
            }
            catch (JsonException)
            {
            }

            return new HotcakesApiException(
                $"A Hotcakes keres nem sikerult ({(int)statusCode}).",
                requestUri,
                statusCode,
                []);
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
