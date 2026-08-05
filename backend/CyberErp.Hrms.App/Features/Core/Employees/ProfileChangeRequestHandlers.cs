using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class ProfileChangeRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string FieldKey { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string? CurrentValue { get; set; }
        public string RequestedValue { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public bool AutoApplied { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string? ResolvedBy { get; set; }
    }

    /// <summary>The HR review queue for the dashboard (mirrors the My-Approvals contract).</summary>
    public class ProfileChangeApprovalsDto
    {
        public bool IsApprover { get; set; }
        public List<ProfileChangeRequestDto> Items { get; set; } = [];
    }

    /// <summary>One selectable restricted field, for the employee's Request-Change picker.</summary>
    public class ProfileChangeFieldDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string? CurrentValue { get; set; }
    }

    public class SubmitProfileChangeRequestDto
    {
        public string FieldKey { get; set; } = string.Empty;
        public string RequestedValue { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class SubmitProfileChangeRequestDtoValidator : AbstractValidator<SubmitProfileChangeRequestDto>
    {
        public SubmitProfileChangeRequestDtoValidator()
        {
            RuleFor(x => x.FieldKey).NotEmpty()
                .Must(k => ProfileChangeCatalog.Fields.ContainsKey(k))
                .WithMessage("That field cannot be change-requested.");
            RuleFor(x => x.RequestedValue).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Reason).MaximumLength(2000);
        }
    }

    public class ResolveProfileChangeRequestDto
    {
        /// <summary>Approve | Reject.</summary>
        public string Decision { get; set; } = "Approve";
        public string? Resolution { get; set; }
    }

    // ---- The restricted-field catalog + apply engine -------------------------
    /// <summary>
    /// The allowlist of change-requestable restricted fields. IdentityField entries are auto-applied
    /// to Person/Employee on approval; Structural ones are acknowledged for HR to fulfil via the
    /// owning module (salary/position via movements, education/experience via the profile children).
    /// </summary>
    internal static class ProfileChangeCatalog
    {
        internal static readonly Dictionary<string, (string Label, ProfileChangeKind Kind)> Fields =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["FirstName"] = ("First Name", ProfileChangeKind.IdentityField),
                ["FatherName"] = ("Father Name", ProfileChangeKind.IdentityField),
                ["GrandFatherName"] = ("Grandfather Name", ProfileChangeKind.IdentityField),
                ["FirstNameA"] = ("First Name (Amharic)", ProfileChangeKind.IdentityField),
                ["FatherNameA"] = ("Father Name (Amharic)", ProfileChangeKind.IdentityField),
                ["GrandFatherNameA"] = ("Grandfather Name (Amharic)", ProfileChangeKind.IdentityField),
                ["Gender"] = ("Gender", ProfileChangeKind.IdentityField),
                ["DateOfBirth"] = ("Date of Birth", ProfileChangeKind.IdentityField),
                ["NationalId"] = ("National ID", ProfileChangeKind.IdentityField),
                ["Tin"] = ("TIN", ProfileChangeKind.IdentityField),
                ["PensionNumber"] = ("Pension Number", ProfileChangeKind.IdentityField),
                ["Salary"] = ("Salary", ProfileChangeKind.Structural),
                ["Position"] = ("Position / Placement", ProfileChangeKind.Structural),
                ["Education"] = ("Education", ProfileChangeKind.Structural),
                ["Experience"] = ("Work Experience", ProfileChangeKind.Structural),
                ["Other"] = ("Other", ProfileChangeKind.Structural),
            };

        internal static string? CurrentValue(string fieldKey, Employee e, Person? p) => fieldKey switch
        {
            "FirstName" => p?.FirstName,
            "FatherName" => p?.FatherName,
            "GrandFatherName" => p?.GrandFatherName,
            "FirstNameA" => p?.FirstNameA,
            "FatherNameA" => p?.FatherNameA,
            "GrandFatherNameA" => p?.GrandFatherNameA,
            "Gender" => p?.Gender.ToString(),
            "DateOfBirth" => e.DateOfBirth?.ToString("yyyy-MM-dd"),
            "NationalId" => e.NationalId,
            "Tin" => e.Tin,
            "PensionNumber" => e.PensionNumber,
            "Salary" => e.Salary?.ToString(),
            _ => null
        };

        /// <summary>Writes one approved IdentityField change to the record; throws on a bad value.</summary>
        internal static void Apply(string fieldKey, string value, Employee e, Person p)
        {
            // Person names/gender: rebuild via Person.Update passing the rest through unchanged.
            string firstName = p.FirstName, grandFather = p.GrandFatherName;
            string? fatherName = p.FatherName, firstNameA = p.FirstNameA, fatherNameA = p.FatherNameA, grandFatherA = p.GrandFatherNameA;
            var gender = p.Gender;
            bool personChanged = true;

            switch (fieldKey)
            {
                case "FirstName": firstName = value; break;
                case "FatherName": fatherName = value; break;
                case "GrandFatherName": grandFather = value; break;
                case "FirstNameA": firstNameA = value; break;
                case "FatherNameA": fatherNameA = value; break;
                case "GrandFatherNameA": grandFatherA = value; break;
                case "Gender":
                    if (!Enum.TryParse<Gender>(value, true, out gender))
                        throw new ValidationException("value", "Gender must be Male or Female.");
                    break;
                default: personChanged = false; break;
            }
            if (personChanged)
            {
                p.Update(firstName, fatherName, grandFather, gender, p.MaritalStatusId,
                    firstNameA, fatherNameA, grandFatherA, p.NationalityId, p.PhoneNumber, p.LocationName);
                return;
            }

            // Employee-record identifiers / DOB: rebuild via Employee.Update passing the rest through.
            DateTime? dob = e.DateOfBirth;
            string? nationalId = e.NationalId, tin = e.Tin, pension = e.PensionNumber;
            switch (fieldKey)
            {
                case "DateOfBirth":
                    if (!DateTime.TryParse(value, out var d))
                        throw new ValidationException("value", "Date of birth must be a valid date.");
                    dob = d.Date;
                    break;
                case "NationalId": nationalId = value; break;
                case "Tin": tin = value; break;
                case "PensionNumber": pension = value; break;
                default: throw new ValidationException("field", "That field cannot be auto-applied.");
            }
            e.Update(e.EmployeeNumber, e.EmploymentStatus, dob, e.PlaceOfBirth, e.SpouseName, e.Email,
                nationalId, tin, pension, e.HireDate, e.PositionId, e.Salary, e.BranchId,
                e.EmploymentNature, e.ContractPeriod, e.IsProbation, e.ProbationEndDate, e.SalaryScaleId);
        }

        internal static ProfileChangeRequestDto ToDto(ProfileChangeRequest r, string? name, string? number) => new()
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = name,
            EmployeeNumber = number,
            FieldKey = r.FieldKey,
            FieldLabel = r.FieldLabel,
            Kind = r.Kind.ToString(),
            CurrentValue = r.CurrentValue,
            RequestedValue = r.RequestedValue,
            Reason = r.Reason,
            Status = r.Status.ToString(),
            Resolution = r.Resolution,
            AutoApplied = r.AutoApplied,
            SubmittedOn = r.SubmittedOn,
            ResolvedOn = r.ResolvedOn,
            ResolvedBy = r.ResolvedBy
        };
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetProfileChangeFields { Task<List<ProfileChangeFieldDto>> GetAsync(); }
    public interface ISubmitProfileChangeRequest { Task<Guid> SubmitAsync(SubmitProfileChangeRequestDto dto); }
    public interface IGetMyProfileChangeRequests { Task<List<ProfileChangeRequestDto>> GetAsync(); }
    public interface IGetPendingProfileChangeRequests { Task<ProfileChangeApprovalsDto> GetAsync(); }
    public interface IResolveProfileChangeRequest { Task ResolveAsync(Guid id, ResolveProfileChangeRequestDto dto); }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>The restricted fields the caller may raise a change request for, with current values.</summary>
    public class GetProfileChangeFields(
        IRepository<Employee> employees,
        IPerformanceVisibilityService visibility) : IGetProfileChangeFields
    {
        public async Task<List<ProfileChangeFieldDto>> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");
            var e = await employees.GetAll().AsNoTracking().Include(x => x.Person)
                .FirstOrDefaultAsync(x => x.Id == scope.EmployeeId.Value)
                ?? throw new NotFoundException(nameof(Employee), scope.EmployeeId.Value.ToString());

            return ProfileChangeCatalog.Fields.Select(kv => new ProfileChangeFieldDto
            {
                Key = kv.Key,
                Label = kv.Value.Label,
                Kind = kv.Value.Kind.ToString(),
                CurrentValue = ProfileChangeCatalog.CurrentValue(kv.Key, e, e.Person)
            }).ToList();
        }
    }

    public class SubmitProfileChangeRequest(
        IRepository<ProfileChangeRequest> repository,
        IRepository<Employee> employees,
        IPerformanceVisibilityService visibility,
        IValidator<SubmitProfileChangeRequestDto> validator,
        ILogger<SubmitProfileChangeRequest> logger) : ISubmitProfileChangeRequest
    {
        public async Task<Guid> SubmitAsync(SubmitProfileChangeRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");
            var empId = scope.EmployeeId.Value;

            var (label, kind) = ProfileChangeCatalog.Fields[dto.FieldKey];
            var e = await employees.GetAll().AsNoTracking().Include(x => x.Person)
                .FirstOrDefaultAsync(x => x.Id == empId)
                ?? throw new NotFoundException(nameof(Employee), empId.ToString());
            var current = ProfileChangeCatalog.CurrentValue(dto.FieldKey, e, e.Person);

            // One open request per field at a time.
            if (await repository.GetAll().AnyAsync(r =>
                    r.EmployeeId == empId && r.FieldKey == dto.FieldKey && r.Status == ProfileChangeStatus.Pending))
                throw new ValidationException("fieldKey", $"You already have a pending request for {label}.");

            var created = ProfileChangeRequest.Create(empId, dto.FieldKey, label, kind, current,
                dto.RequestedValue, dto.Reason, DateTime.UtcNow);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("ProfileChangeRequest {Id} ({Field}) submitted by employee {EmployeeId}",
                created.Id, dto.FieldKey, empId);
            return created.Id;
        }
    }

    public class GetMyProfileChangeRequests(
        IRepository<ProfileChangeRequest> repository,
        IPerformanceVisibilityService visibility) : IGetMyProfileChangeRequests
    {
        public async Task<List<ProfileChangeRequestDto>> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue) return [];
            var empId = scope.EmployeeId.Value;
            var rows = await repository.GetAll().AsNoTracking()
                .Where(r => r.EmployeeId == empId)
                .OrderByDescending(r => r.SubmittedOn)
                .ToListAsync();
            return rows.Select(r => ProfileChangeCatalog.ToDto(r, null, null)).ToList();
        }
    }

    /// <summary>The HR review queue (dashboard) — all PENDING requests, HR-only.</summary>
    public class GetPendingProfileChangeRequests(
        IRepository<ProfileChangeRequest> repository,
        IRepository<Employee> employees,
        IPerformanceVisibilityService visibility) : IGetPendingProfileChangeRequests
    {
        public async Task<ProfileChangeApprovalsDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) return new ProfileChangeApprovalsDto { IsApprover = false };

            var emps = employees.GetAll();
            var rows = await repository.GetAll().AsNoTracking()
                .Where(r => r.Status == ProfileChangeStatus.Pending)
                .OrderBy(r => r.SubmittedOn)
                .Select(r => new
                {
                    Request = r,
                    Name = emps.Where(e => e.Id == r.EmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    Number = emps.Where(e => e.Id == r.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault()
                })
                .ToListAsync();

            return new ProfileChangeApprovalsDto
            {
                IsApprover = true,
                Items = rows.Select(x => ProfileChangeCatalog.ToDto(x.Request, x.Name, x.Number)).ToList()
            };
        }
    }

    public class ResolveProfileChangeRequest(
        IRepository<ProfileChangeRequest> repository,
        IRepository<Employee> employees,
        IPerformanceVisibilityService visibility,
        ICurrentUserService currentUser,
        ILogger<ResolveProfileChangeRequest> logger) : IResolveProfileChangeRequest
    {
        public async Task ResolveAsync(Guid id, ResolveProfileChangeRequestDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR can decide profile change requests.");

            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(ProfileChangeRequest), id.ToString());
            if (request.Status != ProfileChangeStatus.Pending)
                throw new ValidationException("status", $"This request is already {request.Status}.");

            var by = currentUser.GetCurrentUserName();
            var isReject = string.Equals(dto.Decision, "Reject", StringComparison.OrdinalIgnoreCase);

            if (isReject)
            {
                if (string.IsNullOrWhiteSpace(dto.Resolution))
                    throw new ValidationException(nameof(dto.Resolution), "A reason is required to reject.");
                request.Reject(dto.Resolution!, by, DateTime.UtcNow);
                repository.UpdateAsync(request);
                await repository.SaveChangesAsync();
                logger.LogInformation("ProfileChangeRequest {Id} rejected by {By}", id, by);
                return;
            }

            // Approve — auto-apply identity fields; acknowledge structural ones.
            var autoApplied = false;
            if (request.Kind == ProfileChangeKind.IdentityField)
            {
                var e = await employees.GetAll().Include(x => x.Person)
                    .FirstOrDefaultAsync(x => x.Id == request.EmployeeId)
                    ?? throw new NotFoundException(nameof(Employee), request.EmployeeId.ToString());
                var p = e.Person ?? throw new ValidationException("employee", "The linked person record is missing.");
                ProfileChangeCatalog.Apply(request.FieldKey, request.RequestedValue, e, p);
                employees.UpdateAsync(e);
                autoApplied = true;
            }

            request.Approve(dto.Resolution, autoApplied, by, DateTime.UtcNow);
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();
            logger.LogInformation("ProfileChangeRequest {Id} ({Field}) approved by {By} (autoApplied={Auto})",
                id, request.FieldKey, by, autoApplied);
        }
    }
}
