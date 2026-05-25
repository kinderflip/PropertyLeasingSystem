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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Properties()
        {
            var vm = new PropertiesReportViewModel();
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Properties");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                vm.Properties = JsonSerializer.Deserialize<List<PropertyReport>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
            }

            vm.TotalProperties = vm.Properties.Count;
            vm.StandaloneCount = vm.Properties.Count(p => p.IsStandalone);
            vm.MultiUnitCount = vm.Properties.Count(p => !p.IsStandalone);
            vm.TotalUnits = vm.Properties.Sum(p => p.Units.Count);
            vm.LeasedStandalone = vm.Properties.Count(p => p.IsStandalone && p.Status == 1);
            vm.LeasedUnits = vm.Properties.SelectMany(p => p.Units).Count(u => u.Status == 1);

            var denominator = vm.StandaloneCount + vm.TotalUnits;
            vm.OccupancyRate = denominator > 0
                ? (double)(vm.LeasedStandalone + vm.LeasedUnits) / denominator
                : 0.0;

            return View(vm);
        }

        public async Task<IActionResult> Maintenance()
        {
            var requests = new List<MaintenanceReport>();
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/MaintenanceRequests");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                requests = JsonSerializer.Deserialize<List<MaintenanceReport>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
            }
            return View(requests);
        }

        public async Task<IActionResult> Leases()
        {
            var leases = new List<LeaseReport>();
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Leases");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                leases = JsonSerializer.Deserialize<List<LeaseReport>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
            }
            return View(leases);
        }

        public async Task<IActionResult> Payments()
        {
            var payments = new List<PaymentReport>();
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Payments");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                payments = JsonSerializer.Deserialize<List<PaymentReport>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
            }
            return View(payments);
        }
    }
}
