using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Common.DTOs
{
    public class ChangePasswordRequestDto
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}