using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class NotificationEventConfiguration : IEntityTypeConfiguration<NotificationEvent>
    {
        public void Configure(EntityTypeBuilder<NotificationEvent> builder)
        {
            builder.ToTable("NotificationEvent", "Hrms");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EventKey).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Category).HasMaxLength(60);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.Tokens).HasMaxLength(2000);

            // Per tenant: the catalogue is seeded into each tenant so a template's foreign key stays
            // inside its own tenant, like every other row here.
            builder.HasIndex(e => new { e.TenantId, e.EventKey }).IsUnique();
        }
    }

    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder.ToTable("NotificationTemplate", "Hrms");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.EventKey).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Subject).IsRequired().HasMaxLength(400);
            builder.Property(t => t.Body).IsRequired();                       // nvarchar(max) HTML
            builder.Property(t => t.Channel).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<NotificationEvent>().WithMany()
                .HasForeignKey(t => t.NotificationEventId).OnDelete(DeleteBehavior.Restrict);

            // The dispatcher's hot path: active templates for one event key.
            builder.HasIndex(t => new { t.TenantId, t.EventKey, t.IsActive });
        }
    }

    public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
        {
            builder.ToTable("NotificationRecipient", "Hrms");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Kind).IsRequired().HasConversion<string>().HasMaxLength(40);
            builder.Property(r => r.Delivery).IsRequired().HasConversion<string>().HasMaxLength(10);
            builder.Property(r => r.Address).HasMaxLength(320);   // RFC-max address length

            // CASCADE: a recipient rule has no meaning without its template.
            builder.HasOne<NotificationTemplate>().WithMany()
                .HasForeignKey(r => r.NotificationTemplateId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.NotificationTemplateId);
        }
    }
}
