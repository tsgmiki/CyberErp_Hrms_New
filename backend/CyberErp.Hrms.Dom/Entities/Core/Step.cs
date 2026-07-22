using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Salary step lookup (maps to the <c>lupStep</c> table). Backend configuration only — no UI.
/// Combined with a <see cref="JobGrade"/> to define a <see cref="SalaryScale"/> row.
/// </summary>
public class Step : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    private Step() : base() { }

    public static Step Create(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Step code cannot be empty.", nameof(code));

        return new Step { Name = name, Code = code };
    }

    public void Update(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Step code cannot be empty.", nameof(code));

        Name = name;
        Code = code;
        base.Update();
    }
}
