using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Common.DTOs
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
