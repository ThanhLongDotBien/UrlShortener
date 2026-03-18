using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortener.Common.DTOs;
using UrlShortener.MVC.Models;
using UrlShortener.MVC.Services;

namespace UrlShortener.MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;

        public HomeController(ApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View(new CreateShortUrlViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShortUrlViewModel model)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
                return Challenge();

            if (string.IsNullOrWhiteSpace(model.OriginalUrl))
            {
                ModelState.AddModelError("OriginalUrl", "Please enter a URL.");
                return View("Index", model);
            }

            model.OriginalUrl = model.OriginalUrl.Trim();

            if (!Uri.TryCreate(model.OriginalUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("OriginalUrl", "URL must start with http:// or https://");
                return View("Index", model);
            }

            if (!ModelState.IsValid)
                return View("Index", model);

            try
            {
                var request = new CreateShortUrlRequestDto
                {
                    OriginalUrl = model.OriginalUrl,
                    UserId = userId
                };

                var result = await _apiService.CreateShortUrl(request);
                var apiPublicBaseUrl = ((_configuration["ApiSettings:PublicBaseUrl"]
                    ?? _configuration["ApiSettings:InternalBaseUrl"]
                    ?? "https://localhost:7174")).TrimEnd('/');

                ViewBag.ShortUrl = result.ShortLink;
                ViewBag.QrCodeImagePath = string.IsNullOrWhiteSpace(result.QrCodeImagePath)
                    ? null
                    : $"{apiPublicBaseUrl}{result.QrCodeImagePath}";

                ModelState.Clear();
                return View("Index", new CreateShortUrlViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to create short URL. Please check the link and try again.";

                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    var lowerMessage = ex.Message.ToLowerInvariant();

                    if (lowerMessage.Contains("invalid url"))
                        ViewBag.Error = "Invalid URL. Please enter a correct link starting with http:// or https://";
                    else if (lowerMessage.Contains("original url cannot be empty"))
                        ViewBag.Error = "Please enter a URL before shortening.";
                    else if (lowerMessage.Contains("rate limit") || lowerMessage.Contains("429"))
                        ViewBag.Error = "You are sending requests too quickly. Please wait a moment and try again.";
                }

                return View("Index", model);
            }
        }
    }
}