using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data.Configurations
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            // Unit -> Property: cascade so removing a property removes its units.
            builder.HasOne(u => u.Property)
                .WithMany(p => p.Units)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(u => u.PropertyId);
            builder.HasIndex(u => u.Status);
        }
    }
}
