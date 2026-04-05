using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum PaymentType { Rent, Deposit, Fine }

    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; }

        // Navigation property
        public Lease? Lease { get; set; }
    }
}