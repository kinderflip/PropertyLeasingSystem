namespace PropertyLeasingReports.Models
{
    public class PropertyReport
    {
        public int PropertyId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int PropertyType { get; set; }
        public int Bedrooms { get; set; }
        public decimal MonthlyRent { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
    }

    public class MaintenanceReport
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Category { get; set; }
        public int Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime? DateResolved { get; set; }
    }

    public class LeaseReport
    {
        public int LeaseId { get; set; }
        public int PropertyId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public int Status { get; set; }
    }
}