using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum LeaseStatus { Application, Screening, Approved, Rejected, Active, Renewal, Expired, Terminated }

    public class Lease : IValidatableObject
    {
        public int LeaseId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        // Null when the Property is standalone. Required when the Property has Units.
        [Display(Name = "Unit")]
        public int? UnitId { get; set; }

        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        public LeaseStatus Status { get; set; } = LeaseStatus.Application;

        [DataType(DataType.Date)]
        [Display(Name = "Application Date")]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Application Notes")]
        public string? ApplicationNotes { get; set; }

        [StringLength(500)]
        [Display(Name = "Screening Notes")]
        public string? ScreeningNotes { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        // Navigation properties
        public Property? Property { get; set; }
        public Unit? Unit { get; set; }
        public Tenant? Tenant { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Cross-property validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
                yield return new ValidationResult(
                    "End date must be after start date.",
                    new[] { nameof(EndDate) });

            if (Status == LeaseStatus.Application && StartDate.Date < DateTime.Today)
                yield return new ValidationResult(
                    "Start date cannot be in the past.",
                    new[] { nameof(StartDate) });

            // Multi-unit vs standalone rule (enforced at service/controller level where Property can be loaded).
            // The Lease itself cannot see Property.Units here because the graph may not be loaded,
            // so the final check lives in LeasesController.Create / Put.
        }
    }
}
