using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PropertyLeasingAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // M1: comma-separated skill tags for MaintenanceStaff users.
        // Use the MaintenanceCategory enum names as canonical values: "Plumbing,Electrical".
        // Empty / null is fine for non-staff users.
        [StringLength(200)]
        [Display(Name = "Skills (comma-separated)")]
        public string? Skills { get; set; }

        // M10: whether a MaintenanceStaff user is currently available for new assignments.
        // Default true so existing rows don't need backfilling.
        [Display(Name = "Available for new assignments")]
        public bool IsAvailable { get; set; } = true;
    }
}
