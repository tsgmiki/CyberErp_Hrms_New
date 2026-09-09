namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// The competencies a training course develops — the join that turns a course catalogue into a
/// targeting engine.
///
/// <para>Without it the model can say a person is weak in a competency (from their appraisal) and it
/// can list courses, but nothing connects the two: a gap cannot name a course, and a completed
/// course is not evidence of anything in particular. This is the mirror of
/// <see cref="PositionCompetency"/> — that one says which competencies a ROLE requires, this one says
/// which competencies a COURSE builds.</para>
///
/// <para>⚠️ NO PROFICIENCY LEVEL, deliberately. This product has no per-employee competency level to
/// raise: proficiency exists only as <c>AppraisalCompetency.ManagerScore</c>, a point-in-time rating
/// inside one appraisal. A "target level" here would be a number with nothing to compare against and
/// nowhere to write, so the join carries only what the model can actually honour —
/// <see cref="IsPrimary"/>, which orders recommendations by how central the competency is to the
/// course.</para>
/// </summary>
public class CourseCompetency : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingCourseId { get; private set; }
    public Guid CompetencyId { get; private set; }

    /// <summary>
    /// True when the competency is the course's main subject rather than something it touches in
    /// passing. Primary mappings are recommended first, so a gap in "Cold Chain Management" offers
    /// the cold-chain course ahead of a general induction that happens to mention it.
    /// </summary>
    public bool IsPrimary { get; private set; } = true;

    private CourseCompetency() : base() { }

    public static CourseCompetency Create(Guid trainingCourseId, Guid competencyId, bool isPrimary = true)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));
        if (competencyId == Guid.Empty)
            throw new ArgumentException("A competency is required.", nameof(competencyId));

        return new CourseCompetency
        {
            TrainingCourseId = trainingCourseId,
            CompetencyId = competencyId,
            IsPrimary = isPrimary
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        base.Update();
    }
}
