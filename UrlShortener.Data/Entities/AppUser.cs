using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Data.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}