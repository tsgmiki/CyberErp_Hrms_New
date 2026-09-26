using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class ApprovalDelegationConfiguration : IEntityTypeConfiguration<ApprovalDelegation>
    {
        public void Configure(EntityTypeBuilder<ApprovalDelegation> builder)
        {
            builder.ToTable("ApprovalDelegation", "Hrms");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Reason).HasMaxLength(1000);
            builder.Property(d => d.RevokedBy).HasMaxLength(100);
            builder.Property(d => d.RevocationReason).HasMaxLength(1000);
            builder.Property(d => d.ApprovalLimit).HasPrecision(18, 2);

            builder.HasMany(d => d.Scopes)
                .WithOne()
                .HasForeignKey(s => s.DelegationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(d => d.Scopes).UsePropertyAccessMode(PropertyAccessMode.Field);

            // The lookup this feature lives on: "which delegations name ME as the delegate, today?"
            // It runs on EVERY approval evaluation, so it gets a covering index rather than a scan.
            builder.HasIndex(d => new { d.TenantId, d.ToEmployeeId, d.IsRevoked, d.StartDate, d.EndDate })
                .HasDatabaseName("IX_ApprovalDelegation_Delegate_Window");

            // The other direction, for "what have I delegated?" on the self-service screen.
            builder.HasIndex(d => new { d.TenantId, d.FromEmployeeId, d.IsRevoked });
        }
    }

    public class ApprovalDelegationScopeConfiguration : IEntityTypeConfiguration<ApprovalDelegationScope>
    {
        public void Configure(EntityTypeBuilder<ApprovalDelegationScope> builder)
        {
            builder.ToTable("ApprovalDelegationScope", "Hrms");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.EntityType).IsRequired().HasMaxLength(100);
            builder.HasIndex(s => new { s.DelegationId, s.EntityType }).IsUnique();
        }
    }

    public class DelegationPolicyConfiguration : IEntityTypeConfiguration<DelegationPolicy>
    {
        public void Configure(EntityTypeBuilder<DelegationPolicy> builder)
        {
            builder.ToTable("DelegationPolicy", "Hrms");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.DefaultApprovalLimit).HasPrecision(18, 2);

            // One row per tenant — the policy is a singleton, and the database says so rather than
            // trusting every write path to check first.
            builder.HasIndex(p => p.TenantId).IsUnique();
        }
    }
}
