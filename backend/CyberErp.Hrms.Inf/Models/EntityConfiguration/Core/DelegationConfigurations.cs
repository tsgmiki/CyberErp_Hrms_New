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

    public class ActingAssignmentConfiguration : IEntityTypeConfiguration<ActingAssignment>
    {
        public void Configure(EntityTypeBuilder<ActingAssignment> builder)
        {
            builder.ToTable("ActingAssignment", "Hrms");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.PositionTitle).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Notes).HasMaxLength(1000);
            builder.Property(a => a.ActingSalary).HasPrecision(18, 2);
            builder.Property(a => a.OriginalSalary).HasPrecision(18, 2);

            // "Which assignments belong to this delegation" — asked on every save and every
            // withdrawal, so it is an index rather than a scan.
            builder.HasIndex(a => new { a.TenantId, a.DelegationId });
            // The nightly sweep's query: active assignments whose end date has passed.
            builder.HasIndex(a => new { a.TenantId, a.Status, a.EndDate });
            builder.HasIndex(a => new { a.TenantId, a.EmployeeId });
        }
    }

    public class PositionEntitlementConfiguration : IEntityTypeConfiguration<PositionEntitlement>
    {
        public void Configure(EntityTypeBuilder<PositionEntitlement> builder)
        {
            builder.ToTable("PositionEntitlement", "Hrms");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Value).HasPrecision(18, 2);
            builder.Property(e => e.Notes).HasMaxLength(500);

            // A convenience accessor over the two nullable FKs, not a column. EF maps every public
            // property by default and refuses one with no backing field and no setter.
            builder.Ignore(e => e.ReferenceId);

            // "What does this post carry" — the read every acting grant makes.
            builder.HasIndex(e => new { e.TenantId, e.PositionClassId, e.IsActive });

            // ⚠️ Restrict, never cascade. Deleting an allowance type out from under the posts that
            // grant it would silently change what several jobs are worth; the delete should fail
            // and make somebody look at the entitlements first.
            builder.HasOne<AllowanceType>()
                .WithMany()
                .HasForeignKey(e => e.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<BenefitPlan>()
                .WithMany()
                .HasForeignKey(e => e.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<PositionClass>()
                .WithMany()
                .HasForeignKey(e => e.PositionClassId)
                .OnDelete(DeleteBehavior.Cascade);
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
