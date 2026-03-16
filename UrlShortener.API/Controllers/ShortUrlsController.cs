using Microsoft.AspNetCore.Mvc;
using UrlShortener.Common.DTOs;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlsController : ControllerBase
    {
        private readonly IShortUrlService _shortUrlService;

        public ShortUrlsController(IShortUrlService shortUrlService)
        {
            _shortUrlService = shortUrlService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShortUrlRequestDto request)
        {
            try
            {
                var result = await _shortUrlService.CreateShortUrlAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("/api/shorturls/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _shortUrlService.GetByCodeAsync(code);

            if (result == null)
                return NotFound(new { message = "Short URL not found." });

            return Ok(result);
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var originalUrl = await _shortUrlService.GetOriginalUrlAsync(code);

            if (originalUrl == null)
                return NotFound("Short URL not found.");

            return Redirect(originalUrl);
        }
    }
}