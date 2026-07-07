using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class TerminationClearanceDto
    {
        public Guid Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ClearedBy { get; set; }
        public DateTime? ClearedAt { get; set; }
    }

    public class EmployeeTerminationDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string TerminationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime NoticeDate { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime? SettledAt { get; set; }
        public bool AwaitingWorkflow { get; set; }
        public List<TerminationClearanceDto> Clearances { get; set; } = [];
    }

    public class SaveEmployeeTerminationDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string TerminationType { get; set; } = nameof(Dom.Entities.Core.TerminationType.Voluntary);
        public DateTime NoticeDate { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class UpdateTerminationClearanceDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = nameof(ClearanceStatus.Pending);
        public string? Note { get; set; }
    }

    public class SaveEmployeeTerminationDtoValidator : AbstractValidator<SaveEmployeeTerminationDto>
    {
        public SaveEmployeeTerminationDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.TerminationType).NotEmpty()
                .Must(v => Enum.TryParse<TerminationType>(v, true, out _))
                .WithMessage("TerminationType must be Voluntary or Involuntary.");
            RuleFor(x => x.NoticeDate).NotEmpty().WithMessage("Notice date is required.");
            RuleFor(x => x.LastWorkingDate).NotEmpty().WithMessage("Last working date is required.")
                .GreaterThanOrEqualTo(x => x.NoticeDate)
                .WithMessage("Last working date cannot be before the notice date.");
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Remarks).MaximumLength(2000);
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeTermination { Task<Guid> SaveAsync(SaveEmployeeTerminationDto dto); }
    public interface IGetEmployeeTerminations { Task<List<EmployeeTerminationDto>> GetAsync(Guid employeeId); }
    public interface IUpdateTerminationClearance { Task UpdateAsync(UpdateTerminationClearanceDto dto); }
    public interface IFinalizeEmployeeTermination { Task FinalizeAsync(Guid id); }
    public interface ICancelEmployeeTermination { Task CancelAsync(Guid id); }
    public interface IDeleteEmployeeTermination { Task DeleteAsync(Guid id); }

    internal static class TerminationShared
    {
        /// <summary>Default departmental checklist (IT assets, Store property, Finance loans/advances).</summary>
        internal static readonly (string Department, string Description)[] DefaultChecklist =
        [
            ("IT", "Company assets returned (laptop, badge, system accounts revoked)"),
            ("Store", "Company property returned (uniforms, tools, equipment)"),
            ("Finance", "Outstanding loans, advances and payroll items settled"),
        ];

        /// <summary>
        /// Opens the clearance phase: status transition + default checklist rows. New children
        /// need two manual touches: the tenant stamp (repository stamps roots only) and an explicit
        /// Add — otherwise <c>context.Update(root)</c> would mark them Modified (their app-generated
        /// keys look "existing") and SaveChanges would fail with a concurrency exception.
        /// </summary>
        internal static async Task BeginClearanceAsync(
            EmployeeTermination termination, IRepository<TerminationClearance> clearanceRepository)
        {
            var existing = termination.Clearances.Select(c => c.Id).ToHashSet();
            termination.BeginClearance(DefaultChecklist);
            foreach (var item in termination.Clearances.Where(c => !existing.Contains(c.Id)))
            {
                if (string.IsNullOrEmpty(item.TenantId))
                    item.TenantId = termination.TenantId;
                await clearanceRepository.AddAsync(item);
            }
        }

        internal static async Task<EmployeeTermination> GetWithClearancesAsync(
            IRepository<EmployeeTermination> repository, Guid id)
        {
            return await repository.GetAll()
                    .Include(t => t.Clearances)
                    .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
        }
    }

    // ---- Save (initiate / correct while initiated) -------------------------------
    public class SaveEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<TerminationClearance> clearanceRepository,
        IRepository<Employee> employeeRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IValidator<SaveEmployeeTerminationDto> validator,
        ILogger<SaveEmployeeTermination> logger) : ISaveEmployeeTermination
    {
        public async Task<Guid> SaveAsync(SaveEmployeeTerminationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmploymentStatus, e.EmployeeNumber,
                    FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
                    LastName = e.Person != null ? e.Person.GrandFatherName : string.Empty
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            var type = Enum.Parse<TerminationType>(dto.TerminationType, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(EmployeeTermination), dto.Id.Value.ToString());
                if (entity.Status != TerminationStatus.Initiated)
                    throw new ValidationException("status", "Only initiated terminations can be edited.");

                entity.Update(type, dto.NoticeDate, dto.LastWorkingDate, dto.Reason, dto.Remarks);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeTermination {Id}", entity.Id);
                return entity.Id;
            }

            if (employee.EmploymentStatus == EmploymentStatus.Terminated)
                throw new ValidationException("employeeId", "This employee is already terminated.");
            if (await repository.GetAll().AnyAsync(t =>
                    t.EmployeeId == dto.EmployeeId &&
                    (t.Status == TerminationStatus.Initiated || t.Status == TerminationStatus.ClearanceInProgress)))
                throw new ValidationException("employeeId", "An active termination case already exists for this employee.");

            var created = EmployeeTermination.Create(dto.EmployeeId, type, dto.NoticeDate, dto.LastWorkingDate, dto.Reason, dto.Remarks);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeTermination {Id} ({Type}) for Employee {EmployeeId}", created.Id, type, dto.EmployeeId);

            // Route through the approval chain when configured; approval opens the clearance
            // phase. Without a workflow definition the clearance checklist opens immediately.
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.Termination, created.Id, dto.EmployeeId,
                $"Termination ({type}) — {employee.FirstName} {employee.LastName} ({employee.EmployeeNumber})");

            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.Termination, created.Id))
            {
                await TerminationShared.BeginClearanceAsync(created, clearanceRepository);
                repository.UpdateAsync(created);
                await repository.SaveChangesAsync();
            }

            return created.Id;
        }
    }

    // ---- Get (per employee, with checklist) ---------------------------------------
    public class GetEmployeeTerminations(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate) : IGetEmployeeTerminations
    {
        public async Task<List<EmployeeTerminationDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);

            var rows = await repository.GetAll()
                .Include(t => t.Clearances)
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var result = new List<EmployeeTerminationDto>();
            foreach (var t in rows)
            {
                result.Add(new EmployeeTerminationDto
                {
                    Id = t.Id,
                    EmployeeId = t.EmployeeId,
                    TerminationType = t.TerminationType.ToString(),
                    Status = t.Status.ToString(),
                    NoticeDate = t.NoticeDate,
                    LastWorkingDate = t.LastWorkingDate,
                    Reason = t.Reason,
                    Remarks = t.Remarks,
                    SettledAt = t.SettledAt,
                    AwaitingWorkflow = t.Status == TerminationStatus.Initiated
                        && await workflowGate.HasRunningAsync(WorkflowEntityTypes.Termination, t.Id),
                    Clearances = t.Clearances
                        .OrderBy(c => c.Department)
                        .Select(c => new TerminationClearanceDto
                        {
                            Id = c.Id,
                            Department = c.Department,
                            Description = c.Description,
                            Status = c.Status.ToString(),
                            Note = c.Note,
                            ClearedBy = c.ClearedBy,
                            ClearedAt = c.ClearedAt
                        })
                        .ToList()
                });
            }
            return result;
        }
    }

    // ---- Clearance updates -----------------------------------------------------
    public class UpdateTerminationClearance(
        IRepository<TerminationClearance> repository,
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Employee> employeeRepository,
        ICurrentUserService currentUser,
        ILogger<UpdateTerminationClearance> logger) : IUpdateTerminationClearance
    {
        public async Task UpdateAsync(UpdateTerminationClearanceDto dto)
        {
            if (!Enum.TryParse<ClearanceStatus>(dto.Status, true, out var status))
                throw new ValidationException("status", "Status must be Pending, Cleared or Blocked.");

            var item = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.Id)
                ?? throw new NotFoundException(nameof(TerminationClearance), dto.Id.ToString());

            var termination = await terminationRepository.GetAll().FirstOrDefaultAsync(t => t.Id == item.TerminationId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), item.TerminationId.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status != TerminationStatus.ClearanceInProgress)
                throw new ValidationException("status", $"Clearances can only be updated while clearance is in progress (case is {termination.Status}).");

            item.SetStatus(status, dto.Note, currentUser.GetCurrentUserName());
            repository.UpdateAsync(item);
            await repository.SaveChangesAsync();
            logger.LogInformation("Clearance {Id} ({Department}) set to {Status}", item.Id, item.Department, status);
        }
    }

    // ---- Finalize (system automations) -------------------------------------------
    public class FinalizeEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        ILogger<FinalizeEmployeeTermination> logger) : IFinalizeEmployeeTermination
    {
        public async Task FinalizeAsync(Guid id)
        {
            var termination = await TerminationShared.GetWithClearancesAsync(repository, id);
            if (termination.Status != TerminationStatus.ClearanceInProgress)
                throw new ValidationException("status", $"Only a clearance-in-progress termination can be finalized (current: {termination.Status}).");

            var pending = termination.Clearances
                .Where(c => c.Status != ClearanceStatus.Cleared)
                .Select(c => c.Department)
                .ToList();
            if (pending.Count > 0)
                throw new ValidationException("clearances",
                    $"All clearances must be 'Cleared' before settlement. Outstanding: {string.Join(", ", pending)}.");

            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == termination.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), termination.EmployeeId.ToString());

            // System automations: end the employment, decouple the seat and reopen it.
            var oldPositionId = employee.PositionId;
            employee.Terminate();
            employeeRepository.UpdateAsync(employee);
            await EmployeeShared.RecomputePositionVacancyAsync(oldPositionId, employee.Id, positionRepository, employeeRepository);

            termination.MarkSettled();
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Settled EmployeeTermination {Id}: employee {EmployeeId} terminated, position {PositionId} reopened",
                id, employee.Id, oldPositionId);
        }
    }

    // ---- Cancel / Delete ----------------------------------------------------------
    public class CancelEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ILogger<CancelEmployeeTermination> logger) : ICancelEmployeeTermination
    {
        public async Task CancelAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, id);

            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status is TerminationStatus.Settled or TerminationStatus.Cancelled)
                throw new ValidationException("status", $"A {termination.Status} termination can no longer be cancelled.");

            termination.Cancel();
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled EmployeeTermination {Id}", id);
        }
    }

    public class DeleteEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ILogger<DeleteEmployeeTermination> logger) : IDeleteEmployeeTermination
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, id);

            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status == TerminationStatus.Settled)
                throw new ValidationException("status", "Settled terminations are part of the employee's history and cannot be deleted.");

            repository.Delete(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeTermination {Id}", id);
        }
    }
}
