using System.Collections.Generic;
using CyberErp.Hrms.Dom.Events;
using NodaTime;

namespace CyberErp.Hrms.Dom.Entities;

public abstract class BaseEntity : ITenantEntity
{
    public Guid Id { get; private set; }
    public string TenantId { get; set; } = string.Empty;
    public Instant CreatedAt { get; private set; }
    public Instant? UpdatedAt { get; private set; }
    public string? CreatedBy { get; protected internal set; }
    public string? UpdatedBy { get; set; }
    public byte[] RowVersion { get; private set; } 
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor for EF Core and factory methods
     protected BaseEntity()
     {
         Id = Guid.NewGuid();
         CreatedAt = SystemClock.Instance.GetCurrentInstant();
         RowVersion = new byte[8];
         System.BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(RowVersion, 0);
     }

     protected BaseEntity(string tenantId, string? createdBy = null)
     {
         Id = Guid.NewGuid();
         TenantId = tenantId;
         CreatedAt = SystemClock.Instance.GetCurrentInstant();
         CreatedBy = createdBy;
         RowVersion = new byte[8];
         System.BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(RowVersion, 0);
     }
 public void Create(string? createdBy = null)
    {
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        CreatedBy = createdBy;
    }
    // Update method for common properties
    public void Update(string? updatedBy = null)
    {
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        UpdatedBy = updatedBy;
        RowVersion = new byte[8];
        System.BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(RowVersion, 0);
    }

    // Parameterless update for use when UpdatedBy is set by Repository
    public void Update()
    {
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        RowVersion = new byte[8];
        System.BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(RowVersion, 0);
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    public void SetId(Guid id)
    {
        if (Id != Guid.Empty)
            throw new InvalidOperationException("Id can only be set once.");
        Id = id;
    }

    public void IncrementRowVersion()
    {
        RowVersion = new byte[8];
        System.BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(RowVersion, 0);
    }
}

public interface ITenantEntity
{
    string TenantId { get; }
}

