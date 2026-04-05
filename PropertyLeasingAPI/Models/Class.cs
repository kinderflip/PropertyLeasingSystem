using System.ComponentModel.DataAnnotations;

namespace PropertyLeasingAPI.Models
{
    public class Tenant
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        public required string Phone { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "National ID")]
        public required string NationalId { get; set; }

        // Link to Identity user (optional - for login)
        public string? UserId { get; set; }

        // Navigation properties
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}