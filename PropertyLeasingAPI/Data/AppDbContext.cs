using Microsoft.AspNetCore.Identity;
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

            // --- Relationships ---
            modelBuilder.Entity<Unit>()
                .HasOne(u => u.Property)
                .WithMany(p => p.Units)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Property)
                .WithMany(p => p.Leases)
                .HasForeignKey(l => l.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedStaff)
                .WithMany()
                .HasForeignKey(m => m.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Unit)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Property)
                .WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Indexes ---
            modelBuilder.Entity<Unit>().HasIndex(u => u.PropertyId);
            modelBuilder.Entity<Unit>().HasIndex(u => u.Status);
            modelBuilder.Entity<Tenant>().HasIndex(t => t.UserId);
            modelBuilder.Entity<Tenant>().HasIndex(t => t.Email);
            modelBuilder.Entity<Tenant>().HasIndex(t => t.NationalId);
            modelBuilder.Entity<Payment>().HasIndex(p => new { p.Status, p.DueDate });
            modelBuilder.Entity<Payment>().HasIndex(p => p.LeaseId);

            // --- Seed Roles ---
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "PropertyManager",  NormalizedName = "PROPERTYMANAGER"  },
                new IdentityRole { Id = "2", Name = "Tenant",           NormalizedName = "TENANT"           },
                new IdentityRole { Id = "3", Name = "MaintenanceStaff", NormalizedName = "MAINTENANCESTAFF" }
            );

            // --- Seed Properties ---
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    PropertyId = 1,
                    Address = "Building 2455, Road 2832, Block 428, Seef District",
                    City = "Manama",
                    PropertyType = PropertyType.Apartment,
                    Bedrooms = null, MonthlyRent = null, Status = null,
                    Description = "Pearl Boulevard Residences"
                },
                new Property
                {
                    PropertyId = 2,
                    Address = "House 108, Road 3803, Block 338, Juffair",
                    City = "Manama",
                    PropertyType = PropertyType.Villa,
                    Bedrooms = 4, MonthlyRent = 900m, Status = PropertyStatus.Available,
                    Description = "Al Fateh Villa - standalone family villa"
                },
                new Property
                {
                    PropertyId = 3,
                    Address = "Building 217, Road 2409, Block 924",
                    City = "East Riffa",
                    PropertyType = PropertyType.Office,
                    Bedrooms = null, MonthlyRent = null, Status = null,
                    Description = "Riffa Commercial Centre"
                }
            );

            // --- Seed Units ---
            modelBuilder.Entity<Unit>().HasData(
                new Unit { UnitId = 1, PropertyId = 1, UnitNumber = "101", UnitType = UnitType.OneBedroom,
                           Amenities = "AC, Balcony, Furnished kitchen", SizeSqm = 60m, MonthlyRent = 450m,
                           Status = UnitStatus.Available, Description = "First-floor 1BR apartment." },
                new Unit { UnitId = 2, PropertyId = 1, UnitNumber = "102", UnitType = UnitType.OneBedroom,
                           Amenities = "AC, Balcony", SizeSqm = 62m, MonthlyRent = 470m,
                           Status = UnitStatus.Available, Description = "First-floor 1BR apartment." },
                new Unit { UnitId = 3, PropertyId = 1, UnitNumber = "201", UnitType = UnitType.TwoBedroom,
                           Amenities = "AC, 2 balconies, Furnished kitchen", SizeSqm = 95m, MonthlyRent = 650m,
                           Status = UnitStatus.Available, Description = "Second-floor 2BR apartment." },
                new Unit { UnitId = 4, PropertyId = 3, UnitNumber = "G1", UnitType = UnitType.Shop,
                           Amenities = "Street-facing shopfront, AC", SizeSqm = 45m, MonthlyRent = 380m,
                           Status = UnitStatus.Available, Description = "Ground-floor retail unit." },
                new Unit { UnitId = 5, PropertyId = 3, UnitNumber = "G2", UnitType = UnitType.Shop,
                           Amenities = "Street-facing shopfront, AC", SizeSqm = 50m, MonthlyRent = 420m,
                           Status = UnitStatus.Available, Description = "Ground-floor retail unit." },
                new Unit { UnitId = 6, PropertyId = 3, UnitNumber = "F1-Suite-7", UnitType = UnitType.Office,
                           Amenities = "AC, Private washroom, Pantry", SizeSqm = 70m, MonthlyRent = 550m,
                           Status = UnitStatus.Available, Description = "First-floor office suite." }
            );

            // --- Seed Tenants ---
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { TenantId = 1,   FullName = "Ahmed Hassan", Email = "ahmed.hassan@example.bh", Phone = "+97333112233", NationalId = "870412345" },
                new Tenant { TenantId = 2,   FullName = "Sara Faisal",  Email = "sara.faisal@example.bh",  Phone = "+97333445566", NationalId = "900823456" },
                new Tenant { TenantId = 101, FullName = "Fatima Yousef", Email = "fatima.yousef@example.bh", Phone = "+97333778899", NationalId = "920345678" },
                new Tenant { TenantId = 102, FullName = "Sami Mohamed",  Email = "sami.mohamed@example.bh",  Phone = "+97333224455", NationalId = "850612345" },
                new Tenant { TenantId = 103, FullName = "Nada Tariq",    Email = "nada.tariq@example.bh",    Phone = "+97333557788", NationalId = "940109876" }
            );
        }
    }
}
