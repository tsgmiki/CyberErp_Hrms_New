using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
    {
        public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
        {
            builder.ToTable("hrmsEmployeeEducation", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EducationLevel).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Institution).IsRequired().HasMaxLength(300);
            builder.Property(x => x.FieldOfStudy).HasMaxLength(200);
            builder.Property(x => x.Qualification).HasMaxLength(300);
            builder.Property(x => x.Remark).HasMaxLength(1000);

            // Person-owned data: education belongs to the person, not the employment record.
            builder.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PersonId);
        }
    }

    public class EmployeeExperienceConfiguration : IEntityTypeConfiguration<EmployeeExperience>
    {
        public void Configure(EntityTypeBuilder<EmployeeExperience> builder)
        {
            builder.ToTable("hrmsEmployeeExperience", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Organization).IsRequired().HasMaxLength(300);
            builder.Property(x => x.JobTitle).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Responsibilities).HasMaxLength(2000);
            builder.Property(x => x.IsExternal).HasDefaultValue(false);
            builder.Property(x => x.IsGovernmental).HasDefaultValue(false);

            builder.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PersonId);
        }
    }

    public class EmployeeDependentConfiguration : IEntityTypeConfiguration<EmployeeDependent>
    {
        public void Configure(EntityTypeBuilder<EmployeeDependent> builder)
        {
            builder.ToTable("hrmsEmployeeDependent", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Relationship).IsRequired().HasMaxLength(100);
            builder.Property(x => x.PhoneNumber).HasMaxLength(50);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.Remark).HasMaxLength(1000);

            builder.HasOne<Person>()
                .WithMany()
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Internal relationship (HC020) — still an employment link; Restrict avoids cascades.
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.RelatedEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PersonId);
            builder.HasIndex(x => x.RelatedEmployeeId);
        }
    }

    public class EmployeeFieldDefinitionConfiguration : IEntityTypeConfiguration<EmployeeFieldDefinition>
    {
        public void Configure(EntityTypeBuilder<EmployeeFieldDefinition> builder)
        {
            builder.ToTable("hrmsEmployeeFieldDefinition", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OwnerType).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Options).HasMaxLength(2000);

            // Names are unique per (tenant, owner form) — each form has its own field namespace.
            builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.Name }).IsUnique();
        }
    }

    public class EmployeeFieldValueConfiguration : IEntityTypeConfiguration<EmployeeFieldValue>
    {
        public void Configure(EntityTypeBuilder<EmployeeFieldValue> builder)
        {
            builder.ToTable("hrmsEmployeeFieldValue", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OwnerType).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.Value).HasMaxLength(2000);

            // The owner is polymorphic (Employee or a child record) — no cascade FK; each owner's
            // delete handler removes its values via ICustomFieldService.DeleteForOwnerAsync.

            // Definitions with stored values cannot be deleted (deactivate instead).
            builder.HasOne<EmployeeFieldDefinition>()
                .WithMany()
                .HasForeignKey(x => x.FieldDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OwnerType, x.OwnerId, x.FieldDefinitionId }).IsUnique();
        }
    }
}
