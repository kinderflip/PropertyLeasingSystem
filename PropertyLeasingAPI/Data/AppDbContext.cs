using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
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

            // Seed Properties
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    PropertyId = 1,
                    Address = "123 Pearl Boulevard",
                    City = "Manama",
                    PropertyType = PropertyType.Apartment,
                    Bedrooms = 2,
                    MonthlyRent = 350,
                    Status = PropertyStatus.Available,
                    Description = "Modern apartment near the city center"
                },
                new Property
                {
                    PropertyId = 2,
                    Address = "45 Seef District",
                    City = "Manama",
                    PropertyType = PropertyType.Villa,
                    Bedrooms = 4,
                    MonthlyRent = 800,
                    Status = PropertyStatus.Available,
                    Description = "Spacious villa with private garden"
                },
                new Property
                {
                    PropertyId = 3,
                    Address = "78 Riffa Valley Road",
                    City = "Riffa",
                    PropertyType = PropertyType.Shop,
                    Bedrooms = 0,
                    MonthlyRent = 500,
                    Status = PropertyStatus.Available,
                    Description = "Commercial shop in busy area"
                }
            );

            // Seed Tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    TenantId = 1,
                    FullName = "Ahmed Al Mansoori",
                    Email = "ahmed@email.com",
                    Phone = "+97333112233",
                    NationalId = "880112345"
                },
                new Tenant
                {
                    TenantId = 2,
                    FullName = "Sara Al Khalifa",
                    Email = "sara@email.com",
                    Phone = "+97333445566",
                    NationalId = "920567890"
                }
            );
        }
    }
}