using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeMovementDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }

        public Guid? FromPositionId { get; set; }
        public string? FromPositionName { get; set; }
        public Guid? FromJobGradeId { get; set; }
        public string? FromJobGradeName { get; set; }
        public decimal? FromSalary { get; set; }
        public string? FromBranchName { get; set; }

        public Guid? ToPositionId { get; set; }
        public string? ToPositionName { get; set; }
        public Guid? ToJobGradeId { get; set; }
        public string? ToJobGradeName { get; set; }
        public decimal? ToSalary { get; set; }
        public string? ToBranchName { get; set; }

        public string? Reason { get; set; }
        public string? Remark { get; set; }
        public DateTime? ExecutedAt { get; set; }
    }

    public class SaveEmployeeMovementDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string MovementType { get; set; } = nameof(Dom.Entities.Core.MovementType.Transfer);
        public DateTime EffectiveDate { get; set; }
        public Guid? ToPositionId { get; set; }
        public Guid? ToJobGradeId { get; set; }
        public decimal? ToSalary { get; set; }
        public string? Reason { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveEmployeeMovementDtoValidator : AbstractValidator<SaveEmployeeMovementDto>
    {
        public SaveEmployeeMovementDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.EffectiveDate).NotEmpty().WithMessage("Effective date is required.");
            RuleFor(x => x.MovementType).NotEmpty()
                .Must(v => Enum.TryParse<MovementType>(v, true, out _))
                .WithMessage("MovementType must be one of: Transfer, Promotion, Demotion.");
            RuleFor(x => x.ToPositionId).NotEmpty()
                .When(x => string.Equals(x.MovementType, nameof(Dom.Entities.Core.MovementType.Transfer), StringComparison.OrdinalIgnoreCase))
                .WithMessage("A transfer requires a target position.");
            RuleFor(x => x)
                .Must(x => x.ToPositionId.HasValue || x.ToJobGradeId.HasValue || x.ToSalary.HasValue)
                .When(x => !string.Equals(x.MovementType, nameof(Dom.Entities.Core.MovementType.Transfer), StringComparison.OrdinalIgnoreCase))
                .WithMessage("A promotion/demotion must change at least the position, grade or salary.")
                .OverridePropertyName("toJobGradeId");
            RuleFor(x => x.ToSalary).GreaterThanOrEqualTo(0).When(x => x.ToSalary.HasValue);
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeMovement { Task<Guid> SaveAsync(SaveEmployeeMovementDto dto); }
    public interface IGetEmployeeMovements { Task<List<EmployeeMovementDto>> GetAsync(Guid employeeId); }
    public interface IExecuteEmployeeMovement { Task ExecuteAsync(Guid id); }
    public interface ICancelEmployeeMovement { Task CancelAsync(Guid id); }
    public interface IDeleteEmployeeMovement { Task DeleteAsync(Guid id); }

    internal static class MovementShared
    {
        /// <summary>The target position must exist and be open — unless the employee already holds it.</summary>
        internal static async Task EnsureTargetPositionAvailableAsync(
            IRepository<Position> positions, Guid targetPositionId, Guid? currentPositionId)
        {
            var pos = await positions.GetAll()
                .Where(p => p.Id == targetPositionId)
                .Select(p => new { p.IsVacant })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Position), targetPositionId.ToString());

            if (!pos.IsVacant && targetPositionId != currentPositionId)
                throw new ValidationException("toPositionId", "The target position is already occupied.");
        }
    }

    // ---- Save (create / correct while pending) ---------------------------------
    public class SaveEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<JobGrade> jobGradeRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IValidator<SaveEmployeeMovementDto> validator,
        ILogger<SaveEmployeeMovement> logger) : ISaveEmployeeMovement
    {
        public async Task<Guid> SaveAsync(SaveEmployeeMovementDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.PositionId, e.JobGradeId, e.Salary, e.BranchId, e.EmployeeNumber,
                    FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
                    LastName = e.Person != null ? e.Person.GrandFatherName : string.Empty
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            if (dto.ToPositionId.HasValue)
                await MovementShared.EnsureTargetPositionAvailableAsync(positionRepository, dto.ToPositionId.Value, employee.PositionId);
            if (dto.ToJobGradeId.HasValue && !await jobGradeRepository.GetAll().AnyAsync(g => g.Id == dto.ToJobGradeId.Value))
                throw new NotFoundException(nameof(JobGrade), dto.ToJobGradeId.Value.ToString());

            var type = Enum.Parse<MovementType>(dto.MovementType, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(EmployeeMovement), dto.Id.Value.ToString());
                if (entity.Status != MovementStatus.Pending)
                    throw new ValidationException("status", "Only pending movements can be edited.");
                await workflowGate.EnsureNoRunningAsync("EmployeeMovement", entity.Id);

                entity.Update(type, dto.EffectiveDate, dto.ToPositionId, dto.ToJobGradeId, dto.ToSalary, dto.Reason, dto.Remark);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeMovement {Id}", entity.Id);
                return entity.Id;
            }

            // The From* snapshot freezes the employee's placement at recording time.
            var created = EmployeeMovement.Create(
                dto.EmployeeId, type, dto.EffectiveDate,
                employee.PositionId, employee.JobGradeId, employee.Salary, employee.BranchId,
                dto.ToPositionId, dto.ToJobGradeId, dto.ToSalary,
                dto.Reason, dto.Remark);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeMovement {Id} ({Type}) for Employee {EmployeeId}", created.Id, type, dto.EmployeeId);

            // Route the action through its approval chain when one is configured (e.g.
            // "EmployeeMovement.Transfer"); without a definition the module operates directly.
            await workflowService.StartIfDefinedAsync(
                $"EmployeeMovement.{type}", created.Id, dto.EmployeeId,
                $"{type} — {employee.FirstName} {employee.LastName} ({employee.EmployeeNumber})");

            return created.Id;
        }
    }

    // ---- Execute (apply to the employee master) ---------------------------------
    public class ExecuteEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IWorkflowGate workflowGate,
        ILogger<ExecuteEmployeeMovement> logger) : IExecuteEmployeeMovement
    {
        public async Task ExecuteAsync(Guid id)
        {
            // While an approval is running, execution only happens through the workflow's final
            // approval (which completes the instance before invoking this handler).
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            if (movement.Status != MovementStatus.Pending)
                throw new ValidationException("status", $"A {movement.Status} movement can no longer be executed.");

            // Tracked + branch-filtered: a branch admin can only execute for employees they see.
            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == movement.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), movement.EmployeeId.ToString());

            var oldPositionId = employee.PositionId;
            var changePosition = movement.ToPositionId.HasValue && movement.ToPositionId != employee.PositionId;
            Guid? newBranchId = employee.BranchId;

            if (changePosition)
            {
                await MovementShared.EnsureTargetPositionAvailableAsync(positionRepository, movement.ToPositionId!.Value, employee.PositionId);

                // The branch follows the new position (same rule as direct assignment).
                newBranchId = await positionRepository.GetAll()
                    .Where(p => p.Id == movement.ToPositionId.Value)
                    .Select(p => p.BranchId)
                    .FirstOrDefaultAsync();
            }

            employee.ApplyMovement(changePosition, movement.ToPositionId, newBranchId, movement.ToJobGradeId, movement.ToSalary);
            employeeRepository.UpdateAsync(employee);

            if (changePosition)
            {
                await EmployeeShared.RecomputePositionVacancyAsync(oldPositionId, employee.Id, positionRepository, employeeRepository);
                await EmployeeShared.MarkPositionOccupiedAsync(movement.ToPositionId, positionRepository);
            }

            movement.MarkExecuted(newBranchId);
            repository.UpdateAsync(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Executed EmployeeMovement {Id} ({Type}) for Employee {EmployeeId}",
                movement.Id, movement.MovementType, movement.EmployeeId);
        }
    }

    // ---- Cancel -----------------------------------------------------------------
    public class CancelEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ILogger<CancelEmployeeMovement> logger) : ICancelEmployeeMovement
    {
        public async Task CancelAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, movement.EmployeeId);
            if (movement.Status != MovementStatus.Pending)
                throw new ValidationException("status", $"A {movement.Status} movement can no longer be cancelled.");

            movement.Cancel();
            repository.UpdateAsync(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled EmployeeMovement {Id}", id);
        }
    }

    // ---- Delete (drafts only — executed history is immutable) --------------------
    public class DeleteEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ILogger<DeleteEmployeeMovement> logger) : IDeleteEmployeeMovement
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, movement.EmployeeId);
            if (movement.Status == MovementStatus.Completed)
                throw new ValidationException("status", "Executed movements are part of the employee's history and cannot be deleted.");

            repository.Delete(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeMovement {Id}", id);
        }
    }

    // ---- Get (movement history per employee) --------------------------------------
    public class GetEmployeeMovements(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<JobGrade> jobGradeRepository,
        IRepository<Branch> branchRepository) : IGetEmployeeMovements
    {
        public async Task<List<EmployeeMovementDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);

            // Name lookups use the unfiltered sets: display-by-id is safe and keeps historical
            // cross-branch snapshots readable (same approach as RelatedEmployeeName).
            var positions = positionRepository.GetAllWithoutTenantFilter();
            var grades = jobGradeRepository.GetAllWithoutTenantFilter();
            var branches = branchRepository.GetAllWithoutTenantFilter();

            return await repository.GetAll()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EffectiveDate)
                .Select(x => new EmployeeMovementDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    MovementType = x.MovementType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    FromPositionId = x.FromPositionId,
                    FromPositionName = positions.Where(p => p.Id == x.FromPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    FromJobGradeId = x.FromJobGradeId,
                    FromJobGradeName = grades.Where(g => g.Id == x.FromJobGradeId).Select(g => g.Name).FirstOrDefault(),
                    FromSalary = x.FromSalary,
                    FromBranchName = branches.Where(b => b.Id == x.FromBranchId).Select(b => b.Name).FirstOrDefault(),
                    ToPositionId = x.ToPositionId,
                    ToPositionName = positions.Where(p => p.Id == x.ToPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    ToJobGradeId = x.ToJobGradeId,
                    ToJobGradeName = grades.Where(g => g.Id == x.ToJobGradeId).Select(g => g.Name).FirstOrDefault(),
                    ToSalary = x.ToSalary,
                    ToBranchName = branches.Where(b => b.Id == x.ToBranchId).Select(b => b.Name).FirstOrDefault(),
                    Reason = x.Reason,
                    Remark = x.Remark,
                    ExecutedAt = x.ExecutedAt
                })
                .ToListAsync();
        }
    }
}
