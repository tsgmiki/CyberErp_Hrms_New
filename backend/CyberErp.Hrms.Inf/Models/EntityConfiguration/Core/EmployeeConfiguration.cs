using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("hrms_Employee", "Core");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.EmploymentStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(e => e.PlaceOfBirth).HasMaxLength(200);
            builder.Property(e => e.SpouseName).HasMaxLength(200);
            builder.Property(e => e.Email).HasMaxLength(200);
            builder.Property(e => e.PhotoUrl).HasMaxLength(500);
            builder.Property(e => e.NationalId).HasMaxLength(100);
            builder.Property(e => e.Tin).HasMaxLength(100);
            builder.Property(e => e.PensionNumber).HasMaxLength(100);

            builder.Property(e => e.Salary).HasPrecision(18, 2);

            // Personal identity — the person record outlives the employment record (Restrict).
            builder.HasOne(e => e.Person)
                .WithMany()
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(e => e.Person).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.JobGrade)
                .WithMany()
                .HasForeignKey(e => e.JobGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(e => e.Position).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(e => e.JobGrade).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(e => e.Branch).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(e => e.PersonId);
            builder.HasIndex(e => e.PositionId);
            builder.HasIndex(e => e.JobGradeId);
            builder.HasIndex(e => e.BranchId);
            builder.HasIndex(e => e.EmploymentStatus);
            // Employee numbers are unique organization-wide (per tenant), across all branches.
            builder.HasIndex(e => new { e.TenantId, e.EmployeeNumber }).IsUnique();
        }
    }
}
