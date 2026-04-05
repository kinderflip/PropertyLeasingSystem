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

        [Range(0, 20)]
        public int Bedrooms { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Available;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}