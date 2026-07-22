using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            builder.ToTable("hrmsWorkflowDefinition", "dbo");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
            builder.Property(d => d.EntityType).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Description).HasMaxLength(1000);

            builder.HasMany(d => d.Steps)
                .WithOne()
                .HasForeignKey(s => s.DefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(d => d.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One-active-per-entity-type is enforced in the save handler (inactive copies allowed).
            builder.HasIndex(d => new { d.TenantId, d.EntityType });
        }
    }

    public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
    {
        public void Configure(EntityTypeBuilder<WorkflowStep> builder)
        {
            builder.ToTable("hrmsWorkflowStep", "dbo");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.ApproverRole).HasMaxLength(200);

            builder.HasMany(s => s.Approvers)
                .WithOne()
                .HasForeignKey(a => a.StepId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(s => s.Approvers).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(s => new { s.DefinitionId, s.StepOrder });
        }
    }

    public class WorkflowStepApproverConfiguration : IEntityTypeConfiguration<WorkflowStepApprover>
    {
        public void Configure(EntityTypeBuilder<WorkflowStepApprover> builder)
        {
            builder.ToTable("hrmsWorkflowStepApprover", "dbo");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.ApproverType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.DisplayName).IsRequired().HasMaxLength(300);

            builder.HasIndex(a => a.StepId);
            builder.HasIndex(a => new { a.ApproverType, a.ApproverId });
        }
    }

    public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
    {
        public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
        {
            builder.ToTable("hrmsWorkflowInstance", "dbo");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.EntityType).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Summary).IsRequired().HasMaxLength(500);
            builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(i => i.CurrentStepName).IsRequired().HasMaxLength(200);
            builder.Property(i => i.RequestedBy).HasMaxLength(200);

            builder.HasOne<WorkflowDefinition>()
                .WithMany()
                .HasForeignKey(i => i.DefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => new { i.EntityType, i.EntityId });
            builder.HasIndex(i => i.Status);
        }
    }

    public class WorkflowActionLogConfiguration : IEntityTypeConfiguration<WorkflowActionLog>
    {
        public void Configure(EntityTypeBuilder<WorkflowActionLog> builder)
        {
            builder.ToTable("hrmsWorkflowActionLog", "dbo");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.StepName).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Action).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(a => a.Comment).HasMaxLength(2000);
            builder.Property(a => a.ActedBy).HasMaxLength(200);

            builder.HasOne<WorkflowInstance>()
                .WithMany()
                .HasForeignKey(a => a.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.InstanceId);
        }
    }
}
