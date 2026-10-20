using System.Net.Http.Headers;
using System.Text;

namespace RaceDayMVC.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;

            // TODO: Replace with your API's HTTPS port from launchSettings.json
            _http.BaseAddress = new Uri("https://localhost:5001/");
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

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            AttachToken();
            return await _http.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _http.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, object data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _http.PutAsync(url, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            AttachToken();
            return await _http.DeleteAsync(url);
        }
    }
}