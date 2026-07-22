using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class LeaveRequestLineDto
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DayPart { get; set; } = nameof(LeaveDayPart.Full);
        public decimal WorkingDays { get; set; }
    }

    public class LeaveRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid FiscalYearId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public decimal TotalWorkingDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = nameof(LeaveRequestStatus.Pending);
        public string? DecisionComment { get; set; }
        public string? CancelReason { get; set; }
        public List<LeaveRequestLineDto> Lines { get; set; } = [];
    }

    public class SaveLeaveRequestLineDto
    {
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveDayPart DayPart { get; set; } = LeaveDayPart.Full;
    }

    public class SaveLeaveRequestDto
    {
        public Guid EmployeeId { get; set; }
        public string? Reason { get; set; }
        public List<SaveLeaveRequestLineDto> Lines { get; set; } = [];
    }

    public class SaveLeaveRequestDtoValidator : AbstractValidator<SaveLeaveRequestDto>
    {
        public SaveLeaveRequestDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required.");
            RuleFor(x => x.Reason).MaximumLength(1000);
            RuleFor(x => x.Lines).NotEmpty().WithMessage("Add at least one leave line.");
            RuleForEach(x => x.Lines).ChildRules(l =>
            {
                l.RuleFor(y => y.LeaveTypeId).NotEmpty().WithMessage("Leave type is required.");
                l.RuleFor(y => y.StartDate).NotEmpty();
                l.RuleFor(y => y.EndDate).NotEmpty().GreaterThanOrEqualTo(y => y.StartDate)
                    .WithMessage("End date cannot be before start date.");
            });
        }
    }

    public class CancelLeaveRequestDto
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitLeaveRequest { Task<Guid> SubmitAsync(SaveLeaveRequestDto dto); }
    public interface ICancelLeaveRequest { Task CancelAsync(CancelLeaveRequestDto dto); }
    public interface IGetLeaveRequestById { Task<LeaveRequestDto> GetAsync(Guid id); }
    public interface IGetAllLeaveRequests { Task<PaginatedResponse<LeaveRequestDto>> GetAsync(GetAllRequest request); }

    /// <summary>Groups a request's lines by leave type and applies each type's summed days to the ledger.</summary>
    internal static class LeaveBalanceApplier
    {
        internal static async Task DeductAsync(LeaveRequest request, ILeaveBalanceService balanceService, string reason)
        {
            foreach (var g in request.Lines.GroupBy(l => l.LeaveTypeId))
                await balanceService.DeductAsync(request.EmployeeId, g.Key, request.FiscalYearId,
                    g.Sum(l => l.WorkingDays), request.Id, reason);
        }

        internal static async Task ReverseAsync(LeaveRequest request, ILeaveBalanceService balanceService, string reason)
        {
            foreach (var g in request.Lines.GroupBy(l => l.LeaveTypeId))
                await balanceService.ReverseAsync(request.EmployeeId, g.Key, request.FiscalYearId,
                    g.Sum(l => l.WorkingDays), request.Id, reason);
        }

        internal static void StampLineTenant(LeaveRequest request)
        {
            foreach (var line in request.Lines)
                if (string.IsNullOrEmpty(line.TenantId)) line.TenantId = request.TenantId;
        }
    }

    // ---- Submit -------------------------------------------------------------
    public class SubmitLeaveRequest(
        IRepository<LeaveRequest> repository,
        IRepository<LeaveRequestLine> lineRepository,
        IRepository<LeaveType> leaveTypes,
        IRepository<Employee> employees,
        IRepository<AnnualLeaveSetting> leaveSettings,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IFiscalYearResolver fiscalYearResolver,
        IWorkingCalendar calendar,
        ILeaveBalanceService balanceService,
        IWorkflowService workflowService,
        IValidator<SaveLeaveRequestDto> validator,
        ILogger<SubmitLeaveRequest> logger) : ISubmitLeaveRequest
    {
        public async Task<Guid> SubmitAsync(SaveLeaveRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // A submitted request is NEVER auto-approved: it must route through the configured
            // approval workflow and stay Pending until every stage approves (deduction happens
            // only in the workflow handler after the final stage). Fail loudly when unconfigured.
            if (!await workflowDefinitions.GetAll().AnyAsync(d =>
                    d.EntityType == WorkflowEntityTypes.LeaveRequest && d.IsActive))
                throw new ValidationException("workflow",
                    "No active approval workflow is configured for Leave Requests. Ask an administrator to add one under Workflow Definitions (Process: Leave Request) before submitting requests.");

            // Dynamic approvers (Immediate/Unit Manager) must be resolvable BEFORE the request is
            // persisted — otherwise a stuck, unapprovable Pending row would be left behind.
            await workflowService.EnsureStartableAsync(WorkflowEntityTypes.LeaveRequest, dto.EmployeeId);

            var emp = await employees.GetAll().Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    e.HireDate,
                    Gender = e.Person != null ? (Gender?)e.Person.Gender : null,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : ""
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            // The whole request is charged to the fiscal year of its earliest line; every line must fall
            // within that year (balances are per-year — split into separate requests otherwise).
            var earliestStart = dto.Lines.Min(l => l.StartDate.Date);
            var fiscalYear = await fiscalYearResolver.ResolveForDateAsync(earliestStart);
            var fyStart = fiscalYear.StartDate.ToDateTimeUtc().Date;
            var fyEnd = fiscalYear.EndDate.ToDateTimeUtc().Date;

            var typeIds = dto.Lines.Select(l => l.LeaveTypeId).Distinct().ToList();
            var typeById = await leaveTypes.GetAll().Where(t => typeIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id);

            var request = LeaveRequest.Create(dto.EmployeeId, fiscalYear.Id, DateTime.UtcNow, dto.Reason);

            // Validate + cost each line.
            foreach (var line in dto.Lines)
            {
                var start = line.StartDate.Date;
                var end = line.EndDate.Date;
                if (start < fyStart || end > fyEnd)
                    throw new ValidationException("lines",
                        $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} falls outside the fiscal year ({fyStart:yyyy-MM-dd}–{fyEnd:yyyy-MM-dd}). Submit separate requests per fiscal year.");

                if (!typeById.TryGetValue(line.LeaveTypeId, out var leaveType))
                    throw new NotFoundException(nameof(LeaveType), line.LeaveTypeId.ToString());
                if (!leaveType.IsActive)
                    throw new ValidationException("lines", $"Leave type {leaveType.Code} is inactive.");

                if (leaveType.GenderEligibility != LeaveGenderEligibility.Any && emp.Gender.HasValue)
                {
                    var required = leaveType.GenderEligibility == LeaveGenderEligibility.Male ? Gender.Male : Gender.Female;
                    if (emp.Gender.Value != required)
                        throw new ValidationException("lines", $"Leave type {leaveType.Code} is restricted to {required} employees.");
                }

                var halfDay = line.DayPart != LeaveDayPart.Full;
                if (halfDay && !leaveType.AllowHalfDay)
                    throw new ValidationException("lines", $"Leave type {leaveType.Code} does not allow half-day leave.");

                decimal workingDays;
                try { workingDays = await calendar.CountWorkingDaysAsync(start, end, halfDay); }
                catch (ArgumentException ex) { throw new ValidationException("lines", ex.Message); }
                if (workingDays <= 0)
                    throw new ValidationException("lines", $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} contains no working days (only rest days/holidays).");

                if (leaveType.MaxConsecutiveDays.HasValue && workingDays > leaveType.MaxConsecutiveDays.Value)
                    throw new ValidationException("lines", $"Leave type {leaveType.Code} allows at most {leaveType.MaxConsecutiveDays.Value} consecutive days.");

                request.AddLine(line.LeaveTypeId, start, end, line.DayPart, workingDays);
            }

            // Overlap — lines within the request must not overlap each other …
            var newLines = request.Lines.Select(l => (l.StartDate, l.EndDate)).ToList();
            for (var i = 0; i < newLines.Count; i++)
                for (var j = i + 1; j < newLines.Count; j++)
                    if (newLines[i].StartDate <= newLines[j].EndDate && newLines[i].EndDate >= newLines[j].StartDate)
                        throw new ValidationException("lines", "Two lines in this request overlap the same dates.");

            // … nor overlap an existing pending/approved request's lines.
            var existing = await lineRepository.GetAll()
                .Join(repository.GetAll().Where(h => h.EmployeeId == dto.EmployeeId
                        && (h.Status == LeaveRequestStatus.Pending || h.Status == LeaveRequestStatus.Approved)),
                    l => l.LeaveRequestId, h => h.Id, (l, h) => new { l.StartDate, l.EndDate })
                .ToListAsync();
            foreach (var nl in newLines)
                if (existing.Any(e => e.StartDate <= nl.EndDate && e.EndDate >= nl.StartDate))
                    throw new ValidationException("lines", "A line overlaps a date range this employee already has pending or approved.");

            // Probation guard + balance check, aggregated per leave type.
            foreach (var g in request.Lines.GroupBy(l => l.LeaveTypeId))
            {
                var leaveType = typeById[g.Key];
                var setting = await leaveSettings.GetAll().FirstOrDefaultAsync(s =>
                    s.FiscalYearId == fiscalYear.Id && s.LeaveTypeId == g.Key && s.IsActive);
                if (setting is not null && setting.MinExperienceMonths > 0)
                {
                    var refDate = g.Min(l => l.StartDate);
                    var serviceMonths = emp.HireDate.HasValue
                        ? Math.Max(0, ((refDate.Year - emp.HireDate.Value.Year) * 12) + refDate.Month - emp.HireDate.Value.Month
                            - (refDate.Day < emp.HireDate.Value.Day ? 1 : 0))
                        : 0;
                    if (serviceMonths < setting.MinExperienceMonths)
                        throw new ValidationException("employeeId",
                            $"This employee has {serviceMonths} month(s) of service; {setting.MinExperienceMonths} are required for {leaveType.Code}.");
                }

                if (leaveType.AccrualMethod != LeaveAccrualMethod.None)
                {
                    var requested = g.Sum(l => l.WorkingDays);
                    var available = await balanceService.GetAvailableAsync(dto.EmployeeId, g.Key, fiscalYear.Id);
                    if (requested > available)
                        throw new ValidationException("lines",
                            $"Insufficient {leaveType.Code} balance: requested {requested} day(s) but only {available} available.");
                }
            }

            await repository.AddAsync(request);
            LeaveBalanceApplier.StampLineTenant(request);
            await repository.SaveChangesAsync();

            var name = $"{emp.First} {emp.Grand}".Trim();
            var summary = $"{name} ({emp.EmployeeNumber}): {request.Lines.Count} line(s), {request.TotalWorkingDays}d";

            // Route through the approval workflow. The request stays Pending here; the ledger is
            // debited ONLY by LeaveRequestWorkflowHandler.OnApprovedAsync after the FINAL stage
            // approves (rejected / cancelled / pending requests never touch the balance).
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.LeaveRequest, request.Id, dto.EmployeeId, summary);

            logger.LogInformation("Leave request {Id} submitted for approval ({Days}d across {Lines} line(s))",
                request.Id, request.TotalWorkingDays, request.Lines.Count);

            return request.Id;
        }
    }

    // ---- Cancel -------------------------------------------------------------
    public class CancelLeaveRequest(
        IRepository<LeaveRequest> repository,
        ILeaveBalanceService balanceService,
        IWorkflowGate workflowGate,
        ILogger<CancelLeaveRequest> logger) : ICancelLeaveRequest
    {
        public async Task CancelAsync(CancelLeaveRequestDto dto)
        {
            var request = await repository.GetAll().Include(r => r.Lines).FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(LeaveRequest), dto.Id.ToString());

            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.LeaveRequest, request.Id);

            var wasApproved = request.HoldsBalance;
            request.Cancel(dto.Reason);

            if (wasApproved)
                await LeaveBalanceApplier.ReverseAsync(request, balanceService, "Leave cancelled");
            else
                await repository.SaveChangesAsync();

            logger.LogInformation("Leave request {Id} cancelled (balance reversed: {Reversed})", request.Id, wasApproved);
        }
    }

    // ---- Reads --------------------------------------------------------------
    public class GetLeaveRequestById(
        IRepository<LeaveRequest> repository,
        Performance.IPerformanceVisibilityService visibility) : IGetLeaveRequestById
    {
        public async Task<LeaveRequestDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll().Where(r => r.Id == id).Select(LeaveRequestMapper.Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(LeaveRequest), id.ToString());
            // HR admin, the employee themselves, or their manager (subtree) only.
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("access", "You do not have access to this leave request.");
            return dto;
        }
    }

    public class GetAllLeaveRequests(
        IRepository<LeaveRequest> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility) : IGetAllLeaveRequests
    {
        public async Task<PaginatedResponse<LeaveRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // Role-based visibility: HR admin sees all, a manager their unit subtree, else own only.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    var emps = employeeRepository.GetAll();
                    query = query.Where(x => x.EmployeeId == myEmp ||
                        emps.Any(e => e.Id == x.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(x => x.EmployeeId == myEmp);
                }
            }

            if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<LeaveRequestStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.SubmittedDate)
                .Skip(skip).Take(take)
                .Select(LeaveRequestMapper.Projection)
                .ToListAsync();

            return new PaginatedResponse<LeaveRequestDto> { Total = total, Data = data };
        }
    }

    internal static class LeaveRequestMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<LeaveRequest, LeaveRequestDto>> Projection = r => new LeaveRequestDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null && r.Employee.Person != null
                ? (r.Employee.Person.FirstName + " " + r.Employee.Person.GrandFatherName).Trim() : null,
            EmployeeNumber = r.Employee != null ? r.Employee.EmployeeNumber : null,
            FiscalYearId = r.FiscalYearId,
            SubmittedDate = r.SubmittedDate,
            TotalWorkingDays = r.TotalWorkingDays,
            Reason = r.Reason,
            Status = r.Status.ToString(),
            DecisionComment = r.DecisionComment,
            CancelReason = r.CancelReason,
            Lines = r.Lines.OrderBy(l => l.StartDate).Select(l => new LeaveRequestLineDto
            {
                Id = l.Id,
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeCode = l.LeaveType != null ? l.LeaveType.Code : null,
                LeaveTypeName = l.LeaveType != null ? l.LeaveType.Name : null,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                DayPart = l.DayPart.ToString(),
                WorkingDays = l.WorkingDays
            }).ToList()
        };
    }

    // ---- Workflow outcome handler (plugs into the generic engine) -----------
    public class LeaveRequestWorkflowHandler(
        IRepository<LeaveRequest> repository,
        ILeaveBalanceService balanceService,
        ILogger<LeaveRequestWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.LeaveRequest, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().Include(r => r.Lines).FirstOrDefaultAsync(x => x.Id == entityId);
            if (request is null || request.Status != LeaveRequestStatus.Pending) return;

            request.Approve();
            await LeaveBalanceApplier.DeductAsync(request, balanceService, "Leave approved");
            logger.LogInformation("Leave request {Id} approved via workflow; balance debited {Days}d", request.Id, request.TotalWorkingDays);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (request is null || request.Status != LeaveRequestStatus.Pending) return;

            request.Reject();
            await repository.SaveChangesAsync();
            logger.LogInformation("Leave request {Id} rejected via workflow", request.Id);
        }
    }
}
