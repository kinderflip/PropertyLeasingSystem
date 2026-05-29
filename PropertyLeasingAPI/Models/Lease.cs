using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum LeaseStatus { Application, Screening, Approved, Rejected, Active, Renewal, Expired, Terminated }

    public class Lease
    {
        public int LeaseId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        // Null when the property is standalone. Required when the property has units.
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
        [EndDateAfterStartDate]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
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
    }

    // Custom validation attribute: ensures EndDate is after StartDate.
    public class EndDateAfterStartDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var lease = (Lease)context.ObjectInstance;
            if (value is DateTime end && end <= lease.StartDate)
                return new ValidationResult("End date must be after start date.");
            return ValidationResult.Success;
        }
    }
}
