using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using UrlShortener.Common.DTOs;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new RegisterRequestDto
            {
                Email = model.Email,
                Password = model.Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            ViewBag.Error = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new LoginRequestDto
            {
                Email = model.Email,
                Password = model.Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            HttpContext.Session.SetString("user", model.Email);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}