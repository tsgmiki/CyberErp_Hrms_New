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

            // TenantId is GONE (2026-08-13) — Role is a global TEMPLATE now; the per-tenant instance
            // is Core.TenantRole. BaseEntity still declares the property, so it must be ignored
            // explicitly or EF keeps looking for the column.
            builder.Ignore(r => r.TenantId);

            builder.HasKey(r => r.Id);
            builder.HasKey(r => r.Id).HasName("PK_StandardRoleTemplate");   // SRMS's constraint name

            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

            // SRMS alignment: Code identifies the role TEMPLATE, so it is required and unique.
            builder.Property(r => r.Code).IsRequired().HasMaxLength(80);
            builder.Property(r => r.Description).IsRequired().HasMaxLength(500);
            builder.Property(r => r.IsPlatformRole).IsRequired().HasDefaultValue(false);
            builder.Property(r => r.IsActive).IsRequired();

            // Still NOT unique, even though SRMS's IX_StandardRoleTemplate_Code is. The 8 rows here
            // came from two tenants and were only ever distinct by accident; making it unique now
            // would be a data assertion this table has never had to satisfy. Uniqueness stays where
            // it can be stated meaningfully — per tenant, in SaveRole, via TenantRole.
            builder.HasIndex(r => r.Code);

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

            // ⚠️ TenantId is GONE (2026-08-15) — SRMS has none, so this table is GLOBAL now, and
            // unlike every other drop in this series NOTHING can re-derive the tenant: a row carries
            // only UserId and RoleId, and Core.User and Core.Role are both global too.
            //
            // That is why the projector's membership sweep was DELETED rather than re-scoped, and why
            // the six places that read this table by UserId or RoleId now go through
            // ICurrentUserRoles, which answers the same questions from the tenant-scoped model.
            // A bare `userRoles.GetAll().Where(u => u.UserId == x)` is a CROSS-TENANT read.
            builder.Ignore(u => u.TenantId);

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
