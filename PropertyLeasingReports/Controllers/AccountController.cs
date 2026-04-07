using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PropertyLeasingReports.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PropertyAPI");
                var loginData = new { email, password };
                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result != null)
                    {
                        // Store JWT token in session
                        HttpContext.Session.SetString("JwtToken", result.Token);
                        HttpContext.Session.SetString("UserEmail", result.Email);
                        HttpContext.Session.SetString("FullName", result.FullName);

                        // Create cookie claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, result.Email),
                            new Claim(ClaimTypes.GivenName, result.FullName),
                        };

                        foreach (var role in result.Roles)
                            claims.Add(new Claim(ClaimTypes.Role, role));

                        var identity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity));

                        return RedirectToAction("Index", "Home");
                    }
                }

                ViewBag.Error = "Invalid email or password.";
                return View();
            }
            catch
            {
                ViewBag.Error = "Cannot connect to the API. Make sure the API is running.";
                return View();
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}