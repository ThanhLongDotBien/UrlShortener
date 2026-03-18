using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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