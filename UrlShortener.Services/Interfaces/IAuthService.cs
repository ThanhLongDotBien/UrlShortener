using System;
using System.Collections.Generic;
using System.Text;

using UrlShortener.Common.DTOs;

namespace UrlShortener.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> Register(RegisterRequestDto request);
        Task<string> Login(LoginRequestDto request);
        Task Logout();
    }
}