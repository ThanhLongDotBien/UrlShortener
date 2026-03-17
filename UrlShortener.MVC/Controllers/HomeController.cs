using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Common.DTOs;
using UrlShortener.Data;
using UrlShortener.MVC.Models;
using UrlShortener.MVC.Services;

namespace UrlShortener.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly AppDbContext _context;

        public HomeController(ApiService apiService, AppDbContext context)
        {
            _apiService = apiService;
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("user") == null)
                return RedirectToAction("Login", "Account");

            return View(new CreateShortUrlViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShortUrlViewModel model)
        {
            var email = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

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

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var request = new CreateShortUrlRequestDto
                {
                    OriginalUrl = model.OriginalUrl,
                    UserId = user.Id
                };

                var result = await _apiService.CreateShortUrl(request);

                ViewBag.ShortUrl = result.ShortLink;
                ViewBag.QrCodeImagePath = $"https://localhost:7174{result.QrCodeImagePath}";

                ModelState.Clear();
                return View("Index", new CreateShortUrlViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to create short URL. Please check the link and try again.";

                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    var lowerMessage = ex.Message.ToLower();

                    if (lowerMessage.Contains("invalid url"))
                    {
                        ViewBag.Error = "Invalid URL. Please enter a correct link starting with http:// or https://";
                    }
                    else if (lowerMessage.Contains("original url cannot be empty"))
                    {
                        ViewBag.Error = "Please enter a URL before shortening.";
                    }
                }

                return View("Index", model);
            }
        }
    }
}