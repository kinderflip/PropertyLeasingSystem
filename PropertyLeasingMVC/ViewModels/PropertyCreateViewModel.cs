using System.ComponentModel.DataAnnotations;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingMVC.ViewModels
{
    /// <summary>
    /// Q4: dedicated input model for Property create/edit so the entity is never
    /// bound directly from a request payload (overposting protection).
    /// </summary>
    public class PropertyCreateViewModel
    {
        public int PropertyId { get; set; }

        [Required, StringLength(200)]
        public required string Address { get; set; }

        [Required, StringLength(100)]
        public required string City { get; set; }

        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        [Range(0, 20)]
        [Display(Name = "Bedrooms (standalone only)")]
        public int? Bedrooms { get; set; }

        [Range(0.01, 999999.99)]
        [Display(Name = "Monthly Rent (standalone only)")]
        public decimal? MonthlyRent { get; set; }

        [Display(Name = "Status (standalone only)")]
        public PropertyStatus? Status { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Property ToEntity() => new Property
        {
            PropertyId   = PropertyId,
            Address      = Address,
            City         = City,
            PropertyType = PropertyType,
            Bedrooms     = Bedrooms,
            MonthlyRent  = MonthlyRent,
            Status       = Status,
            Description  = Description
        };

        public static PropertyCreateViewModel FromEntity(Property p) => new()
        {
            PropertyId   = p.PropertyId,
            Address      = p.Address,
            City         = p.City,
            PropertyType = p.PropertyType,
            Bedrooms     = p.Bedrooms,
            MonthlyRent  = p.MonthlyRent,
            Status       = p.Status,
            Description  = p.Description
        };
    }
}
