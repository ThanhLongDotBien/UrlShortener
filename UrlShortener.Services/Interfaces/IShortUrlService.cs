using System;
using System.Collections.Generic;
using System.Text;
using UrlShortener.Common.DTOs;

namespace UrlShortener.Services.Interfaces
{
    public interface IShortUrlService
    {
        Task<ShortUrlResponseDto> CreateShortUrlAsync(CreateShortUrlRequestDto request);
        Task<ShortUrlResponseDto?> GetByCodeAsync(string code);
        Task<string?> GetOriginalUrlAsync(string code);
    }
}