using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EZTM.Common
{
    public class Grok
    {
        private string ApiKey { get; set; } = "xai-xtHAwSg0GwefMs3PD9Hg9lnToYIujszfWM0cP8cWZP4HvD3JJiENNapRpXeY5FNHsqGFoejRsAg80c3S"; // API key for authentication, must be set in constructor
        private string ApiUrl { get; set; } = "https://api.x.ai/v1"; // Default API URL, can be overridden
        private static readonly HttpClient _httpClient = new HttpClient();

        public Grok()
        {
            // Default constructor initializes with an empty API key.
            // The ApiKey must be set before making any API calls.
        }

        // Public constructor for Grok class that accepts the apiKey.
        public Grok(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// Sends a GET request to the specified Grok API endpoint with optional query parameters.
        /// </summary>
        public async Task<string> GetAsync(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var url = ApiUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += "?" + query;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends a POST request to the specified Grok API endpoint with a JSON payload.
        /// </summary>
        public async Task<string> PostAsync(string endpoint, object payload)
        {
            var url = ApiUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
