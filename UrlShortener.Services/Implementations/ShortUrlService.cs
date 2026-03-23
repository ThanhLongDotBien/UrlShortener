using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Text.Json;
using UrlShortener.Common.Configurations;
using UrlShortener.Common.DTOs;
using UrlShortener.Data;
using UrlShortener.Data.Entities;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.Services.Implementations
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _cache;
        private static readonly Random _random = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public ShortUrlService(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            IDistributedCache cache)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _cache = cache;
        }

        public async Task<ShortUrlResponseDto> CreateShortUrlAsync(CreateShortUrlRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.OriginalUrl))
                throw new ArgumentException("Original URL cannot be empty.");

            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Invalid URL format.");
            }

            var userExists = await _context.Users.AnyAsync(x => x.Id == request.UserId);
            if (!userExists)
                throw new ArgumentException("User not found.");

            string code;
            bool exists;

            do
            {
                code = GenerateCodeFromUrl(request.OriginalUrl);
                exists = await _context.ShortUrls.AnyAsync(x => x.Code == code);
            }
            while (exists);

            var baseUrl = (_appSettings.BaseUrl ?? string.Empty).TrimEnd('/');
            var shortLink = $"{baseUrl}/{code}";
            var qrPath = GenerateQrCodeImage(shortLink);

            var entity = new ShortUrl
            {
                OriginalUrl = request.OriginalUrl,
                Code = code,
                ShortLink = shortLink,
                QrCodeImagePath = qrPath,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0,
                UserId = request.UserId
            };

            _context.ShortUrls.Add(entity);
            await _context.SaveChangesAsync();

            var response = MapToDto(entity);
            await CacheShortUrlAsync(response);
            return response;
        }

        public async Task<ShortUrlResponseDto?> GetByCodeAsync(string code)
        {
            var cacheKey = GetShortUrlCacheKey(code);
            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                return JsonSerializer.Deserialize<ShortUrlResponseDto>(cachedJson);
            }

            var entity = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code);
            if (entity == null)
                return null;

            var dto = MapToDto(entity);
            await CacheShortUrlAsync(dto);
            return dto;
        }

        public async Task<string?> GetOriginalUrlAsync(string code)
        {
            var originalKey = GetOriginalUrlCacheKey(code);
            var dtoKey = GetShortUrlCacheKey(code);
            var cachedOriginal = await _cache.GetStringAsync(originalKey);

            if (!string.IsNullOrWhiteSpace(cachedOriginal))
            {
                var rows = await _context.ShortUrls
                    .Where(x => x.Code == code)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.ClickCount, x => x.ClickCount + 1));

                if (rows == 0)
                {
                    await _cache.RemoveAsync(originalKey);
                    await _cache.RemoveAsync(dtoKey);
                    return null;
                }

                var cachedDtoJson = await _cache.GetStringAsync(dtoKey);
                if (!string.IsNullOrWhiteSpace(cachedDtoJson))
                {
                    var cachedDto = JsonSerializer.Deserialize<ShortUrlResponseDto>(cachedDtoJson);
                    if (cachedDto != null)
                    {
                        cachedDto.ClickCount++;
                        await _cache.SetStringAsync(dtoKey, JsonSerializer.Serialize(cachedDto), BuildCacheOptions());
                    }
                }

                return cachedOriginal;
            }

            var entity = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code);
            if (entity == null)
                return null;

            entity.ClickCount++;
            await _context.SaveChangesAsync();

            var dto = MapToDto(entity);
            await CacheShortUrlAsync(dto);
            return entity.OriginalUrl;
        }

        private async Task CacheShortUrlAsync(ShortUrlResponseDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                await _cache.SetStringAsync(GetShortUrlCacheKey(dto.Code), json, BuildCacheOptions());
                await _cache.SetStringAsync(GetOriginalUrlCacheKey(dto.Code), dto.OriginalUrl, BuildCacheOptions());
            }
            catch
            {
                // bỏ qua lỗi cache để chức năng chính vẫn chạy
            }
        }

        private static string GetShortUrlCacheKey(string code) => $"shorturl:dto:{code}";
        private static string GetOriginalUrlCacheKey(string code) => $"shorturl:original:{code}";

        private static DistributedCacheEntryOptions BuildCacheOptions() => new()
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        };

        private static ShortUrlResponseDto MapToDto(ShortUrl entity) => new()
        {
            Id = entity.Id,
            OriginalUrl = entity.OriginalUrl,
            Code = entity.Code,
            ShortLink = entity.ShortLink,
            QrCodeImagePath = entity.QrCodeImagePath,
            CreatedAt = entity.CreatedAt,
            ClickCount = entity.ClickCount,
            UserId = entity.UserId
        };

        private string GenerateCodeFromUrl(string url)
        {
            var uri = new Uri(url);
            var host = uri.Host.Replace("www.", "");
            var name = host.Split('.')[0].ToLowerInvariant();

            if (name.Length > 8)
                name = name[..8];

            var suffix = _random.Next(10, 100);
            return $"{name}{suffix}";
        }

        private string GenerateQrCodeImage(string shortLink)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrcodes");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(folderPath, fileName);

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(shortLink, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(data);
            byte[] qrBytes = qrCode.GetGraphic(20);

            File.WriteAllBytes(filePath, qrBytes);

            return $"/qrcodes/{fileName}";
        }
    }
}