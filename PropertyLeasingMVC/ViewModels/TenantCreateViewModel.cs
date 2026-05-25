using System.ComponentModel.DataAnnotations;
using PropertyLeasingAPI.Models;
namespace PropertyLeasingMVC.ViewModels
{
    /// <summary>
    /// Q4: dedicated input model for Tenant create/edit so the entity isn't
    /// bound directly. Phone is normalised at <see cref="ToEntity"/> time.
    /// </summary>
    public class TenantCreateViewModel
    {
        public int TenantId { get; set; }

        [Required, StringLength(100), Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, Phone, StringLength(20)]
        public required string Phone { get; set; }

        [Required, StringLength(20), Display(Name = "National ID")]
        public required string NationalId { get; set; }

        public string? UserId { get; set; }

        public Tenant ToEntity() => new Tenant
        {
            TenantId   = TenantId,
            FullName   = FullName,
            Email      = Email,
            Phone      = Phone,
            NationalId = NationalId,
            UserId     = UserId
        };

        public static TenantCreateViewModel FromEntity(Tenant t) => new()
        {
            TenantId   = t.TenantId,
            FullName   = t.FullName,
            Email      = t.Email,
            Phone      = t.Phone,
            NationalId = t.NationalId,
            UserId     = t.UserId
        };
    }
}
