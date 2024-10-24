using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PocketBaseCore
{
    public class PocketBaseClient : IPocketBaseClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger _logger;

        public JsonSerializerOptions JsonOptions => _jsonOptions;

        public PocketBaseClient(string baseUrl, ILogger logger = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            _logger = logger ?? NullLogger.Instance;
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(baseUrl) };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateTimeConverter() }
            };
        }

        public string AuthToken { get; private set; }

        public async Task<AuthResponse<PocketBaseUser>> AuthenticateAsync(string identity, string password, string expand = null, string fields = null)
        {
            return await AuthenticateAsync<PocketBaseUser>(identity, password, expand, fields);
        }

        public async Task<AuthResponse<T>> AuthenticateAsync<T>(string identity, string password, string expand = null, string fields = null) where T : PocketBaseUser
        {
            var content = new { identity, password };
            var uri = BuildUri("/api/collections/users/auth-with-password", expand, fields);

            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
            });

            var authResponse = JsonSerializer.Deserialize<AuthResponse<T>>(await response.Content.ReadAsStringAsync(), _jsonOptions);
            AuthToken = authResponse.Token;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
            return authResponse;
        }

        public async Task<T> CreateRecordAsync<T>(string collection, object data, string expand = null, string fields = null) where T : class
        {
            var uri = BuildUri($"/api/collections/{collection}/records", expand, fields);

            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
            });

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public async Task<T> GetRecordAsync<T>(string collection, string id, string expand = null, string fields = null) where T : class
        {
            var uri = BuildUri($"/api/collections/{collection}/records/{id}", expand, fields);

            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public async Task<RecordList<T>> GetRecordsAsync<T>(
            string collection,
            int page = 1,
            int perPage = 100,
            bool skipTotal = false,
            string filter = null,
            string sort = null,
            string expand = null,
            string fields = null) where T : class
        {
            var queryParams = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"perPage", perPage.ToString()},
                {"skipTotal", skipTotal.ToString().ToLower()},
                {"sort", sort},
                {"filter", Uri.EscapeDataString(filter ?? string.Empty) },
                {"expand", expand},
                {"fields", fields}
            };

            var uri = BuildUri($"/api/collections/{collection}/records", queryParams);

            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
            return JsonSerializer.Deserialize<RecordList<T>>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public async Task<T> UpdateRecordAsync<T>(string collection, string id, object data, string expand = null, string fields = null) where T : class
        {
            var uri = BuildUri($"/api/collections/{collection}/records/{id}", expand, fields);

            var response = await SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
            });

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public async Task DeleteRecordAsync(string collection, string id)
        {
            await SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/collections/{collection}/records/{id}"));
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            _logger.LogDebug("Sending request to {Url}", request.RequestUri);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error response: {Error}", errorContent);
                throw new PocketSharpException(errorContent, response.StatusCode);
            }

            return response;
        }

        private string BuildUri(string baseUri, string expand, string fields)
        {
            var queryParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(expand))
                queryParams.Add("expand", expand);

            if (!string.IsNullOrEmpty(fields))
                queryParams.Add("fields", fields);

            return BuildUri(baseUri, queryParams);
        }

        private string BuildUri(string baseUri, Dictionary<string, string> queryParams)
        {
            if (queryParams.Any())
            {
                baseUri += "?" + string.Join("&", queryParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)).Select(kvp =>$"{kvp.Key}={kvp.Value}" ));
            }

            return baseUri;
        }
    }
}