using System.ComponentModel.DataAnnotations;

namespace UrlShortener.MVC.Models
{
    public class CreateShortUrlViewModel
    {
        [Required(ErrorMessage = "Please enter a URL.")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string OriginalUrl { get; set; } = string.Empty;
    }
}