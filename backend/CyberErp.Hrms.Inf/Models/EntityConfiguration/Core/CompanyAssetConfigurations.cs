using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // Exit / Separation — asset recovery (HC214/HC215).

    public class CompanyAssetConfiguration : IEntityTypeConfiguration<CompanyAsset>
    {
        public void Configure(EntityTypeBuilder<CompanyAsset> builder)
        {
            builder.ToTable("hrmsCompanyAsset", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.SerialNo).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            // Serves the exit checklist generation: one indexed read of the employee's assignments.
            builder.HasIndex(x => new { x.TenantId, x.AssignedToEmployeeId });
        }
    }

    public class ExitQuestionnaireConfiguration : IEntityTypeConfiguration<ExitQuestionnaire>
    {
        public void Configure(EntityTypeBuilder<ExitQuestionnaire> builder)
        {
            builder.ToTable("hrmsExitQuestionnaire", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.QuestionsJson).IsRequired();
        }
    }

    public class ExitInterviewConfiguration : IEntityTypeConfiguration<ExitInterview>
    {
        public void Configure(EntityTypeBuilder<ExitInterview> builder)
        {
            builder.ToTable("hrmsExitInterview", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.QuestionsJson).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<EmployeeTermination>()
                .WithMany()
                .HasForeignKey(x => x.TerminationId)
                .OnDelete(DeleteBehavior.Cascade);

            // One interview per exit case.
            builder.HasIndex(x => new { x.TenantId, x.TerminationId }).IsUnique();
        }
    }

    public class TerminationSettlementConfiguration : IEntityTypeConfiguration<TerminationSettlement>
    {
        public void Configure(EntityTypeBuilder<TerminationSettlement> builder)
        {
            builder.ToTable("hrmsTerminationSettlement", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.PaidReference).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne<EmployeeTermination>()
                .WithMany()
                .HasForeignKey(x => x.TerminationId)
                .OnDelete(DeleteBehavior.Cascade);

            // One worksheet per exit case.
            builder.HasIndex(x => new { x.TenantId, x.TerminationId }).IsUnique();
        }
    }

    public class SettlementLineConfiguration : IEntityTypeConfiguration<SettlementLine>
    {
        public void Configure(EntityTypeBuilder<SettlementLine> builder)
        {
            builder.ToTable("hrmsSettlementLine", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Amount).HasPrecision(18, 2);

            builder.HasOne<TerminationSettlement>()
                .WithMany()
                .HasForeignKey(x => x.TerminationSettlementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.TerminationSettlementId });
        }
    }

    public class TerminationAssetRecoveryConfiguration : IEntityTypeConfiguration<TerminationAssetRecovery>
    {
        public void Configure(EntityTypeBuilder<TerminationAssetRecovery> builder)
        {
            builder.ToTable("hrmsTerminationAssetRecovery", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssetName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(20);
            builder.Property(x => x.SerialNo).HasMaxLength(100);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne<EmployeeTermination>()
                .WithMany()
                .HasForeignKey(x => x.TerminationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<CompanyAsset>()
                .WithMany()
                .HasForeignKey(x => x.CompanyAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.TerminationId });
        }
    }
}
