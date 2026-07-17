namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>One value inside a <see cref="LookupCategory"/> (e.g. "Bachelor's Degree" under
/// "Education Level"). The generic value row every lookup in the system reuses.</summary>
public class LookupCategoryList : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    private LookupCategoryList() : base() { }

    public static LookupCategoryList Create(Guid categoryId, string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Value code is required.", nameof(code));
        return new LookupCategoryList { CategoryId = categoryId, Name = name.Trim(), Code = code.Trim() };
    }

    public void Update(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Value code is required.", nameof(code));
        Name = name.Trim();
        Code = code.Trim();
        base.Update();
    }
}
