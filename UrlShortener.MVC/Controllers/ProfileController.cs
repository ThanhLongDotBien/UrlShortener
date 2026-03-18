using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShortener.Data;
using UrlShortener.Data.Entities;

namespace UrlShortener.MVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
                return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Challenge();

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
                return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Challenge();

            var hasher = new PasswordHasher<AppUser>();

            var currentCheck = hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, currentPassword);
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

            foreach (var history in histories)
            {
                var reused = hasher.VerifyHashedPassword(user, history.PasswordHash, newPassword);
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
            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}