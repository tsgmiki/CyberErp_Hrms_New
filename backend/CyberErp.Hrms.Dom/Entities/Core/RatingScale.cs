using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Scoring framework of a rating scale (HC138): a numeric scale (e.g. 1–5) or percentage bands.</summary>
public enum RatingScoreType
{
    Numeric = 0,
    Percentage = 1
}

/// <summary>
/// Performance rating scale (HC138) — a configurable scoring framework referenced by a review cycle
/// and applied to goal/competency ratings. Owns its ordered <see cref="RatingScaleLevel"/> bands.
/// </summary>
public class RatingScale : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public RatingScoreType ScoreType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private readonly List<RatingScaleLevel> _levels = [];
    public IReadOnlyCollection<RatingScaleLevel> Levels => _levels;

    private RatingScale() : base() { }

    public static RatingScale Create(string name, RatingScoreType scoreType, string? description = null,
        bool isActive = true, int sortOrder = 0)
    {
        Guard(name);
        return new RatingScale
        {
            Name = name,
            ScoreType = scoreType,
            Description = description,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, RatingScoreType scoreType, string? description, bool isActive, int sortOrder)
    {
        Guard(name);
        Name = name;
        ScoreType = scoreType;
        Description = description;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    /// <summary>Replaces the scale's levels from the submitted specs (values unique within the scale).</summary>
    public void SetLevels(IEnumerable<RatingScaleLevelSpec> specs)
    {
        var list = specs.ToList();
        var values = new HashSet<int>();
        foreach (var s in list)
        {
            if (string.IsNullOrWhiteSpace(s.Label))
                throw new ArgumentException("Rating level label cannot be empty.", nameof(specs));
            if (!values.Add(s.Value))
                throw new ArgumentException($"Duplicate rating level value '{s.Value}'.", nameof(specs));
        }

        _levels.Clear();
        var order = 0;
        foreach (var s in list)
        {
            _levels.Add(RatingScaleLevel.Create(Id, s.Value, s.Label, s.Description,
                s.MinScore, s.MaxScore, s.SortOrder != 0 ? s.SortOrder : order));
            order++;
        }
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rating scale name cannot be empty.", nameof(name));
    }
}

/// <summary>Input spec for one rating level when saving a scale (see <see cref="RatingScale.SetLevels"/>).</summary>
public record RatingScaleLevelSpec(
    Guid? Id,
    int Value,
    string Label,
    string? Description,
    decimal? MinScore,
    decimal? MaxScore,
    int SortOrder);

/// <summary>One band of a <see cref="RatingScale"/> (e.g. "5 – Outstanding" or "90–100% – Exceeds").</summary>
public class RatingScaleLevel : BaseEntity
{
    public Guid RatingScaleId { get; private set; }
    /// <summary>Ordinal value (numeric scales); also the tie-break order for percentage scales.</summary>
    public int Value { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Inclusive score band (percentage scales) — null for pure numeric levels.</summary>
    public decimal? MinScore { get; private set; }
    public decimal? MaxScore { get; private set; }
    public int SortOrder { get; private set; }

    private RatingScaleLevel() : base() { }

    public static RatingScaleLevel Create(Guid ratingScaleId, int value, string label, string? description,
        decimal? minScore, decimal? maxScore, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Rating level label cannot be empty.", nameof(label));
        return new RatingScaleLevel
        {
            RatingScaleId = ratingScaleId,
            Value = value,
            Label = label,
            Description = description,
            MinScore = minScore,
            MaxScore = maxScore,
            SortOrder = sortOrder
        };
    }
}
