using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var adminEmails = _configuration
                .GetSection("AdminSettings:Emails")
                .Get<string[]>() ?? Array.Empty<string>();

            if (!adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                return Forbid();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalLinks = await _context.ShortUrls.CountAsync(),
                TotalClicks = await _context.ShortUrls.SumAsync(x => x.ClickCount),
                NewUsersLast7Days = await _context.Users.CountAsync(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                TopLinks = await _context.ShortUrls
                    .OrderByDescending(x => x.ClickCount)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}