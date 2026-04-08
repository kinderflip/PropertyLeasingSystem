using System.ComponentModel.DataAnnotations;

namespace PropertyLeasingAPI.Models
{
    public enum NotificationType { LeaseUpdate, MaintenanceUpdate, PaymentReminder, General }

    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.General;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? LinkUrl { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
