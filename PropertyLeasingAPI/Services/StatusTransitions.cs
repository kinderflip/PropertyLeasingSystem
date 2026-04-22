using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Services
{
    // B9: Single source of truth for lease + maintenance lifecycle transitions.
    // Used by both the API controllers and the MVC controllers so the rules
    // cannot drift between surfaces.
    public static class StatusTransitions
    {
        public static bool IsValidLeaseTransition(LeaseStatus from, LeaseStatus to) =>
            (from, to) switch
            {
                (LeaseStatus.Application, LeaseStatus.Screening)  => true,
                (LeaseStatus.Screening,   LeaseStatus.Approved)   => true,
                (LeaseStatus.Screening,   LeaseStatus.Rejected)   => true,
                (LeaseStatus.Approved,    LeaseStatus.Active)     => true,
                (LeaseStatus.Active,      LeaseStatus.Renewal)    => true,
                (LeaseStatus.Active,      LeaseStatus.Terminated) => true,
                (LeaseStatus.Renewal,     LeaseStatus.Active)     => true,
                (LeaseStatus.Renewal,     LeaseStatus.Terminated) => true,
                (LeaseStatus.Active,      LeaseStatus.Expired)    => true,
                _ => false
            };

        public static bool IsValidMaintenanceTransition(MaintenanceStatus from, MaintenanceStatus to) =>
            (from, to) switch
            {
                (MaintenanceStatus.Submitted,  MaintenanceStatus.Assigned)   => true,
                (MaintenanceStatus.Assigned,   MaintenanceStatus.InProgress) => true,
                (MaintenanceStatus.InProgress, MaintenanceStatus.Resolved)   => true,
                (MaintenanceStatus.Resolved,   MaintenanceStatus.Closed)     => true,
                // Shortcut paths the existing MVC flow allows
                (MaintenanceStatus.Submitted,  MaintenanceStatus.InProgress) => true,
                (MaintenanceStatus.InProgress, MaintenanceStatus.Closed)     => true,
                _ => false
            };
    }
}
