namespace PropertyLeasingReports.Models
{
    public class PropertyReport
    {
        public int PropertyId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int PropertyType { get; set; }
        public int? Bedrooms { get; set; }          // nullable for multi-unit
        public decimal? MonthlyRent { get; set; }   // nullable for multi-unit
        public int? Status { get; set; }            // nullable for multi-unit
        public string? Description { get; set; }
        public List<UnitReport> Units { get; set; } = new();
        public bool IsStandalone => Units.Count == 0;
    }

    public class UnitReport
    {
        public int UnitId { get; set; }
        public int PropertyId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public int UnitType { get; set; }
        public string? Amenities { get; set; }
        public decimal SizeSqm { get; set; }
        public decimal MonthlyRent { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
    }

    public class MaintenanceReport
    {
        public int RequestId { get; set; }
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Category { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? DateResolved { get; set; }
        public string? StaffNotes { get; set; }
    }

    public class LeaseReport
    {
        public int LeaseId { get; set; }
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public int Status { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public string? ApplicationNotes { get; set; }
        public string? ScreeningNotes { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }

    public class PaymentReport
    {
        public int PaymentId { get; set; }
        public int LeaseId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int PaymentType { get; set; }
        public int Status { get; set; }
    }

    // Aggregated view-model used by /Reports/Properties
    public class PropertiesReportViewModel
    {
        public List<PropertyReport> Properties { get; set; } = new();
        public int TotalProperties { get; set; }
        public int StandaloneCount { get; set; }
        public int MultiUnitCount { get; set; }
        public int TotalUnits { get; set; }
        public int LeasedStandalone { get; set; }
        public int LeasedUnits { get; set; }
        public double OccupancyRate { get; set; }  // 0.0 - 1.0
    }
}
