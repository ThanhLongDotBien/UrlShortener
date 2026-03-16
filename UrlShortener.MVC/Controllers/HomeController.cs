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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShortUrlViewModel model)
        {
            var email = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var request = new CreateShortUrlRequestDto
            {
                OriginalUrl = model.OriginalUrl,
                UserId = user.Id
            };

            var result = await _apiService.CreateShortUrl(request);

            ViewBag.ShortUrl = result.ShortLink;
            ViewBag.QrCodeImagePath = $"https://localhost:7174{result.QrCodeImagePath}";

            return View("Index");
        }
    }
}