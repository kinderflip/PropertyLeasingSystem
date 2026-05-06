using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyLeasingAPI.Models;

namespace PropertyLeasingAPI.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasIndex(p => new { p.Status, p.DueDate });
            builder.HasIndex(p => p.LeaseId);
        }
    }
}
