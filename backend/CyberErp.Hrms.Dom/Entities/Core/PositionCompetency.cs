using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Associates a <see cref="Competency"/> to a <see cref="Position"/> with a weighting factor
/// (HC123/HC124). The competencies an employee is assessed on are derived from their position, and
/// the weight reflects each competency's relative importance in the overall performance evaluation.
/// </summary>
public class PositionCompetency : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PositionId { get; private set; }
    public Guid CompetencyId { get; private set; }
    /// <summary>Relative importance (%) of this competency for the position.</summary>
    public decimal Weight { get; private set; }

    private PositionCompetency() : base() { }

    public static PositionCompetency Create(Guid positionId, Guid competencyId, decimal weight)
    {
        if (positionId == Guid.Empty)
            throw new ArgumentException("Position is required.", nameof(positionId));
        if (competencyId == Guid.Empty)
            throw new ArgumentException("Competency is required.", nameof(competencyId));
        if (weight < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));

        return new PositionCompetency { PositionId = positionId, CompetencyId = competencyId, Weight = weight };
    }

    public void SetWeight(decimal weight)
    {
        if (weight < 0) throw new ArgumentException("Weight cannot be negative.", nameof(weight));
        Weight = weight;
        base.Update();
    }
}
