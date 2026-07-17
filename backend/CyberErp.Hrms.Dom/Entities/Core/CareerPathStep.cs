using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>The required-competency spec set on a career-path step (HC162).</summary>
public record CareerPathStepCompetencySpec(Guid CompetencyId, decimal Weight);

/// <summary>
/// One stage of a <see cref="CareerPath"/> (HC161, HC162): the target role (position class) / grade at
/// this level, the experience &amp; certifications needed to progress into it, and the required
/// competencies (<see cref="CareerPathStepCompetency"/>) used for skill-gap analysis.
/// </summary>
public class CareerPathStep : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid CareerPathId { get; private set; }
    public int StepOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid? PositionClassId { get; private set; }
    public Guid? JobGradeId { get; private set; }
    public int? RequiredExperienceMonths { get; private set; }
    public string? Certifications { get; private set; }
    public string? Description { get; private set; }

    private PositionClass? _positionClass;
    public PositionClass? PositionClass => _positionClass;

    private readonly List<CareerPathStepCompetency> _competencies = [];
    public IReadOnlyCollection<CareerPathStepCompetency> Competencies => _competencies;

    private CareerPathStep() : base() { }

    public static CareerPathStep Create(Guid careerPathId, int stepOrder, string name, Guid? positionClassId, Guid? jobGradeId, int? requiredExperienceMonths, string? certifications, string? description)
    {
        if (careerPathId == Guid.Empty) throw new ArgumentException("A career path is required.", nameof(careerPathId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Step name is required.", nameof(name));
        return new CareerPathStep
        {
            CareerPathId = careerPathId,
            StepOrder = stepOrder,
            Name = name.Trim(),
            PositionClassId = positionClassId,
            JobGradeId = jobGradeId,
            RequiredExperienceMonths = requiredExperienceMonths,
            Certifications = certifications,
            Description = description,
        };
    }

    public void Update(int stepOrder, string name, Guid? positionClassId, Guid? jobGradeId, int? requiredExperienceMonths, string? certifications, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Step name is required.", nameof(name));
        StepOrder = stepOrder;
        Name = name.Trim();
        PositionClassId = positionClassId;
        JobGradeId = jobGradeId;
        RequiredExperienceMonths = requiredExperienceMonths;
        Certifications = certifications;
        Description = description;
        base.Update();
    }

    public void SetCompetencies(IEnumerable<CareerPathStepCompetencySpec> competencies)
    {
        _competencies.Clear();
        foreach (var c in competencies)
            _competencies.Add(CareerPathStepCompetency.Create(Id, c.CompetencyId, c.Weight));
        base.Update();
    }
}

/// <summary>A competency required at a career-path step, with a weighting (HC162 / HC164 gap analysis).</summary>
public class CareerPathStepCompetency : BaseEntity
{
    public Guid CareerPathStepId { get; private set; }
    public Guid CompetencyId { get; private set; }
    public decimal Weight { get; private set; }

    private CareerPathStepCompetency() : base() { }

    public static CareerPathStepCompetency Create(Guid careerPathStepId, Guid competencyId, decimal weight)
    {
        if (competencyId == Guid.Empty) throw new ArgumentException("A competency is required.", nameof(competencyId));
        if (weight < 0) throw new ArgumentException("Weight cannot be negative.", nameof(weight));
        return new CareerPathStepCompetency { CareerPathStepId = careerPathStepId, CompetencyId = competencyId, Weight = weight };
    }
}
