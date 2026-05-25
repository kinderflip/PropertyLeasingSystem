using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum PropertyType { Apartment, Villa, Shop, Office }
    public enum PropertyStatus { Available, Leased, UnderMaintenance }

    // a property can be standalone (like a villa) or a building with many units
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

        // these 3 below only matter when the property is standalone
        // if it has units we leave them empty and use the unit values instead

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

        // relationship navs
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
