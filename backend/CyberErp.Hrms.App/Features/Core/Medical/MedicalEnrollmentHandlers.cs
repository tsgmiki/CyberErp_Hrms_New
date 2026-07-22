using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Medical
{
    // ---- DTOs ---------------------------------------------------------------
    public class MedicalBeneficiaryDto
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid? EmployeeDependentId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Relationship { get; set; }
        public bool IsActive { get; set; }
    }

    public class MedicalEnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid MedicalPlanId { get; set; }
        public string? MedicalPlanName { get; set; }
        public bool CoversDependents { get; set; }
        public DateTime EnrolledOn { get; set; }
        public DateTime CoverageStart { get; set; }
        public DateTime? CoverageEnd { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public List<MedicalBeneficiaryDto> Beneficiaries { get; set; } = [];
    }

    public class SaveMedicalEnrollmentDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid MedicalPlanId { get; set; }
        public DateTime CoverageStart { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveMedicalEnrollmentDtoValidator : AbstractValidator<SaveMedicalEnrollmentDto>
    {
        public SaveMedicalEnrollmentDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.MedicalPlanId).NotEmpty();
            RuleFor(x => x.CoverageStart).NotEmpty();
        }
    }

    public class AddBeneficiaryDto
    {
        public Guid MedicalEnrollmentId { get; set; }
        public string Category { get; set; } = nameof(BeneficiaryCategory.Child);
        /// <summary>Link to an existing dependent — its name/DOB/relationship are snapshotted.</summary>
        public Guid? EmployeeDependentId { get; set; }
        /// <summary>Used when no dependent link is given.</summary>
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Relationship { get; set; }
    }

    public class MedicalEnrollmentActionDto { public DateTime? CoverageEnd { get; set; } }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveMedicalEnrollment { Task<Guid> SaveAsync(SaveMedicalEnrollmentDto dto); }
    public interface IGetEmployeeMedicalEnrollments { Task<List<MedicalEnrollmentDto>> GetAsync(Guid employeeId); }
    public interface IGetMyMedicalEnrollments { Task<List<MedicalEnrollmentDto>> GetAsync(); }
    public interface ISetMedicalEnrollmentStatus { Task SetAsync(Guid id, string status, DateTime? coverageEnd); }
    public interface IAddMedicalBeneficiary { Task<Guid> AddAsync(AddBeneficiaryDto dto); }
    public interface IRemoveMedicalBeneficiary { Task RemoveAsync(Guid beneficiaryId); }
    public interface IDeleteMedicalEnrollment { Task DeleteAsync(Guid id); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveMedicalEnrollment(
        IRepository<MedicalEnrollment> repository,
        IRepository<MedicalBeneficiary> beneficiaryRepository,
        IRepository<MedicalPlan> planRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveMedicalEnrollmentDto> validator,
        ILogger<SaveMedicalEnrollment> logger) : ISaveMedicalEnrollment
    {
        public async Task<Guid> SaveAsync(SaveMedicalEnrollmentDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical enrollments.");

            var employee = await employeeRepository.GetAll().AsNoTracking().Where(e => e.Id == dto.EmployeeId)
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber })
                .FirstOrDefaultAsync() ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            var plan = await planRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.MedicalPlanId)
                ?? throw new NotFoundException(nameof(MedicalPlan), dto.MedicalPlanId.ToString());

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(MedicalEnrollment), dto.Id.Value.ToString());
                entity.UpdateCoverage(dto.CoverageStart, dto.Remark);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            if (!plan.IsActive)
                throw new ValidationException(nameof(dto.MedicalPlanId), "The selected medical plan is inactive.");
            if (await repository.GetAll().AnyAsync(x => x.EmployeeId == dto.EmployeeId && x.MedicalPlanId == dto.MedicalPlanId && x.Status != MedicalEnrollmentStatus.Terminated))
                throw new ValidationException(nameof(dto.MedicalPlanId), "The employee already has an active enrollment in this plan.");

            var created = MedicalEnrollment.Create(dto.EmployeeId, dto.MedicalPlanId, DateTime.UtcNow.Date, dto.CoverageStart, dto.Remark);
            await repository.AddAsync(created);
            // Auto-add the employee as the primary beneficiary (via own repo — aggregate-child gotcha).
            await beneficiaryRepository.AddAsync(MedicalBeneficiary.Create(created.Id, BeneficiaryCategory.Employee, employee.Name!, null, null, "Self"));
            await repository.SaveChangesAsync();
            logger.LogInformation("Created MedicalEnrollment {Id} for Employee {EmployeeId}", created.Id, dto.EmployeeId);
            return created.Id;
        }
    }

    public class GetEmployeeMedicalEnrollments(
        IRepository<MedicalEnrollment> repository,
        IRepository<MedicalBeneficiary> beneficiaryRepository,
        IRepository<MedicalPlan> planRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeeMedicalEnrollments
    {
        public async Task<List<MedicalEnrollmentDto>> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's medical coverage.");

            var plans = planRepository.GetAll();
            var employees = employeeRepository.GetAll();
            var enrollments = await repository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.EnrolledOn)
                .Select(e => new MedicalEnrollmentDto
                {
                    Id = e.Id, EmployeeId = e.EmployeeId,
                    EmployeeName = employees.Where(x => x.Id == e.EmployeeId && x.Person != null).Select(x => x.Person!.FirstName + " " + x.Person!.GrandFatherName).FirstOrDefault(),
                    MedicalPlanId = e.MedicalPlanId,
                    MedicalPlanName = plans.Where(p => p.Id == e.MedicalPlanId).Select(p => p.Name).FirstOrDefault(),
                    CoversDependents = plans.Where(p => p.Id == e.MedicalPlanId).Select(p => p.CoversDependents).FirstOrDefault(),
                    EnrolledOn = e.EnrolledOn, CoverageStart = e.CoverageStart, CoverageEnd = e.CoverageEnd,
                    Status = e.Status.ToString(), Remark = e.Remark
                }).ToListAsync();

            var ids = enrollments.Select(e => e.Id).ToList();
            var beneficiaries = await beneficiaryRepository.GetAll().AsNoTracking()
                .Where(b => ids.Contains(b.MedicalEnrollmentId))
                .Select(b => new { b.MedicalEnrollmentId, Dto = new MedicalBeneficiaryDto { Id = b.Id, Category = b.Category.ToString(), FullName = b.FullName, EmployeeDependentId = b.EmployeeDependentId, DateOfBirth = b.DateOfBirth, Relationship = b.Relationship, IsActive = b.IsActive } })
                .ToListAsync();
            foreach (var e in enrollments)
                e.Beneficiaries = beneficiaries.Where(b => b.MedicalEnrollmentId == e.Id).Select(b => b.Dto).ToList();
            return enrollments;
        }
    }

    /// <summary>The signed-in employee's own medical enrollments (self-service claim entry).</summary>
    public class GetMyMedicalEnrollments(
        IGetEmployeeMedicalEnrollments inner,
        IPerformanceVisibilityService visibility) : IGetMyMedicalEnrollments
    {
        public async Task<List<MedicalEnrollmentDto>> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("scope", "Your account is not linked to an employee record.");
            return await inner.GetAsync(scope.EmployeeId.Value);
        }
    }

    public class SetMedicalEnrollmentStatus(
        IRepository<MedicalEnrollment> repository,
        IPerformanceVisibilityService visibility) : ISetMedicalEnrollmentStatus
    {
        public async Task SetAsync(Guid id, string status, DateTime? coverageEnd)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can change medical coverage.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(MedicalEnrollment), id.ToString());
            var st = Enum.Parse<MedicalEnrollmentStatus>(status, true);
            switch (st)
            {
                case MedicalEnrollmentStatus.Active: entity.Reactivate(); break;
                case MedicalEnrollmentStatus.Suspended: entity.Suspend(); break;
                case MedicalEnrollmentStatus.Terminated: entity.Terminate(coverageEnd ?? DateTime.UtcNow.Date); break;
            }
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class AddMedicalBeneficiary(
        IRepository<MedicalBeneficiary> beneficiaryRepository,
        IRepository<MedicalEnrollment> enrollmentRepository,
        IRepository<MedicalPlan> planRepository,
        IRepository<EmployeeDependent> dependentRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        ILogger<AddMedicalBeneficiary> logger) : IAddMedicalBeneficiary
    {
        public async Task<Guid> AddAsync(AddBeneficiaryDto dto)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(dto.MedicalEnrollmentId), "Only HR can manage beneficiaries.");

            var enrollment = await enrollmentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == dto.MedicalEnrollmentId)
                ?? throw new NotFoundException(nameof(MedicalEnrollment), dto.MedicalEnrollmentId.ToString());
            var category = Enum.Parse<BeneficiaryCategory>(dto.Category, true);

            var coversDependents = await planRepository.GetAll().Where(p => p.Id == enrollment.MedicalPlanId).Select(p => p.CoversDependents).FirstOrDefaultAsync();
            if (!coversDependents && category != BeneficiaryCategory.Employee)
                throw new ValidationException(nameof(dto.Category), "This plan does not cover dependents.");

            string fullName; DateTime? dob; string? relationship;
            if (dto.EmployeeDependentId.HasValue)
            {
                // Snapshot from the dependent — and verify it belongs to the enrolled employee.
                var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, enrollment.EmployeeId);
                var dep = await dependentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(d => d.Id == dto.EmployeeDependentId.Value)
                    ?? throw new NotFoundException(nameof(EmployeeDependent), dto.EmployeeDependentId.Value.ToString());
                if (dep.PersonId != personId)
                    throw new ValidationException(nameof(dto.EmployeeDependentId), "That dependent does not belong to this employee.");
                fullName = dep.FullName; dob = dep.DateOfBirth; relationship = dep.Relationship;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    throw new ValidationException(nameof(dto.FullName), "Provide a dependent or a name.");
                fullName = dto.FullName!; dob = dto.DateOfBirth; relationship = dto.Relationship;
            }

            var created = MedicalBeneficiary.Create(dto.MedicalEnrollmentId, category, fullName, dto.EmployeeDependentId, dob, relationship);
            await beneficiaryRepository.AddAsync(created);
            await beneficiaryRepository.SaveChangesAsync();
            logger.LogInformation("Added beneficiary {Id} to enrollment {EnrollmentId}", created.Id, dto.MedicalEnrollmentId);
            return created.Id;
        }
    }

    public class RemoveMedicalBeneficiary(
        IRepository<MedicalBeneficiary> repository,
        IPerformanceVisibilityService visibility) : IRemoveMedicalBeneficiary
    {
        public async Task RemoveAsync(Guid beneficiaryId)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(beneficiaryId), "Only HR can manage beneficiaries.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(b => b.Id == beneficiaryId)
                ?? throw new NotFoundException(nameof(MedicalBeneficiary), beneficiaryId.ToString());
            if (entity.Category == BeneficiaryCategory.Employee)
                throw new ValidationException(nameof(beneficiaryId), "The primary (employee) beneficiary cannot be removed.");
            entity.Deactivate();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class DeleteMedicalEnrollment(
        IRepository<MedicalEnrollment> repository,
        IPerformanceVisibilityService visibility) : IDeleteMedicalEnrollment
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can delete medical enrollments.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(MedicalEnrollment), id.ToString());
            repository.Delete(entity);   // beneficiaries cascade
            await repository.SaveChangesAsync();
        }
    }
}
