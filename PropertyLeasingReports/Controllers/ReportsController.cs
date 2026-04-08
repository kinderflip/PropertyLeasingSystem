using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using PropertyLeasingReports.Models;

namespace PropertyLeasingReports.Controllers
{
    [Authorize(Roles = "PropertyManager")]
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

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

            var properties = new List<PropertyReport>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                properties = JsonSerializer.Deserialize<List<PropertyReport>>(json, _jsonOptions) ?? new();
            }

            return View(properties);
        }

        // GET: /Reports/Maintenance
        public async Task<IActionResult> Maintenance()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/MaintenanceRequests");

            var requests = new List<MaintenanceReport>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                requests = JsonSerializer.Deserialize<List<MaintenanceReport>>(json, _jsonOptions) ?? new();
            }

            return View(requests);
        }

        // GET: /Reports/Leases
        public async Task<IActionResult> Leases()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Leases");

            var leases = new List<LeaseReport>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                leases = JsonSerializer.Deserialize<List<LeaseReport>>(json, _jsonOptions) ?? new();
            }

            return View(leases);
        }

        // GET: /Reports/Payments
        public async Task<IActionResult> Payments()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Payments");

            var payments = new List<PaymentReport>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                payments = JsonSerializer.Deserialize<List<PaymentReport>>(json, _jsonOptions) ?? new();
            }

            return View(payments);
        }
    }
}
