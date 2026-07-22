using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // Reward & Recognition — §3.7.4 (HC177–HC186): award categories, programs, nominations,
    // points ledger and the monetary-disbursement hand-off.

    public class AwardCategoryConfiguration : IEntityTypeConfiguration<AwardCategory>
    {
        public void Configure(EntityTypeBuilder<AwardCategory> builder)
        {
            builder.ToTable("hrmsAwardCategory", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Criteria).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class RecognitionProgramConfiguration : IEntityTypeConfiguration<RecognitionProgram>
    {
        public void Configure(EntityTypeBuilder<RecognitionProgram> builder)
        {
            builder.ToTable("hrmsRecognitionProgram", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Period).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<RecognitionBadge>()
                .WithMany()
                .HasForeignKey(x => x.RecognitionBadgeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class RewardNominationConfiguration : IEntityTypeConfiguration<RewardNomination>
    {
        public void Configure(EntityTypeBuilder<RewardNomination> builder)
        {
            builder.ToTable("hrmsRewardNomination", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(30);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.NomineeEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<RecognitionBadge>()
                .WithMany()
                .HasForeignKey(x => x.RecognitionBadgeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<RecognitionProgram>()
                .WithMany()
                .HasForeignKey(x => x.RecognitionProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasIndex(x => new { x.TenantId, x.NomineeEmployeeId });
        }
    }

    public class RewardPointsTransactionConfiguration : IEntityTypeConfiguration<RewardPointsTransaction>
    {
        public void Configure(EntityTypeBuilder<RewardPointsTransaction> builder)
        {
            builder.ToTable("hrmsRewardPointsTransaction", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Source).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Balance = SUM over this index; statements read it newest-first.
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.TransactionDate });
        }
    }

    public class RewardDisbursementConfiguration : IEntityTypeConfiguration<RewardDisbursement>
    {
        public void Configure(EntityTypeBuilder<RewardDisbursement> builder)
        {
            builder.ToTable("hrmsRewardDisbursement", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Reference).HasMaxLength(200);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<RecognitionBadge>()
                .WithMany()
                .HasForeignKey(x => x.RecognitionBadgeId)
                .OnDelete(DeleteBehavior.Restrict);
            // SetNull: deleting a recognition keeps the payment row (audit) but drops the link.
            builder.HasOne<EmployeeRecognition>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeRecognitionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }
}
