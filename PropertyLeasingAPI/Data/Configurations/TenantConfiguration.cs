using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            // D1: real FK to ApplicationUser, set-null on user deletion.
            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(t => t.UserId);
            // D2: indexes on lookup columns (kept non-unique for safety on live data).
            builder.HasIndex(t => t.Email);
            builder.HasIndex(t => t.NationalId);
        }
    }
}
