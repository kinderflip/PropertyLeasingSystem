using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum LeaseStatus { Active, Expired, Terminated }

    public class Lease : IValidatableObject
    {
        public int LeaseId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

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

        public LeaseStatus Status { get; set; } = LeaseStatus.Active;

        // Navigation properties
        public Property? Property { get; set; }
        public Tenant? Tenant { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Cross-property validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
                yield return new ValidationResult(
                    "End date must be after start date.",
                    new[] { nameof(EndDate) });

            if (StartDate < DateTime.Today)
                yield return new ValidationResult(
                    "Start date cannot be in the past.",
                    new[] { nameof(StartDate) });
        }
    }
}