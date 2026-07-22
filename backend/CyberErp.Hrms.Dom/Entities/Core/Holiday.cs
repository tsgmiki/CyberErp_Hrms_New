using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HolidayType
{
    Public = 0,
    Religious = 1,
    Organizational = 2
}

/// <summary>
/// Attendance &amp; Leave (HC040): a non-working public/religious/organizational holiday. Excluded from
/// working-day computations for leave and attendance. The date is stored as the canonical Gregorian
/// date; the UI renders it alongside the Ethiopian calendar.
/// </summary>
public class Holiday : BaseEntity, IAggregateRoot, IAuditable
{
    public DateTime Date { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? NameA { get; private set; }
    public HolidayType HolidayType { get; private set; } = HolidayType.Public;
    /// <summary>Recurs on the same month/day every year (e.g. fixed national holidays).</summary>
    public bool IsRecurring { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Holiday() : base() { }

    public static Holiday Create(
        DateTime date,
        string name,
        string? nameA = null,
        HolidayType holidayType = HolidayType.Public,
        bool isRecurring = false,
        string? description = null,
        bool isActive = true)
    {
        Validate(name);
        return new Holiday
        {
            Date = date.Date,
            Name = name.Trim(),
            NameA = nameA,
            HolidayType = holidayType,
            IsRecurring = isRecurring,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(
        DateTime date,
        string name,
        string? nameA,
        HolidayType holidayType,
        bool isRecurring,
        string? description,
        bool isActive)
    {
        Validate(name);
        Date = date.Date;
        Name = name.Trim();
        NameA = nameA;
        HolidayType = holidayType;
        IsRecurring = isRecurring;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Holiday name cannot be empty.", nameof(name));
    }
}
