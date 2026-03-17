using FluentValidation;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Validators
{
    public class CreateShortUrlViewModelValidator : AbstractValidator<CreateShortUrlViewModel>
    {
        public CreateShortUrlViewModelValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Please enter a URL.")
                .MaximumLength(2048).WithMessage("URL must not exceed 2048 characters.")
                .Must(BeAValidUrl).WithMessage("URL must start with http:// or https://");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.Trim();

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
