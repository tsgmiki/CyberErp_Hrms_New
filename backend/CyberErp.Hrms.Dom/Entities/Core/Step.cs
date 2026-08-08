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

    /// <summary>
    /// Position of this step on the pay ladder (1 = first rung, ascending, gaps allowed).
    /// <para>
    /// This is the ONLY thing step arithmetic may key off. <see cref="Code"/> cannot be used: it is
    /// free text and differs per tenant ("1".."8" in one, "S1"/"ST1" in others), and carries named
    /// rungs such as "Base" (code "01") and "Ceiling" (code "11") whose codes are not their position.
    /// Parsing the code would silently mis-rank those.
    /// </para>
    /// </summary>
    public int Ordinal { get; private set; }

    private Step() : base() { }

    public static Step Create(string name, string code, int ordinal)
    {
        Guard(name, code, ordinal);
        return new Step { Name = name, Code = code, Ordinal = ordinal };
    }

    public void Update(string name, string code, int ordinal)
    {
        Guard(name, code, ordinal);
        Name = name;
        Code = code;
        Ordinal = ordinal;
        base.Update();
    }

    private static void Guard(string name, string code, int ordinal)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Step code cannot be empty.", nameof(code));
        if (ordinal < 1)
            throw new ArgumentException("Step ordinal must be 1 or greater.", nameof(ordinal));
    }
}
