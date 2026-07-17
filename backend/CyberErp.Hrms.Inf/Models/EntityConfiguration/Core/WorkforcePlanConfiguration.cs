using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class WorkforcePlanConfiguration : IEntityTypeConfiguration<WorkforcePlan>
    {
        public void Configure(EntityTypeBuilder<WorkforcePlan> builder)
        {
            builder.ToTable("hrmsWorkforcePlan", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.Horizon).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(p => p.Scenario).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(p => p.EscalationJustification).HasMaxLength(2000);
            builder.Property(p => p.TotalBudget).HasColumnType("decimal(18,2)");
            builder.Property(p => p.BudgetThresholdPercent).HasColumnType("decimal(5,2)");
            builder.Property(p => p.ProjectedCost).HasColumnType("decimal(18,2)");

            builder.HasOne<FiscalYear>()
                .WithMany()
                .HasForeignKey(p => p.StartFiscalYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(p => p.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // RootPlanId is a version-chain grouping key, not an FK — versions must survive the
            // deletion of a superseded draft ancestor.

            builder.HasMany(p => p.Lines)
                .WithOne()
                .HasForeignKey(l => l.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(p => p.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(p => new { p.TenantId, p.Status });
            builder.HasIndex(p => p.RootPlanId);
            builder.HasIndex(p => p.OrganizationUnitId);
            builder.HasIndex(p => p.StartFiscalYearId);
        }
    }

    public class WorkforcePlanLineConfiguration : IEntityTypeConfiguration<WorkforcePlanLine>
    {
        public void Configure(EntityTypeBuilder<WorkforcePlanLine> builder)
        {
            builder.ToTable("hrmsWorkforcePlanLine", "dbo");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.EmploymentType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(l => l.RequiredCompetencies).HasMaxLength(1000);
            builder.Property(l => l.Remark).HasMaxLength(1000);
            builder.Property(l => l.AnnualSalaryCost).HasColumnType("decimal(18,2)");
            builder.Property(l => l.AnnualAllowances).HasColumnType("decimal(18,2)");
            builder.Property(l => l.AnnualBenefits).HasColumnType("decimal(18,2)");

            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(l => l.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PositionClass>()
                .WithMany()
                .HasForeignKey(l => l.PositionClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.PlanId);
            // One line per unit × role × employment type × period within a plan.
            builder.HasIndex(l => new { l.PlanId, l.OrganizationUnitId, l.PositionClassId, l.EmploymentType, l.PeriodIndex })
                .IsUnique();
        }
    }
}
