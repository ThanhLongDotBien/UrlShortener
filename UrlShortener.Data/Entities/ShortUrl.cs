using UrlShortener.Data.Entities;

namespace UrlShortener.Data.Entities
{
    public class ShortUrl
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ShortLink { get; set; } = string.Empty;
        public string? QrCodeImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ClickCount { get; set; } = 0;

        public int UserId { get; set; }
        public AppUser? User { get; set; }
    }
}