using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How a given weekday is worked, for leave/attendance day-counting.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkDayType
{
    /// <summary>Non-working (weekend/rest) — contributes 0 to chargeable days.</summary>
    Rest = 0,
    /// <summary>Half working day — contributes 0.5.</summary>
    Half = 1,
    /// <summary>Full working day — contributes 1.</summary>
    Full = 2
}

/// <summary>
/// A client's work-week pattern: what each weekday counts as (Full / Half / Rest). Drives the working-day
/// calculation for leave and attendance so non-working days are excluded and half-work Saturdays are
/// costed at 0.5. One configuration is active per tenant; absent one, the calendar falls back to a
/// Saturday/Sunday rest week.
/// </summary>
public class WorkWeekConfiguration : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public WorkDayType Monday { get; private set; } = WorkDayType.Full;
    public WorkDayType Tuesday { get; private set; } = WorkDayType.Full;
    public WorkDayType Wednesday { get; private set; } = WorkDayType.Full;
    public WorkDayType Thursday { get; private set; } = WorkDayType.Full;
    public WorkDayType Friday { get; private set; } = WorkDayType.Full;
    public WorkDayType Saturday { get; private set; } = WorkDayType.Rest;
    public WorkDayType Sunday { get; private set; } = WorkDayType.Rest;
    public bool IsActive { get; private set; } = true;

    private WorkWeekConfiguration() : base() { }

    public static WorkWeekConfiguration Create(string name, WorkDayType mon, WorkDayType tue, WorkDayType wed,
        WorkDayType thu, WorkDayType fri, WorkDayType sat, WorkDayType sun, bool isActive = true)
    {
        Guard(name);
        return new WorkWeekConfiguration
        {
            Name = name,
            Monday = mon, Tuesday = tue, Wednesday = wed, Thursday = thu, Friday = fri,
            Saturday = sat, Sunday = sun, IsActive = isActive
        };
    }

    public void Update(string name, WorkDayType mon, WorkDayType tue, WorkDayType wed, WorkDayType thu,
        WorkDayType fri, WorkDayType sat, WorkDayType sun, bool isActive)
    {
        Guard(name);
        Name = name;
        Monday = mon; Tuesday = tue; Wednesday = wed; Thursday = thu; Friday = fri;
        Saturday = sat; Sunday = sun; IsActive = isActive;
        base.Update();
    }

    /// <summary>The chargeable value of a weekday under this pattern (Full=1, Half=0.5, Rest=0).</summary>
    public decimal WorkValueFor(DayOfWeek day)
    {
        var type = day switch
        {
            DayOfWeek.Monday => Monday,
            DayOfWeek.Tuesday => Tuesday,
            DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday,
            DayOfWeek.Friday => Friday,
            DayOfWeek.Saturday => Saturday,
            _ => Sunday
        };
        return type switch { WorkDayType.Full => 1m, WorkDayType.Half => 0.5m, _ => 0m };
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work-week configuration name cannot be empty.", nameof(name));
    }
}
