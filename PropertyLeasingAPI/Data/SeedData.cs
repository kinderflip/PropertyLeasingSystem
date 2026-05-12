using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data
{
    /// <summary>
    /// D7: seed data extracted from <see cref="AppDbContext.OnModelCreating"/> so the
    /// context stays focused on relationship configuration.
    /// IDs match the existing production rows — do not renumber without a coordinated
    /// data migration on the live database.
    /// </summary>
    public static class SeedData
    {
        public static void Apply(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedProperties(modelBuilder);
            SeedUnits(modelBuilder);
            SeedTenants(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = Roles.PropertyManager,  NormalizedName = "PROPERTYMANAGER"  },
                new IdentityRole { Id = "2", Name = Roles.Tenant,           NormalizedName = "TENANT"           },
                new IdentityRole { Id = "3", Name = Roles.MaintenanceStaff, NormalizedName = "MAINTENANCESTAFF" }
            );
        }

        private static void SeedProperties(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    PropertyId = 1,
                    Address = "Building 2455, Road 2832, Block 428, Seef District",
                    City = "Manama",
                    PropertyType = PropertyType.Apartment,
                    Bedrooms = null, MonthlyRent = null, Status = null,
                    Description = "Pearl Boulevard Residences — modern apartment building near Seef Mall."
                },
                new Property
                {
                    PropertyId = 2,
                    Address = "House 108, Road 3803, Block 338, Juffair",
                    City = "Manama",
                    PropertyType = PropertyType.Villa,
                    Bedrooms = 4, MonthlyRent = 900m, Status = PropertyStatus.Available,
                    Description = "Al Fateh Villa — spacious standalone family villa with private garden."
                },
                new Property
                {
                    PropertyId = 3,
                    Address = "Building 217, Road 2409, Block 924",
                    City = "East Riffa",
                    PropertyType = PropertyType.Office,
                    Bedrooms = null, MonthlyRent = null, Status = null,
                    Description = "Riffa Commercial Centre — mixed-use office and retail building."
                }
            );
        }

        private static void SeedUnits(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Unit>().HasData(
                new Unit { UnitId = 1, PropertyId = 1, UnitNumber = "101", UnitType = UnitType.OneBedroom,
                           Amenities = "AC, Balcony, Furnished kitchen", SizeSqm = 60m, MonthlyRent = 450m,
                           Status = UnitStatus.Available, Description = "First-floor 1BR apartment facing the pool." },
                new Unit { UnitId = 2, PropertyId = 1, UnitNumber = "102", UnitType = UnitType.OneBedroom,
                           Amenities = "AC, Balcony", SizeSqm = 62m, MonthlyRent = 470m,
                           Status = UnitStatus.Available, Description = "First-floor 1BR apartment with garden view." },
                new Unit { UnitId = 3, PropertyId = 1, UnitNumber = "201", UnitType = UnitType.TwoBedroom,
                           Amenities = "AC, 2 balconies, Furnished kitchen, Storage room", SizeSqm = 95m, MonthlyRent = 650m,
                           Status = UnitStatus.Available, Description = "Second-floor 2BR apartment, corner unit." },
                new Unit { UnitId = 4, PropertyId = 3, UnitNumber = "G1", UnitType = UnitType.Shop,
                           Amenities = "Street-facing shopfront, AC", SizeSqm = 45m, MonthlyRent = 380m,
                           Status = UnitStatus.Available, Description = "Ground-floor retail unit." },
                new Unit { UnitId = 5, PropertyId = 3, UnitNumber = "G2", UnitType = UnitType.Shop,
                           Amenities = "Street-facing shopfront, AC, Back storeroom", SizeSqm = 50m, MonthlyRent = 420m,
                           Status = UnitStatus.Available, Description = "Ground-floor retail unit next to the main entrance." },
                new Unit { UnitId = 6, PropertyId = 3, UnitNumber = "F1-Suite-7", UnitType = UnitType.Office,
                           Amenities = "AC, Private washroom, Pantry", SizeSqm = 70m, MonthlyRent = 550m,
                           Status = UnitStatus.Available, Description = "First-floor office suite." }
            );
        }

        private static void SeedTenants(ModelBuilder modelBuilder)
        {
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
