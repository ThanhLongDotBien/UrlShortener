using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data.Entities;

namespace UrlShortener.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ShortUrl> ShortUrls { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
    }
}