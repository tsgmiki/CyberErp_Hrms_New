using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Job grade / pay grade lookup (HC004-HC005). Referenced by <see cref="Position"/> and
/// <see cref="SalaryScale"/>. Salary bands are now expressed per step in coreSalaryScale rather
/// than as min/max columns here.
/// </summary>
public class JobGrade : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    /// <summary>Amharic name.</summary>
    public string? NameA { get; private set; }
    public string Code { get; private set; } = string.Empty;

    private JobGrade() : base() { }

    public static JobGrade Create(string name, string code, string? nameA = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job grade name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Job grade code cannot be empty.", nameof(code));

        return new JobGrade
        {
            Name = name,
            NameA = nameA,
            Code = code
        };
    }

    public void Update(string name, string code, string? nameA = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job grade name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Job grade code cannot be empty.", nameof(code));

        Name = name;
        NameA = nameA;
        Code = code;
        base.Update();
    }
}
