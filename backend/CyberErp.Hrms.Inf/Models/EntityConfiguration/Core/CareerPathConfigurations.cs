using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // ===== Career Development §3.7.B — Career Path (HC161–HC169) =====

    public class CareerPathConfiguration : IEntityTypeConfiguration<CareerPath>
    {
        public void Configure(EntityTypeBuilder<CareerPath> builder)
        {
            builder.ToTable("hrmsCareerPath", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        }
    }

    public class CareerPathStepConfiguration : IEntityTypeConfiguration<CareerPathStep>
    {
        public void Configure(EntityTypeBuilder<CareerPathStep> builder)
        {
            builder.ToTable("hrmsCareerPathStep", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Certifications).HasMaxLength(1000);
            builder.Property(x => x.Description).HasMaxLength(2000);

            builder.HasOne<CareerPath>().WithMany().HasForeignKey(x => x.CareerPathId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.PositionClass).WithMany().HasForeignKey(x => x.PositionClassId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.PositionClass).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne<JobGrade>().WithMany().HasForeignKey(x => x.JobGradeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Competencies).WithOne().HasForeignKey(c => c.CareerPathStepId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Competencies).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.CareerPathId, x.StepOrder }).IsUnique();
        }
    }

    public class CareerPathStepCompetencyConfiguration : IEntityTypeConfiguration<CareerPathStepCompetency>
    {
        public void Configure(EntityTypeBuilder<CareerPathStepCompetency> builder)
        {
            builder.ToTable("hrmsCareerPathStepCompetency", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Weight).HasPrecision(5, 2);

            builder.HasOne<Competency>().WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CareerPathStepId, x.CompetencyId }).IsUnique();
        }
    }

    public class EmployeeCareerPathConfiguration : IEntityTypeConfiguration<EmployeeCareerPath>
    {
        public void Configure(EntityTypeBuilder<EmployeeCareerPath> builder)
        {
            builder.ToTable("hrmsEmployeeCareerPath", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProgressPercent).HasPrecision(5, 2);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.AssignedBy).HasMaxLength(150);
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne(x => x.CareerPath).WithMany().HasForeignKey(x => x.CareerPathId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.CareerPath).UsePropertyAccessMode(PropertyAccessMode.Field);
            // CurrentStepId is a soft reference to a step within the same path (app-validated, no FK).

            builder.HasMany(x => x.StepProgress).WithOne().HasForeignKey(p => p.EmployeeCareerPathId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.StepProgress).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.CareerPathId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
            builder.HasIndex(x => new { x.TenantId, x.CareerPathId });
        }
    }

    public class EmployeeCareerPathStepProgressConfiguration : IEntityTypeConfiguration<EmployeeCareerPathStepProgress>
    {
        public void Configure(EntityTypeBuilder<EmployeeCareerPathStepProgress> builder)
        {
            builder.ToTable("hrmsEmployeeCareerPathStepProgress", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            // CareerPathStepId is a soft reference (the step belongs to the assignment's path).
            builder.HasIndex(x => x.EmployeeCareerPathId);
        }
    }

    public class MentorshipConfiguration : IEntityTypeConfiguration<Mentorship>
    {
        public void Configure(EntityTypeBuilder<Mentorship> builder)
        {
            builder.ToTable("hrmsMentorship", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Context).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne(x => x.Mentor).WithMany().HasForeignKey(x => x.MentorEmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Mentor).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne(x => x.Mentee).WithMany().HasForeignKey(x => x.MenteeEmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Mentee).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.MenteeEmployeeId });
            builder.HasIndex(x => x.MentorEmployeeId);
        }
    }

    public class CareerPathChangeRequestConfiguration : IEntityTypeConfiguration<CareerPathChangeRequest>
    {
        public void Configure(EntityTypeBuilder<CareerPathChangeRequest> builder)
        {
            builder.ToTable("hrmsCareerPathChangeRequest", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(2000);
            builder.Property(x => x.DecisionNotes).HasMaxLength(2000);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne<CareerPath>().WithMany().HasForeignKey(x => x.RequestedCareerPathId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<CareerPath>().WithMany().HasForeignKey(x => x.CurrentCareerPathId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Status });
        }
    }
}
