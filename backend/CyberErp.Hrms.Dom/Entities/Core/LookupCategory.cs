namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A generic, CENTRALIZED lookup category (e.g. "Education Level", "Field of Study"). One row here
/// groups a set of <see cref="LookupCategoryList"/> values, so the whole system's reference lists live
/// in just two tables instead of a table per lookup. Lookups are GLOBAL (shared across tenants) — they
/// carry an empty TenantId and are exempt from the repository's tenant filter.
/// </summary>
public class LookupCategory : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    private readonly List<LookupCategoryList> _items = [];
    public IReadOnlyCollection<LookupCategoryList> Items => _items.AsReadOnly();

    private LookupCategory() : base() { }

    public static LookupCategory Create(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Category code is required.", nameof(code));
        return new LookupCategory { Name = name.Trim(), Code = code.Trim() };
    }

    public void Update(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Category code is required.", nameof(code));
        Name = name.Trim();
        Code = code.Trim();
        base.Update();
    }

    /// <summary>Replace the category's value list (clears + re-adds, so a save is idempotent).</summary>
    public void SetItems(IEnumerable<(string Name, string Code)> items)
    {
        _items.Clear();
        foreach (var (itemName, itemCode) in items)
            _items.Add(LookupCategoryList.Create(Id, itemName, itemCode));
        base.Update();
    }
}
