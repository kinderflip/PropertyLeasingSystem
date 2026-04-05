using System.ComponentModel.DataAnnotations;

namespace PropertyLeasingMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}