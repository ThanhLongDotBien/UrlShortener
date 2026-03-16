using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;
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
        private static readonly Random _random = new();

        public ShortUrlService(
            AppDbContext context,
            IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
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

            return new ShortUrlResponseDto
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
        }

        public async Task<ShortUrlResponseDto?> GetByCodeAsync(string code)
        {
            var entity = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code);

            if (entity == null)
                return null;

            return new ShortUrlResponseDto
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
        }

        public async Task<string?> GetOriginalUrlAsync(string code)
        {
            var entity = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code);

            if (entity == null)
                return null;

            entity.ClickCount++;
            await _context.SaveChangesAsync();

            return entity.OriginalUrl;
        }

        private string GenerateCodeFromUrl(string url)
        {
            var uri = new Uri(url);
            var host = uri.Host.Replace("www.", "");
            var name = host.Split('.')[0].ToLower();

            if (name.Length > 8)
                name = name.Substring(0, 8);

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