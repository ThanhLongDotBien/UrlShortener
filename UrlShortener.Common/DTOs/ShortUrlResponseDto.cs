using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Common.DTOs
{
    public class ShortUrlResponseDto
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ShortLink { get; set; } = string.Empty;
        public string? QrCodeImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }
        public int UserId { get; set; }
    }
}