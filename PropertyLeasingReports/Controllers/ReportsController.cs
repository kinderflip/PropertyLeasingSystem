using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using PropertyLeasingReports.Models;

namespace PropertyLeasingReports.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("PropertyAPI");
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // GET: /Reports/Properties
        public async Task<IActionResult> Properties()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Properties");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var properties = JsonSerializer.Deserialize<List<PropertyReport>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(properties ?? new List<PropertyReport>());
            }

            return View(new List<PropertyReport>());
        }

        // GET: /Reports/Maintenance
        public async Task<IActionResult> Maintenance()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/MaintenanceRequests");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var requests = JsonSerializer.Deserialize<List<MaintenanceReport>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(requests ?? new List<MaintenanceReport>());
            }

            return View(new List<MaintenanceReport>());
        }

        // GET: /Reports/Leases
        public async Task<IActionResult> Leases()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Leases");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var leases = JsonSerializer.Deserialize<List<LeaseReport>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(leases ?? new List<LeaseReport>());
            }

            return View(new List<LeaseReport>());
        }
    }
}