using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>Maps onto the pre-existing Core.FiscalYear table (created outside EF; adopted, not recreated).</summary>
    public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
    {
        public void Configure(EntityTypeBuilder<FiscalYear> builder)
        {
            builder.ToTable("FiscalYear", "Core");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    /// <summary>Annual-leave accrual policy per fiscal year (successor of legacy hrmsAnnualLeaveSetting).</summary>
    public class AnnualLeaveSettingConfiguration : IEntityTypeConfiguration<AnnualLeaveSetting>
    {
        public void Configure(EntityTypeBuilder<AnnualLeaveSetting> builder)
        {
            builder.ToTable("hrms_AnnualLeaveSetting", "Core");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.FiscalYear).WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.FiscalYear).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.LeaveType).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One policy per fiscal year + leave type.
            builder.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.LeaveTypeId }).IsUnique();
        }
    }
}
