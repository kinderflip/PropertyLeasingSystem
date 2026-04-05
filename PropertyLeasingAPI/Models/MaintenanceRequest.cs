using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum MaintenanceCategory { Plumbing, Electrical, HVAC, General }
    public enum MaintenanceStatus { Pending, InProgress, Completed }

    public class MaintenanceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        public MaintenanceCategory Category { get; set; }

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [Display(Name = "Date Resolved")]
        public DateTime? DateResolved { get; set; }

        // Navigation properties
        public Property? Property { get; set; }
        public Tenant? Tenant { get; set; }
    }
}