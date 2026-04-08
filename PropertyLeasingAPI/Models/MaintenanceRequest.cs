using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum MaintenanceCategory { Plumbing, Electrical, HVAC, General }
    public enum MaintenanceStatus { Submitted, Assigned, InProgress, Resolved, Closed }
    public enum MaintenancePriority { Low, Medium, High, Urgent }

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

        public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;

        [Display(Name = "Assigned Staff")]
        public string? AssignedStaffId { get; set; }

        [Display(Name = "Staff Notes")]
        [StringLength(500)]
        public string? StaffNotes { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [Display(Name = "Date Assigned")]
        public DateTime? DateAssigned { get; set; }

        [Display(Name = "Date Resolved")]
        public DateTime? DateResolved { get; set; }

        // Navigation properties
        public Property? Property { get; set; }
        public Tenant? Tenant { get; set; }
        public ApplicationUser? AssignedStaff { get; set; }
    }
}