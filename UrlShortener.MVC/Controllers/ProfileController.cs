using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Data.Entities;

namespace UrlShortener.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
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

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var email = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var email = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var hasher = new PasswordHasher<AppUser>();

            var currentCheck = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", currentPassword);
            if (currentCheck == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            var histories = await _context.PasswordHistories
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var h in histories)
            {
                var reused = hasher.VerifyHashedPassword(user, h.PasswordHash, newPassword);
                if (reused == PasswordVerificationResult.Success)
                {
                    ViewBag.Error = "Cannot reuse the last 5 passwords.";
                    return View();
                }
            }

            var newHash = hasher.HashPassword(user, newPassword);
            user.PasswordHash = newHash;

            _context.PasswordHistories.Add(new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = newHash,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}