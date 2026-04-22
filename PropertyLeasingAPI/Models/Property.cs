using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum PropertyType { Apartment, Villa, Shop, Office }
    public enum PropertyStatus { Available, Leased, UnderMaintenance }

    public class Property
    {
        public int PropertyId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public required string Address { get; set; }

        [Required]
        [StringLength(100)]
        public required string City { get; set; }

        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        // The following three fields are only meaningful when the Property is STANDALONE (no Units).
        // When the Property has Units, these stay NULL and per-unit values are used instead.

        [Range(0, 20)]
        [Display(Name = "Bedrooms (standalone only)")]
        public int? Bedrooms { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Monthly Rent (standalone only)")]
        public decimal? MonthlyRent { get; set; }

        [Display(Name = "Status (standalone only)")]
        public PropertyStatus? Status { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

        // Convenience: a Property is "standalone" when it has no child Units.
        [NotMapped]
        public bool IsStandalone => Units == null || Units.Count == 0;
    }
}
