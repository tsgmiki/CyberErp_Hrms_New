using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
    {
        public void Configure(EntityTypeBuilder<TenantSubscription> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.AmountPaid)
                .HasPrecision(18, 2);

            builder.Property(s => s.PaymentMethod)
                .HasMaxLength(100);

            builder.Property(s => s.TransactionId)
                .HasMaxLength(100);

            // Configure relationships
            builder.HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.SubscriptionPlan)
                .WithMany()
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            builder.HasIndex(s => s.TenantId);
            builder.HasIndex(s => s.SubscriptionPlanId);
            builder.HasIndex(s => s.Status);

            // DateTime for CreatedAt
            builder.Property(s => s.CreatedAt)
                .HasColumnType("datetime2(7)")
                .IsRequired();

            // DateTime for UpdatedAt
            builder.Property(s => s.UpdatedAt)
                .HasColumnType("datetime2(7)");

            builder.Property(s => s.RowVersion)
                ;
        }
    }
}
