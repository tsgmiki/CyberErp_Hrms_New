using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Modules.DTOs
{
    public class ModuleDtoValidator : AbstractValidator<ModuleDto>
    {
        public ModuleDtoValidator()
        {
            RuleFor(x => x.SubsystemId)
                .NotEmpty().WithMessage("Subsystem is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
        }
    }

    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.SubsystemId)
                .NotEmpty().WithMessage("Subsystem is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
        }
    }

    public class ModuleDto
    {
        public Guid Id { get; set; }
        public Guid SubsystemId { get; set; }
        /// <summary>Subsystem display name (resolved from SubsystemId).</summary>
        public string SubSystem { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class GetModuleDto
    {
        public Guid Id { get; set; }
        public Guid SubsystemId { get; set; }
        /// <summary>Subsystem display name (resolved from SubsystemId).</summary>
        public string SubSystem { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateModuleDto
    {
        public Guid Id { get; set; }
        public Guid SubsystemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ModuleResult
    {
        public Guid Id { get; set; }
        public Guid SubsystemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedAt { get; set; }
    }

    public class GetModuleWithOperationResult
    {
        public Guid Id { get; set; }
        public Guid SubsystemId { get; set; }
        /// <summary>Subsystem display NAME (resolved from SubsystemId) — renameable; do NOT scope on it.</summary>
        public string SubSystem { get;set;}
        /// <summary>
        /// The owning subsystem's ABBREVIATION (Core.Subsystem.Abbreviation) — what a SPA scopes its
        /// sidebar on. <see cref="SubSystem"/> is a display label an administrator can rename, and
        /// did: "HRMS" became "Human Resource Management System", which silently emptied this
        /// application's menu for anyone arriving from the Home portal.
        /// </summary>
        public string SubSystemAbbreviation { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public List<OperationRecord> Operations { get; set; } = new();
    }

    public class OperationRecord
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanView { get; set; }
        /// <summary>Core.TenantRolePermission.CanExport — the column existed but never left the database.</summary>
        public bool CanExport { get; set; }
    }
}

