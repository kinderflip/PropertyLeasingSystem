using Microsoft.AspNetCore.Identity;

namespace PropertyLeasingAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}