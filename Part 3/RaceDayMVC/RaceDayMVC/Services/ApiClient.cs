using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDayMVC.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(
            HttpClient http,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            ILogger<ApiClient> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            var baseUrl = config["ApiSettings:BaseUrl"]
                ?? config["ApiBaseUrl"]
                ?? "https://localhost:7065/";

            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            _http.BaseAddress = new Uri(baseUrl);
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        }

        private HttpRequestMessage AttachTokenToRequest(HttpRequestMessage request)
        {
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return request;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AttachTokenToRequest(request);

            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Url} returned {StatusCode}: {Content}", url, response.StatusCode, content);
            }

            response.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            AttachTokenToRequest(request);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("POST {Url} returned {StatusCode}: {Error}", url, response.StatusCode, err);
                response.Content = new StringContent(err, Encoding.UTF8, "application/json");
            }
            return response;
        }

        public async Task<HttpResponseMessage> PutAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
            AttachTokenToRequest(request);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("PUT {Url} returned {StatusCode}: {Error}", url, response.StatusCode, err);
                response.Content = new StringContent(err, Encoding.UTF8, "application/json");
            }
            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            AttachTokenToRequest(request);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("DELETE {Url} returned {StatusCode}: {Error}", url, response.StatusCode, err);
                response.Content = new StringContent(err, Encoding.UTF8, "application/json");
            }
            return response;
        }

        public async Task<HttpResponseMessage> PostMultipartAsync(string url, IFormFile file, string fieldName = "file")
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");

            content.Add(fileContent, fieldName, file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            AttachTokenToRequest(request);

            return await _http.SendAsync(request);
        }
    }
}