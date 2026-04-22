using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyLeasingAPI.Models
{
    public enum PaymentType { Rent, Deposit, Fine }
    public enum PaymentStatus { Pending, Completed, Overdue }

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
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddMonths(1);

        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; }

        [Display(Name = "Status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // B7: compare on Date component to avoid off-by-one when DueDate carries a time.
        [NotMapped]
        public bool IsOverdue => Status == PaymentStatus.Pending && DueDate.Date < DateTime.Today;

        // Navigation property
        public Lease? Lease { get; set; }
    }
}
