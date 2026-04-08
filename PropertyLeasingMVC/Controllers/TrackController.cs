using Microsoft.AspNetCore.Mvc;

namespace PropertyLeasingMVC.Controllers
{
    public class TrackController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TrackController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Track
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Track
        [HttpPost]
        public async Task<IActionResult> Index(int ticketId, string phone)
        {
            if (ticketId <= 0)
            {
                ViewBag.Error = "Please enter a valid ticket number.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                ViewBag.Error = "Please enter your registered phone number.";
                return View();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PropertyAPI");
                var response = await client.GetAsync(
                    $"api/MaintenanceRequests/track?ticketId={ticketId}&phone={Uri.EscapeDataString(phone)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<TrackingResult>();
                    return View("Result", result);
                }
                else
                {
                    ViewBag.Error = "No matching request found. Please verify your ticket number and phone number.";
                    return View();
                }
            }
            catch
            {
                ViewBag.Error = "Unable to connect to the tracking service. Please try again later.";
                return View();
            }
        }
    }

    public class TrackingResult
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string DateSubmitted { get; set; } = string.Empty;
        public string DateAssigned { get; set; } = string.Empty;
        public string DateResolved { get; set; } = string.Empty;
        public string PropertyAddress { get; set; } = string.Empty;
        public string PropertyCity { get; set; } = string.Empty;
    }
}
