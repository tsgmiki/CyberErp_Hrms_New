using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A named career path / progression track (HC161) — a sequence of <see cref="CareerPathStep"/> stages
/// employees can be assigned to and progress along. The path definition (steps + required skills) is
/// managed on its own; assignments live in <see cref="EmployeeCareerPath"/>.
/// </summary>
public class CareerPath : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CareerPath() : base() { }

    public static CareerPath Create(string name, string code, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Career path name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Career path code is required.", nameof(code));
        return new CareerPath { Name = name.Trim(), Code = code.Trim(), Description = description, IsActive = isActive };
    }

    public void Update(string name, string code, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Career path name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Career path code is required.", nameof(code));
        Name = name.Trim();
        Code = code.Trim();
        Description = description;
        IsActive = isActive;
        base.Update();
    }
}
