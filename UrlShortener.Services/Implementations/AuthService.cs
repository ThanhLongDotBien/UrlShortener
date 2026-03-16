using Microsoft.AspNetCore.Identity;
using UrlShortener.Common.DTOs;
using UrlShortener.Data.Entities;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<string> Register(RegisterRequestDto request)
        {
            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return string.Join(",", result.Errors.Select(x => x.Description));

            return "Register success";
        }

        public async Task<string> Login(LoginRequestDto request)
        {
            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                true,
                false
            );

            if (!result.Succeeded)
                return "Login failed";

            return "Login success";
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            throw new NotImplementedException();
        }
    }
}