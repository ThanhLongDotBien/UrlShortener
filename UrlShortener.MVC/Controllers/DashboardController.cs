using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;

namespace UrlShortener.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var links = await _context.ShortUrls
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(links);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var email = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var link = await _context.ShortUrls
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            if (link != null)
            {
                _context.ShortUrls.Remove(link);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}