using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Data.Entities
{
    public class PasswordHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AppUser? User { get; set; }
    }
}
