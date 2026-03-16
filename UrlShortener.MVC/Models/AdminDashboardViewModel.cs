using UrlShortener.Data.Entities;

namespace UrlShortener.MVC.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalLinks { get; set; }
        public int TotalClicks { get; set; }
        public int NewUsersLast7Days { get; set; }
        public List<ShortUrl> TopLinks { get; set; } = new();
    }
}