using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MaintenanceRequest -> AssignedStaff relationship
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedStaff)
                .WithMany()
                .HasForeignKey(m => m.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unit -> Property (cascade delete: removing a property removes its units)
            modelBuilder.Entity<Unit>()
                .HasOne(u => u.Property)
                .WithMany(p => p.Units)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lease -> Unit (optional; set null on unit delete so the lease history isn't lost)
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            // Lease -> Property (restrict to prevent accidental property delete while leases exist).
            // Keep default behaviour (no explicit override) which is Restrict for required FKs.
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Property)
                .WithMany(p => p.Leases)
                .HasForeignKey(l => l.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceRequest -> Unit (optional; set null on unit delete)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Unit)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            // MaintenanceRequest -> Property (restrict)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Property)
                .WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for search / lookup performance
            modelBuilder.Entity<Unit>().HasIndex(u => u.PropertyId);
            modelBuilder.Entity<Unit>().HasIndex(u => u.Status);
            modelBuilder.Entity<Payment>().HasIndex(p => new { p.Status, p.DueDate });

            // Seed Roles
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "1",
                    Name = "PropertyManager",
                    NormalizedName = "PROPERTYMANAGER"
                },
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "2",
                    Name = "Tenant",
                    NormalizedName = "TENANT"
                },
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "3",
                    Name = "MaintenanceStaff",
                    NormalizedName = "MAINTENANCESTAFF"
                }
            );

            // Seed Properties — Bahrain-localised
            // Property 1: Multi-unit residential (Pearl Boulevard Residences, Seef)
            // Property 2: Standalone villa (Al Fateh Villa, Juffair) — keeps Bedrooms/Rent/Status
            // Property 3: Multi-unit commercial (Riffa Commercial Centre, East Riffa)
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    PropertyId = 1,
                    Address = "Building 2455, Road 2832, Block 428, Seef District",
                    City = "Manama",
                    PropertyType = PropertyType.Apartment,
                    Bedrooms = null,
                    MonthlyRent = null,
                    Status = null,
                    Description = "Pearl Boulevard Residences — modern apartment building near Seef Mall."
                },
                new Property
                {
                    PropertyId = 2,
                    Address = "House 108, Road 3803, Block 338, Juffair",
                    City = "Manama",
                    PropertyType = PropertyType.Villa,
                    Bedrooms = 4,
                    MonthlyRent = 900m,
                    Status = PropertyStatus.Available,
                    Description = "Al Fateh Villa — spacious standalone family villa with private garden."
                },
                new Property
                {
                    PropertyId = 3,
                    Address = "Building 217, Road 2409, Block 924",
                    City = "East Riffa",
                    PropertyType = PropertyType.Office,
                    Bedrooms = null,
                    MonthlyRent = null,
                    Status = null,
                    Description = "Riffa Commercial Centre — mixed-use office and retail building."
                }
            );

            // Seed Units
            modelBuilder.Entity<Unit>().HasData(
                // Pearl Boulevard Residences (PropertyId = 1)
                new Unit
                {
                    UnitId = 1,
                    PropertyId = 1,
                    UnitNumber = "101",
                    UnitType = UnitType.OneBedroom,
                    Amenities = "AC, Balcony, Furnished kitchen",
                    SizeSqm = 60m,
                    MonthlyRent = 450m,
                    Status = UnitStatus.Available,
                    Description = "First-floor 1BR apartment facing the pool."
                },
                new Unit
                {
                    UnitId = 2,
                    PropertyId = 1,
                    UnitNumber = "102",
                    UnitType = UnitType.OneBedroom,
                    Amenities = "AC, Balcony",
                    SizeSqm = 62m,
                    MonthlyRent = 470m,
                    Status = UnitStatus.Available,
                    Description = "First-floor 1BR apartment with garden view."
                },
                new Unit
                {
                    UnitId = 3,
                    PropertyId = 1,
                    UnitNumber = "201",
                    UnitType = UnitType.TwoBedroom,
                    Amenities = "AC, 2 balconies, Furnished kitchen, Storage room",
                    SizeSqm = 95m,
                    MonthlyRent = 650m,
                    Status = UnitStatus.Available,
                    Description = "Second-floor 2BR apartment, corner unit."
                },
                // Riffa Commercial Centre (PropertyId = 3)
                new Unit
                {
                    UnitId = 4,
                    PropertyId = 3,
                    UnitNumber = "G1",
                    UnitType = UnitType.Shop,
                    Amenities = "Street-facing shopfront, AC",
                    SizeSqm = 45m,
                    MonthlyRent = 380m,
                    Status = UnitStatus.Available,
                    Description = "Ground-floor retail unit."
                },
                new Unit
                {
                    UnitId = 5,
                    PropertyId = 3,
                    UnitNumber = "G2",
                    UnitType = UnitType.Shop,
                    Amenities = "Street-facing shopfront, AC, Back storeroom",
                    SizeSqm = 50m,
                    MonthlyRent = 420m,
                    Status = UnitStatus.Available,
                    Description = "Ground-floor retail unit next to the main entrance."
                },
                new Unit
                {
                    UnitId = 6,
                    PropertyId = 3,
                    UnitNumber = "F1-Suite-7",
                    UnitType = UnitType.Office,
                    Amenities = "AC, Private washroom, Pantry",
                    SizeSqm = 70m,
                    MonthlyRent = 550m,
                    Status = UnitStatus.Available,
                    Description = "First-floor office suite."
                }
            );

            // Seed Tenants — Bahraini names
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    TenantId = 1,
                    FullName = "Ahmed bin Mohammed Al Mansoori",
                    Email = "ahmed.mansoori@example.bh",
                    Phone = "+97333112233",
                    NationalId = "870412345"
                },
                new Tenant
                {
                    TenantId = 2,
                    FullName = "Sara bint Khalifa Al Khalifa",
                    Email = "sara.khalifa@example.bh",
                    Phone = "+97333445566",
                    NationalId = "900823456"
                },
                // IDs 101+ to avoid collisions with live-created tenants in Azure SQL.
                new Tenant
                {
                    TenantId = 101,
                    FullName = "Fatima bint Isa Al Dosari",
                    Email = "fatima.dosari@example.bh",
                    Phone = "+97333778899",
                    NationalId = "920345678"
                },
                new Tenant
                {
                    TenantId = 102,
                    FullName = "Hamad bin Salman Al Mahmood",
                    Email = "hamad.mahmood@example.bh",
                    Phone = "+97333224455",
                    NationalId = "850612345"
                },
                new Tenant
                {
                    TenantId = 103,
                    FullName = "Noor bint Ali Al Zayani",
                    Email = "noor.zayani@example.bh",
                    Phone = "+97333557788",
                    NationalId = "940109876"
                }
            );
        }
    }
}
