using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Common.DTOs
{
    public class CreateShortUrlRequestDto
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}