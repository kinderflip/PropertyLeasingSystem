using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum UnitType { Studio, OneBedroom, TwoBedroom, ThreeBedroom, Office, Shop, Other }
    public enum UnitStatus { Available, Leased, UnderMaintenance }

    public class Unit
    {
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Unit Number")]
        public required string UnitNumber { get; set; }

        [Display(Name = "Unit Type")]
        public UnitType UnitType { get; set; }

        [StringLength(500)]
        public string? Amenities { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        [Range(1, 100000)]
        [Display(Name = "Size (sqm)")]
        public decimal SizeSqm { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        public UnitStatus Status { get; set; } = UnitStatus.Available;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public Property? Property { get; set; }
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
