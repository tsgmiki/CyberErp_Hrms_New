using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Trips
{
    // ---- DTOs ---------------------------------------------------------------
    public class TripExpenseDto
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ETB";
    }

    public class TripRequestDto
    {
        public Guid Id { get; set; }
        public string TripNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string TripType { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public decimal DailyPerDiemRate { get; set; }
        public decimal PerDiemAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string Currency { get; set; } = "ETB";
        public Guid? TripBudgetId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? AdvanceDisbursedAt { get; set; }
        public string? AdvanceReference { get; set; }
        public DateTime? SettledAt { get; set; }
        public decimal? SettlementNet { get; set; }
        public string? SettlementReference { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<TripExpenseDto> Expenses { get; set; } = [];
    }

    public class RequestTripDto
    {
        public Guid? EmployeeId { get; set; }
        public string TripType { get; set; } = nameof(Dom.Entities.Core.TripType.Local);
        public string Destination { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        /// <summary>Optional; defaults to the computed per-diem when omitted.</summary>
        public decimal? AdvanceAmount { get; set; }
    }

    public class RequestTripDtoValidator : AbstractValidator<RequestTripDto>
    {
        public RequestTripDtoValidator()
        {
            RuleFor(x => x.Destination).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
            RuleFor(x => x.TripType).Must(v => Enum.TryParse<TripType>(v, true, out _)).WithMessage("Trip type must be Local or International.");
            RuleFor(x => x.AdvanceAmount).GreaterThanOrEqualTo(0).When(x => x.AdvanceAmount.HasValue);
        }
    }

    public class ApproveTripDto { public string? Note { get; set; } }
    public class RejectTripDto { public string Reason { get; set; } = string.Empty; }
    public class AddTripExpenseDto { public Guid TripRequestId { get; set; } public string Category { get; set; } = string.Empty; public string? Description { get; set; } public DateTime ExpenseDate { get; set; } public decimal Amount { get; set; } public string? Currency { get; set; } }

    public class TripBudgetUtilizationDto
    {
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal Committed { get; set; }
        public decimal Remaining => BudgetAmount - Committed;
        public int TripCount { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IRequestTrip { Task<Guid> RequestAsync(RequestTripDto dto); }
    public interface IGetTrips { Task<PaginatedResponse<TripRequestDto>> GetAsync(GetAllRequest request); }
    public interface IGetTripById { Task<TripRequestDto> GetAsync(Guid id); }
    public interface IApproveTrip { Task ApproveAsync(Guid id, string? note); }
    public interface IRejectTrip { Task RejectAsync(Guid id, string reason); }
    public interface ICancelTrip { Task CancelAsync(Guid id); }
    public interface ITransitionTrip { Task StartAsync(Guid id); Task CompleteAsync(Guid id); }
    public interface IAddTripExpense { Task<Guid> AddAsync(AddTripExpenseDto dto); }
    public interface IRemoveTripExpense { Task RemoveAsync(Guid expenseId); }
    public interface IGetTripBudgetUtilization { Task<TripBudgetUtilizationDto> GetAsync(int fiscalYear, Guid? organizationUnitId); }

    // ---- Shared resolution --------------------------------------------------
    internal static class TripShared
    {
        /// <summary>Workflow entity-type prefix ("TripRequest.Local" / "TripRequest.International").</summary>
        internal const string WorkflowPrefix = "TripRequest";

        /// <summary>Trips that reserve budget: everything except Rejected/Cancelled.</summary>
        internal static IQueryable<TripRequest> Committed(IQueryable<TripRequest> q, Guid budgetId) =>
            q.Where(t => t.TripBudgetId == budgetId && t.Status != TripRequestStatus.Rejected && t.Status != TripRequestStatus.Cancelled);
    }

    // ---- Handlers -----------------------------------------------------------
    public class RequestTrip(
        IRepository<TripRequest> repository,
        IRepository<PerDiemRate> rateRepository,
        IRepository<TripBudget> budgetRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowService workflowService,
        INumberSequenceService numberSequence,
        IValidator<RequestTripDto> validator,
        ILogger<RequestTrip> logger) : IRequestTrip
    {
        public async Task<Guid> RequestAsync(RequestTripDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            Guid employeeId;
            if (scope.IsAdmin)
                employeeId = dto.EmployeeId ?? scope.EmployeeId ?? throw new ValidationException(nameof(dto.EmployeeId), "An employee is required.");
            else
            {
                employeeId = scope.EmployeeId ?? throw new ValidationException("scope", "Your account is not linked to an employee record.");
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != employeeId)
                    throw new ValidationException(nameof(dto.EmployeeId), "You can only request your own trips.");
            }

            var emp = await employeeRepository.GetAll().AsNoTracking().Where(e => e.Id == employeeId)
                .Select(e => new { e.PositionId, GradeId = e.SalaryScale != null ? (Guid?)e.SalaryScale.JobGradeId : null })
                .FirstOrDefaultAsync() ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var tripType = Enum.Parse<TripType>(dto.TripType, true);
            var days = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;

            // Per-diem (HC267): traveller's grade × trip type × days.
            decimal dailyRate = 0m, perDiem = 0m;
            string currency = "ETB";
            if (emp.GradeId.HasValue)
            {
                var rate = await rateRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(r => r.JobGradeId == emp.GradeId.Value && r.TripType == tripType && r.IsActive);
                if (rate != null) { dailyRate = rate.DailyRate; currency = rate.Currency; perDiem = Math.Round(dailyRate * days, 2); }
            }
            var advance = dto.AdvanceAmount ?? perDiem;

            // Budget resolution (HC266): the employee's unit for the trip year, else the org-wide budget.
            Guid? unitId = emp.PositionId == Guid.Empty ? null
                : await positionRepository.GetAll().AsNoTracking().Where(p => p.Id == emp.PositionId).Select(p => (Guid?)p.OrganizationUnitId).FirstOrDefaultAsync();
            var year = dto.StartDate.Year;
            Guid? budgetId = null;
            if (unitId.HasValue)
                budgetId = await budgetRepository.GetAll().AsNoTracking().Where(b => b.FiscalYear == year && b.OrganizationUnitId == unitId.Value).Select(b => (Guid?)b.Id).FirstOrDefaultAsync();
            budgetId ??= await budgetRepository.GetAll().AsNoTracking().Where(b => b.FiscalYear == year && b.OrganizationUnitId == null).Select(b => (Guid?)b.Id).FirstOrDefaultAsync();

            // Budget gate (HC266): committed advances + this advance must fit the budget.
            if (budgetId.HasValue && advance > 0)
            {
                var budgetAmount = await budgetRepository.GetAll().AsNoTracking().Where(b => b.Id == budgetId.Value).Select(b => b.Amount).FirstAsync();
                var committed = await TripShared.Committed(repository.GetAll().AsNoTracking(), budgetId.Value).SumAsync(t => (decimal?)t.AdvanceAmount) ?? 0m;
                if (committed + advance > budgetAmount)
                    throw new ValidationException(nameof(dto.AdvanceAmount), $"The advance ({advance:N2}) exceeds the remaining travel budget ({budgetAmount - committed:N2}).");
            }

            var tripNumber = $"TR-{await numberSequence.NextAsync("TripRequest"):D5}";
            var created = TripRequest.Create(tripNumber, employeeId, tripType, dto.Destination, dto.Purpose,
                dto.StartDate, dto.EndDate, days, dailyRate, perDiem, advance, currency, budgetId, DateTime.UtcNow.Date);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();

            var name = await employeeRepository.GetAll().Where(e => e.Id == employeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefaultAsync();
            var wfKey = tripType == TripType.International ? WorkflowEntityTypes.TripInternational : WorkflowEntityTypes.TripLocal;
            await workflowService.StartIfDefinedAsync(wfKey, created.Id, employeeId, $"Trip {tripNumber} — {name} — {dto.Destination}");

            logger.LogInformation("Requested Trip {TripNumber} for Employee {EmployeeId}", tripNumber, employeeId);
            return created.Id;
        }
    }

    public class GetTrips(
        IRepository<TripRequest> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetTrips
    {
        public async Task<PaginatedResponse<TripRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                query = query.Where(x => x.EmployeeId == (scope.EmployeeId ?? Guid.Empty));  // own only

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TripRequestStatus>(request.Status, true, out var st))
                query = query.Where(x => x.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.TripNumber.Contains(term) || x.Destination.Contains(term));
            }

            var total = await query.CountAsync();
            var employees = employeeRepository.GetAll();
            var data = await query.OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new TripRequestDto
                {
                    Id = x.Id, TripNumber = x.TripNumber, EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    TripType = x.TripType.ToString(), Destination = x.Destination, Purpose = x.Purpose,
                    StartDate = x.StartDate, EndDate = x.EndDate, Days = x.Days, DailyPerDiemRate = x.DailyPerDiemRate,
                    PerDiemAmount = x.PerDiemAmount, AdvanceAmount = x.AdvanceAmount, Currency = x.Currency, TripBudgetId = x.TripBudgetId,
                    Status = x.Status.ToString(), Resolution = x.Resolution, RequestDate = x.RequestDate,
                    AdvanceDisbursedAt = x.AdvanceDisbursedAt, AdvanceReference = x.AdvanceReference,
                    SettledAt = x.SettledAt, SettlementNet = x.SettlementNet, SettlementReference = x.SettlementReference
                }).ToListAsync();
            return new PaginatedResponse<TripRequestDto> { Total = total, Data = data };
        }
    }

    public class GetTripById(
        IRepository<TripRequest> repository,
        IRepository<TripExpense> expenseRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetTripById
    {
        public async Task<TripRequestDto> GetAsync(Guid id)
        {
            var employees = employeeRepository.GetAll();
            var dto = await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new TripRequestDto
                {
                    Id = x.Id, TripNumber = x.TripNumber, EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    TripType = x.TripType.ToString(), Destination = x.Destination, Purpose = x.Purpose,
                    StartDate = x.StartDate, EndDate = x.EndDate, Days = x.Days, DailyPerDiemRate = x.DailyPerDiemRate,
                    PerDiemAmount = x.PerDiemAmount, AdvanceAmount = x.AdvanceAmount, Currency = x.Currency, TripBudgetId = x.TripBudgetId,
                    Status = x.Status.ToString(), Resolution = x.Resolution, RequestDate = x.RequestDate,
                    AdvanceDisbursedAt = x.AdvanceDisbursedAt, AdvanceReference = x.AdvanceReference,
                    SettledAt = x.SettledAt, SettlementNet = x.SettlementNet, SettlementReference = x.SettlementReference
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TripRequest), id.ToString());

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this trip.");

            dto.Expenses = await expenseRepository.GetAll().AsNoTracking().Where(e => e.TripRequestId == id).OrderBy(e => e.ExpenseDate)
                .Select(e => new TripExpenseDto { Id = e.Id, Category = e.Category, Description = e.Description, ExpenseDate = e.ExpenseDate, Amount = e.Amount, Currency = e.Currency })
                .ToListAsync();
            dto.TotalExpenses = dto.Expenses.Sum(e => e.Amount);
            return dto;
        }
    }

    public class ApproveTrip(
        IRepository<TripRequest> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IApproveTrip
    {
        public async Task ApproveAsync(Guid id, string? note)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can approve trips.");
            await workflowGate.EnsureNoRunningAsync(TripShared.WorkflowPrefix, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            if (entity.Status != TripRequestStatus.Requested) throw new ValidationException(nameof(id), "Only a requested trip can be approved.");
            entity.Approve(note);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class RejectTrip(
        IRepository<TripRequest> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IRejectTrip
    {
        public async Task RejectAsync(Guid id, string reason)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can reject trips.");
            if (string.IsNullOrWhiteSpace(reason)) throw new ValidationException(nameof(reason), "A rejection reason is required.");
            await workflowGate.EnsureNoRunningAsync(TripShared.WorkflowPrefix, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            if (entity.Status != TripRequestStatus.Requested) throw new ValidationException(nameof(id), "Only a requested trip can be rejected.");
            entity.Reject(reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class CancelTrip(
        IRepository<TripRequest> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : ICancelTrip
    {
        public async Task CancelAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            if (!scope.IsAdmin && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(id), "You can only cancel your own trips.");
            await workflowGate.EnsureNoRunningAsync(TripShared.WorkflowPrefix, id);
            entity.Cancel();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class TransitionTrip(
        IRepository<TripRequest> repository,
        IPerformanceVisibilityService visibility) : ITransitionTrip
    {
        private async Task<TripRequest> LoadOwnedAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            if (!scope.IsAdmin && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(id), "You can only update your own trips.");
            return entity;
        }

        public async Task StartAsync(Guid id)
        {
            var entity = await LoadOwnedAsync(id);
            entity.Start();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }

        public async Task CompleteAsync(Guid id)
        {
            var entity = await LoadOwnedAsync(id);
            entity.Complete();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class AddTripExpense(
        IRepository<TripExpense> expenseRepository,
        IRepository<TripRequest> tripRepository,
        IPerformanceVisibilityService visibility) : IAddTripExpense
    {
        public async Task<Guid> AddAsync(AddTripExpenseDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            var trip = await tripRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TripRequestId)
                ?? throw new NotFoundException(nameof(TripRequest), dto.TripRequestId.ToString());
            if (!scope.IsAdmin && trip.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(dto.TripRequestId), "You can only add expenses to your own trips.");
            if (trip.Status is not (TripRequestStatus.Approved or TripRequestStatus.InProgress or TripRequestStatus.Completed))
                throw new ValidationException(nameof(dto.TripRequestId), "Expenses can only be added to an approved, in-progress or completed trip.");
            if (string.IsNullOrWhiteSpace(dto.Category)) throw new ValidationException(nameof(dto.Category), "Category is required.");
            if (dto.Amount < 0) throw new ValidationException(nameof(dto.Amount), "Amount cannot be negative.");

            var created = TripExpense.Create(dto.TripRequestId, dto.Category, dto.Description, dto.ExpenseDate, dto.Amount, dto.Currency ?? trip.Currency);
            await expenseRepository.AddAsync(created);
            await expenseRepository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class RemoveTripExpense(
        IRepository<TripExpense> expenseRepository,
        IRepository<TripRequest> tripRepository,
        IPerformanceVisibilityService visibility) : IRemoveTripExpense
    {
        public async Task RemoveAsync(Guid expenseId)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await expenseRepository.GetByIdAsync(expenseId) ?? throw new NotFoundException(nameof(TripExpense), expenseId.ToString());
            var ownerId = await tripRepository.GetAll().Where(t => t.Id == entity.TripRequestId).Select(t => t.EmployeeId).FirstOrDefaultAsync();
            if (!scope.IsAdmin && ownerId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(expenseId), "You can only remove expenses from your own trips.");
            expenseRepository.Delete(entity);
            await expenseRepository.SaveChangesAsync();
        }
    }

    /// <summary>HC266 — computed travel-budget utilization (committed advances vs the allocation).</summary>
    public class GetTripBudgetUtilization(
        IRepository<TripBudget> budgetRepository,
        IRepository<TripRequest> tripRepository,
        IPerformanceVisibilityService visibility) : IGetTripBudgetUtilization
    {
        public async Task<TripBudgetUtilizationDto> GetAsync(int fiscalYear, Guid? organizationUnitId)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can view budget utilization.");
            var budget = organizationUnitId.HasValue
                ? await budgetRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(b => b.FiscalYear == fiscalYear && b.OrganizationUnitId == organizationUnitId.Value)
                : await budgetRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(b => b.FiscalYear == fiscalYear && b.OrganizationUnitId == null);
            var dto = new TripBudgetUtilizationDto { FiscalYear = fiscalYear, OrganizationUnitId = organizationUnitId, BudgetAmount = budget?.Amount ?? 0m };
            if (budget != null)
            {
                var committed = TripShared.Committed(tripRepository.GetAll().AsNoTracking(), budget.Id);
                dto.Committed = await committed.SumAsync(t => (decimal?)t.AdvanceAmount) ?? 0m;
                dto.TripCount = await committed.CountAsync();
            }
            return dto;
        }
    }
}
