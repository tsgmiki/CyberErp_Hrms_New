using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Salary scale entry (maps to the <c>coreSalaryScale</c> table): the pay amount for a
/// specific <see cref="JobGrade"/> at a specific <see cref="Step"/>.
/// </summary>
public class SalaryScale : BaseEntity, IAggregateRoot
{
    public Guid JobGradeId { get; private set; }
    public Guid StepId { get; private set; }
    public decimal Salary { get; private set; }

    public JobGrade JobGrade { get; private set; } = null!;
    public Step Step { get; private set; } = null!;

    private SalaryScale() : base() { }

    public static SalaryScale Create(Guid jobGradeId, Guid stepId, decimal salary)
    {
        if (jobGradeId == Guid.Empty)
            throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));
        if (stepId == Guid.Empty)
            throw new ArgumentException("StepId cannot be empty.", nameof(stepId));
        if (salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        return new SalaryScale { JobGradeId = jobGradeId, StepId = stepId, Salary = salary };
    }

    public void Update(Guid jobGradeId, Guid stepId, decimal salary)
    {
        if (jobGradeId == Guid.Empty)
            throw new ArgumentException("JobGradeId cannot be empty.", nameof(jobGradeId));
        if (stepId == Guid.Empty)
            throw new ArgumentException("StepId cannot be empty.", nameof(stepId));
        if (salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        JobGradeId = jobGradeId;
        StepId = stepId;
        Salary = salary;
        base.Update();
    }
}
