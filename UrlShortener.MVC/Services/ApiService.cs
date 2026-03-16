using System.Net.Http.Json;
using UrlShortener.Common.DTOs;

namespace UrlShortener.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ShortUrlResponseDto> CreateShortUrl(CreateShortUrlRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/shorturls", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {(int)response.StatusCode} - {errorText}");
            }

            var result = await response.Content.ReadFromJsonAsync<ShortUrlResponseDto>();

            if (result == null)
                throw new Exception("API returned empty response.");

            return result;
        }
    }
}