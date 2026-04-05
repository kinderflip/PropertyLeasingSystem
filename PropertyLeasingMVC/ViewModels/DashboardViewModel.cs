using PropertyLeasingAPI.Models;

namespace PropertyLeasingMVC.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProperties { get; set; }
        public int AvailableProperties { get; set; }
        public int LeasedProperties { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveLeases { get; set; }
        public int PendingMaintenance { get; set; }
        public List<MaintenanceRequest> RecentRequests { get; set; } = new();
        public List<Lease> RecentLeases { get; set; } = new();
    }
}