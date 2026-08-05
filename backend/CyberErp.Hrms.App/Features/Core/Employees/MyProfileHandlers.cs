using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    /// <summary>
    /// The signed-in employee's own profile (ESS). Editable personal/contact fields plus a
    /// read-only block of employment/organization/statutory data an employee may see but never
    /// change from self-service.
    /// </summary>
    public class MyProfileDto
    {
        public Guid EmployeeId { get; set; }
        // Read-only identity / employment context
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? PositionTitle { get; set; }
        public string? DepartmentName { get; set; }
        public string? BranchName { get; set; }
        public string? NationalId { get; set; }
        public string? Tin { get; set; }
        public string? PensionNumber { get; set; }
        public string? PhotoUrl { get; set; }
        // Editable personal / contact fields
        public string? PhoneNumber { get; set; }
        public string? LocationName { get; set; }
        public string MaritalStatus { get; set; } = nameof(Dom.Entities.Core.MaritalStatus.Single);
        public string? Email { get; set; }
        public string? SpouseName { get; set; }
        public string? PlaceOfBirth { get; set; }
    }

    /// <summary>The strictly self-editable field set — org/pay/statutory fields are intentionally absent.</summary>
    public class UpdateMyProfileDto
    {
        public string? PhoneNumber { get; set; }
        public string? LocationName { get; set; }
        public string MaritalStatus { get; set; } = nameof(Dom.Entities.Core.MaritalStatus.Single);
        public string? Email { get; set; }
        public string? SpouseName { get; set; }
        public string? PlaceOfBirth { get; set; }
    }

    public class UpdateMyProfileDtoValidator : AbstractValidator<UpdateMyProfileDto>
    {
        public UpdateMyProfileDtoValidator()
        {
            RuleFor(x => x.MaritalStatus)
                .Must(v => Enum.TryParse<MaritalStatus>(v, true, out _))
                .WithMessage("Marital status must be Single, Married, Divorced or Widowed.");
            RuleFor(x => x.Email).EmailAddress().MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.PhoneNumber).MaximumLength(50);
            RuleFor(x => x.LocationName).MaximumLength(300);
            RuleFor(x => x.SpouseName).MaximumLength(200);
            RuleFor(x => x.PlaceOfBirth).MaximumLength(200);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetMyProfile { Task<MyProfileDto> GetAsync(); }
    public interface IUpdateMyProfile { Task UpdateAsync(UpdateMyProfileDto dto); }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>ESS profile read — strictly the caller's own record (gate mirrors GetMyCompensation).</summary>
    public class GetMyProfile(
        IRepository<Employee> employees,
        IRepository<Position> positions,
        IRepository<PositionClass> positionClasses,
        IRepository<OrganizationUnit> units,
        IRepository<Branch> branches,
        IPerformanceVisibilityService visibility) : IGetMyProfile
    {
        public async Task<MyProfileDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");

            var empId = scope.EmployeeId.Value;
            var e = await employees.GetAll().AsNoTracking().Include(x => x.Person)
                .FirstOrDefaultAsync(x => x.Id == empId)
                ?? throw new NotFoundException(nameof(Employee), empId.ToString());

            string? positionTitle = null, departmentName = null;
            if (e.PositionId.HasValue)
            {
                var pos = await positions.GetAll().AsNoTracking()
                    .Where(p => p.Id == e.PositionId.Value)
                    .Select(p => new { p.PositionClassId, p.OrganizationUnitId })
                    .FirstOrDefaultAsync();
                if (pos is not null)
                {
                    positionTitle = await positionClasses.GetAll().AsNoTracking()
                        .Where(c => c.Id == pos.PositionClassId).Select(c => c.Title).FirstOrDefaultAsync();
                    departmentName = await units.GetAll().AsNoTracking()
                        .Where(u => u.Id == pos.OrganizationUnitId).Select(u => u.Name).FirstOrDefaultAsync();
                }
            }
            var branchName = e.BranchId.HasValue
                ? await branches.GetAll().AsNoTracking()
                    .Where(b => b.Id == e.BranchId.Value).Select(b => b.Name).FirstOrDefaultAsync()
                : null;

            var p = e.Person;
            return new MyProfileDto
            {
                EmployeeId = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FullName = p?.FullName ?? string.Empty,
                Gender = p?.Gender.ToString(),
                DateOfBirth = e.DateOfBirth,
                HireDate = e.HireDate,
                EmploymentStatus = e.EmploymentStatus.ToString(),
                PositionTitle = positionTitle,
                DepartmentName = departmentName,
                BranchName = branchName,
                NationalId = e.NationalId,
                Tin = e.Tin,
                PensionNumber = e.PensionNumber,
                PhotoUrl = e.PhotoUrl,
                PhoneNumber = p?.PhoneNumber,
                LocationName = p?.LocationName,
                MaritalStatus = (p?.MaritalStatusId ?? Dom.Entities.Core.MaritalStatus.Single).ToString(),
                Email = e.Email,
                SpouseName = e.SpouseName,
                PlaceOfBirth = e.PlaceOfBirth
            };
        }
    }

    /// <summary>
    /// ESS profile update — writes ONLY the safe personal/contact fields of the caller's own record;
    /// every organization/pay/statutory field is preserved verbatim. The gate guarantees an employee
    /// can never reach another record, and the restricted DTO guarantees they can never change their
    /// position, pay, branch or identifiers (unlike the full-admin PUT Employee).
    /// </summary>
    public class UpdateMyProfile(
        IRepository<Employee> employees,
        IPerformanceVisibilityService visibility,
        IValidator<UpdateMyProfileDto> validator,
        ILogger<UpdateMyProfile> logger) : IUpdateMyProfile
    {
        public async Task UpdateAsync(UpdateMyProfileDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");

            var empId = scope.EmployeeId.Value;
            var e = await employees.GetAll().Include(x => x.Person)
                .FirstOrDefaultAsync(x => x.Id == empId)
                ?? throw new NotFoundException(nameof(Employee), empId.ToString());
            var p = e.Person
                ?? throw new ValidationException("employee", "The linked person record is missing.");

            var marital = Enum.Parse<MaritalStatus>(dto.MaritalStatus, true);

            // Person-owned contact fields — names/gender/nationality kept as-is (HR-controlled).
            p.Update(p.FirstName, p.FatherName, p.GrandFatherName, p.Gender, marital,
                p.FirstNameA, p.FatherNameA, p.GrandFatherNameA, p.NationalityId,
                Trim(dto.PhoneNumber), Trim(dto.LocationName));

            // Employee-record personal fields — every employment/pay/statutory field is passed
            // through unchanged, so this endpoint can only touch email/spouse/place-of-birth.
            e.Update(e.EmployeeNumber, e.EmploymentStatus, e.DateOfBirth, Trim(dto.PlaceOfBirth),
                Trim(dto.SpouseName), Trim(dto.Email), e.NationalId, e.Tin, e.PensionNumber,
                e.HireDate, e.PositionId, e.Salary, e.BranchId, e.EmploymentNature,
                e.ContractPeriod, e.IsProbation, e.ProbationEndDate, e.SalaryScaleId);

            employees.UpdateAsync(e);
            await employees.SaveChangesAsync();
            logger.LogInformation("Employee {EmployeeId} updated their own profile (self-service)", empId);
        }

        private static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }
}
