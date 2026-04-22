using PropertyLeasingAPI.Models;

namespace PropertyLeasingMVC.ViewModels
{
    public class DashboardViewModel
    {
        // "Manager" = PropertyManager role; see HomeController.Index for the branching.
        public string Role { get; set; } = "Guest";

        // Global totals (Manager view)
        public int TotalProperties { get; set; }
        public int StandaloneProperties { get; set; }
        public int MultiUnitProperties { get; set; }
        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
        public int AvailableProperties { get; set; }   // includes "has any available unit"
        public int LeasedProperties { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveLeases { get; set; }
        public int PendingMaintenance { get; set; }

        // Staff view
        public int AssignedToMe { get; set; }
        public int InProgressByMe { get; set; }
        public int CompletedByMe { get; set; }

        // Tenant view
        public int MyActiveLeases { get; set; }
        public int MyOpenRequests { get; set; }
        public int MyOverduePayments { get; set; }

        public List<MaintenanceRequest> RecentRequests { get; set; } = new();
        public List<Lease> RecentLeases { get; set; } = new();
    }
}