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

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<ApiClient> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _http.BaseAddress = new Uri("https://localhost:7065/");
        }
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
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
            var token = GetToken();
            AttachTokenToRequest(request);

            _logger.LogInformation($"Making GET request to: {url}");
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"Token attached: {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                _logger.LogWarning("No token found in session!");
            }

            var response = await _http.SendAsync(request);
            _logger.LogInformation($"GET {url} returned status: {response.StatusCode}");

            // Log response content for debugging
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response content: {content}");

            // Reset content for actual use
            response.Content = new StringContent(content, Encoding.UTF8, "application/json");

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            AttachTokenToRequest(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
            AttachTokenToRequest(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            AttachTokenToRequest(request);
            return await _http.SendAsync(request);
        }
        public async Task<HttpResponseMessage> PostMultipartAsync(string url, IFormFile file, string fieldName = "file")
        {
            AttachToken();

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");

            content.Add(fileContent, fieldName, file.FileName);

            return await _http.PostAsync(url, content);
        }
    }
}