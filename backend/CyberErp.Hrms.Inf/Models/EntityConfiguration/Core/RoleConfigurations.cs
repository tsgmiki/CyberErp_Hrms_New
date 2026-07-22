using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>Maps onto the pre-existing Core.Role table (created by the Initial migration).</summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role", "Core");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Code).HasMaxLength(100);

            // Relationship configured on RolePermissionConfiguration; expose the backing field.
            builder.Navigation(r => r.RolePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    /// <summary>
    /// Maps onto the pre-existing Core.UserRole table. The legacy nullable UserId1 column is
    /// intentionally unmapped.
    /// </summary>
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRole", "Core");

            builder.HasKey(u => u.Id);

            // RoleId / UserId are mapped as plain scalar columns (no EF relationship). The handler
            // joins by id rather than traversing navigations, and configuring HasOne<Role>/<User>
            // here produced duplicate shadow FKs (RoleId1/UserId1) now that Role/User are prominent
            // in the model. The database still enforces its own FK constraints.
            builder.Property(u => u.UserId).IsRequired();
            builder.Property(u => u.RoleId).IsRequired();

            builder.HasIndex(u => u.UserId);
            builder.HasIndex(u => u.RoleId);
        }
    }
}
