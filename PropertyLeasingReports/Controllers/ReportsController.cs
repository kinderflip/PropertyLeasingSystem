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
        private readonly ILogger<ReportsController> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ReportsController(
            IHttpClientFactory httpClientFactory,
            ILogger<ReportsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
            var vm = new PropertiesReportViewModel();
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Properties");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    vm.Properties = JsonSerializer.Deserialize<List<PropertyReport>>(json, _jsonOptions) ?? new();
                }
                else
                {
                    _logger.LogWarning("GET api/Properties returned {StatusCode}", response.StatusCode);
                    ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load properties report");
                ViewBag.Error = "Unable to connect to the API. Please try again later.";
            }

            // Aggregations (Plan L7): multi-unit-aware occupancy rate
            vm.TotalProperties = vm.Properties.Count;
            vm.StandaloneCount = vm.Properties.Count(p => p.IsStandalone);
            vm.MultiUnitCount = vm.Properties.Count(p => !p.IsStandalone);
            vm.TotalUnits = vm.Properties.Sum(p => p.Units.Count);
            // PropertyStatus.Leased == 1
            vm.LeasedStandalone = vm.Properties.Count(p => p.IsStandalone && p.Status == 1);
            // UnitStatus.Leased == 1
            vm.LeasedUnits = vm.Properties.SelectMany(p => p.Units).Count(u => u.Status == 1);

            var denominator = vm.StandaloneCount + vm.TotalUnits;
            vm.OccupancyRate = denominator > 0
                ? (double)(vm.LeasedStandalone + vm.LeasedUnits) / denominator
                : 0.0;

            return View(vm);
        }

        // GET: /Reports/Maintenance
        public async Task<IActionResult> Maintenance()
        {
            var requests = new List<MaintenanceReport>();
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/MaintenanceRequests");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    requests = JsonSerializer.Deserialize<List<MaintenanceReport>>(json, _jsonOptions) ?? new();
                }
                else
                {
                    _logger.LogWarning("GET api/MaintenanceRequests returned {StatusCode}", response.StatusCode);
                    ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load maintenance report");
                ViewBag.Error = "Unable to connect to the API. Please try again later.";
            }
            return View(requests);
        }

        // GET: /Reports/Leases
        public async Task<IActionResult> Leases()
        {
            var leases = new List<LeaseReport>();
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Leases");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    leases = JsonSerializer.Deserialize<List<LeaseReport>>(json, _jsonOptions) ?? new();
                }
                else
                {
                    _logger.LogWarning("GET api/Leases returned {StatusCode}", response.StatusCode);
                    ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load leases report");
                ViewBag.Error = "Unable to connect to the API. Please try again later.";
            }
            return View(leases);
        }

        // GET: /Reports/Payments
        public async Task<IActionResult> Payments()
        {
            var payments = new List<PaymentReport>();
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Payments");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    payments = JsonSerializer.Deserialize<List<PaymentReport>>(json, _jsonOptions) ?? new();
                }
                else
                {
                    _logger.LogWarning("GET api/Payments returned {StatusCode}", response.StatusCode);
                    ViewBag.Error = $"API returned {(int)response.StatusCode}. Please log in again if your session expired.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load payments report");
                ViewBag.Error = "Unable to connect to the API. Please try again later.";
            }
            return View(payments);
        }
    }
}
