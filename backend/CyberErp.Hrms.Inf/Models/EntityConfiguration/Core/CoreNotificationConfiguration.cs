using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>
    /// Maps <c>Core.Notification</c> — the portal alert table OWNED by the Home portal.
    /// HRMS only WRITES to it (raising approval alerts), so the table is EXCLUDED from HRMS
    /// migrations: Home's migrations create and evolve its schema. Column shapes mirror
    /// Home's config so inserts stay compatible.
    /// </summary>
    public class CoreNotificationConfiguration : IEntityTypeConfiguration<CoreNotification>
    {
        public void Configure(EntityTypeBuilder<CoreNotification> builder)
        {
            builder.ToTable("Notification", "Core", t => t.ExcludeFromMigrations());
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenantId).IsRequired().HasMaxLength(450);
            builder.Property(x => x.SourceSubsystem).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Body).HasMaxLength(2000);
            builder.Property(x => x.LinkUrl).HasMaxLength(600);
            builder.Property(x => x.Severity).IsRequired().HasMaxLength(20);
            builder.Property(x => x.SourceEntityType).HasMaxLength(100);
        }
    }
}
