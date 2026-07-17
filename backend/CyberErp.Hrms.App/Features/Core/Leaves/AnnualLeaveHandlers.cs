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
    public class AnnualLeaveDetailDto
    {
        public Guid Id { get; set; }
        public string LeaveUsage { get; set; } = nameof(AnnualLeaveUsage.FullDay);
        public string? HalfDayPart { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LeaveDays { get; set; }
    }

    public class AnnualLeaveHeaderDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid AnnualLeaveLedgerId { get; set; }
        public string? FiscalYearName { get; set; }
        public decimal LedgerAvailable { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Remark { get; set; }
        public decimal TotalLeaveDays { get; set; }
        public string Status { get; set; } = nameof(AnnualLeaveStatus.Pending);
        public List<AnnualLeaveDetailDto> Details { get; set; } = [];
    }

    public class SaveAnnualLeaveDetailDto
    {
        public AnnualLeaveUsage LeaveUsage { get; set; } = AnnualLeaveUsage.FullDay;
        public HalfDayPart? HalfDayPart { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SaveAnnualLeaveDto
    {
        public Guid EmployeeId { get; set; }
        public Guid AnnualLeaveLedgerId { get; set; }
        public string? Remark { get; set; }
        public List<SaveAnnualLeaveDetailDto> Details { get; set; } = [];
    }

    public class SaveAnnualLeaveDtoValidator : AbstractValidator<SaveAnnualLeaveDto>
    {
        public SaveAnnualLeaveDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required.");
            RuleFor(x => x.AnnualLeaveLedgerId).NotEmpty().WithMessage("The annual-leave ledger is required.");
            RuleFor(x => x.Remark).MaximumLength(1000);
            RuleFor(x => x.Details).NotEmpty().WithMessage("Add at least one leave line.");
            RuleForEach(x => x.Details).ChildRules(d =>
            {
                d.RuleFor(y => y.StartDate).NotEmpty();
                d.RuleFor(y => y.EndDate).NotEmpty().GreaterThanOrEqualTo(y => y.StartDate)
                    .WithMessage("End date cannot be before start date.");
                d.RuleFor(y => y.HalfDayPart).NotNull()
                    .When(y => y.LeaveUsage == AnnualLeaveUsage.HalfDay)
                    .WithMessage("Specify Morning or Afternoon for a half day.");
            });
        }
    }

    public class CancelAnnualLeaveDto
    {
        public Guid Id { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitAnnualLeave { Task<Guid> SubmitAsync(SaveAnnualLeaveDto dto); }
    public interface ICancelAnnualLeave { Task CancelAsync(CancelAnnualLeaveDto dto); }
    public interface IGetAnnualLeaveById { Task<AnnualLeaveHeaderDto> GetAsync(Guid id); }
    public interface IGetAllAnnualLeaves { Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAsync(GetAllRequest request); }

    // ---- Submit -------------------------------------------------------------
    public class SubmitAnnualLeave(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<AnnualLeaveDetail> detailRepository,
        IRepository<LeaveBalance> ledgers,
        IRepository<Employee> employees,
        IRepository<AnnualLeaveSetting> leaveSettings,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IWorkingCalendar calendar,
        ILeaveBalanceService balanceService,
        IWorkflowService workflowService,
        IValidator<SaveAnnualLeaveDto> validator,
        ILogger<SubmitAnnualLeave> logger) : ISubmitAnnualLeave
    {
        public async Task<Guid> SubmitAsync(SaveAnnualLeaveDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // A submitted request is NEVER auto-approved: it must route through the configured
            // approval workflow and stay Pending until every stage approves. Fail loudly up front
            // when no workflow is configured — silently auto-approving (the old fallback) debited
            // the ledger on submission, and saving first would strand an unapprovable request.
            if (!await workflowDefinitions.GetAll().AnyAsync(d =>
                    d.EntityType == WorkflowEntityTypes.AnnualLeave && d.IsActive))
                throw new ValidationException("workflow",
                    "No active approval workflow is configured for Annual Leave. Ask an administrator to add one under Workflow Definitions (Process: Annual Leave) before submitting requests.");

            // Dynamic approvers (Immediate/Unit Manager) must be resolvable BEFORE the request is
            // persisted — otherwise a stuck, unapprovable Pending row would be left behind.
            await workflowService.EnsureStartableAsync(WorkflowEntityTypes.AnnualLeave, dto.EmployeeId);

            // The ledger row fixes employee + fiscal year + the annual leave type — that is why the
            // request carries no LeaveType field.
            var ledger = await ledgers.GetAll()
                .Include(b => b.LeaveType)
                .Include(b => b.FiscalYear)
                .FirstOrDefaultAsync(b => b.Id == dto.AnnualLeaveLedgerId)
                ?? throw new NotFoundException(nameof(LeaveBalance), dto.AnnualLeaveLedgerId.ToString());

            if (ledger.EmployeeId != dto.EmployeeId)
                throw new ValidationException("annualLeaveLedgerId", "The selected ledger does not belong to this employee.");
            if (ledger.LeaveType is null)
                throw new ValidationException("annualLeaveLedgerId", "The selected ledger has no leave type.");
            if (!ledger.LeaveType.IsActive)
                throw new ValidationException("annualLeaveLedgerId", $"Leave type {ledger.LeaveType.Code} is inactive.");

            var leaveType = ledger.LeaveType;
            var fyStart = ledger.FiscalYear!.StartDate.ToDateTimeUtc().Date;
            var fyEnd = ledger.FiscalYear.EndDate.ToDateTimeUtc().Date;

            var emp = await employees.GetAll().Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    e.HireDate,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : ""
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            var header = AnnualLeaveHeader.Create(ledger.EmployeeId, ledger.Id, DateTime.UtcNow, dto.Remark);

            // Validate + cost each detail row against the ledger's fiscal year.
            foreach (var d in dto.Details)
            {
                var start = d.StartDate.Date;
                var end = d.EndDate.Date;
                if (start < fyStart || end > fyEnd)
                    throw new ValidationException("details",
                        $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} falls outside the ledger's fiscal year ({fyStart:yyyy-MM-dd}–{fyEnd:yyyy-MM-dd}).");

                var halfDay = d.LeaveUsage == AnnualLeaveUsage.HalfDay;
                if (halfDay && !leaveType.AllowHalfDay)
                    throw new ValidationException("details", $"Leave type {leaveType.Code} does not allow half-day leave.");

                decimal leaveDays;
                try { leaveDays = await calendar.CountWorkingDaysAsync(start, end, halfDay); }
                catch (ArgumentException ex) { throw new ValidationException("details", ex.Message); }
                if (leaveDays <= 0)
                    throw new ValidationException("details", $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} contains no working days (only rest days/holidays).");

                if (leaveType.MaxConsecutiveDays.HasValue && leaveDays > leaveType.MaxConsecutiveDays.Value)
                    throw new ValidationException("details", $"Leave type {leaveType.Code} allows at most {leaveType.MaxConsecutiveDays.Value} consecutive days.");

                header.AddDetail(d.LeaveUsage, start, end, leaveDays, d.HalfDayPart);
            }

            // Overlap — rows within this request must not overlap each other …
            var newRows = header.Details.Select(x => (x.StartDate, x.EndDate)).ToList();
            for (var i = 0; i < newRows.Count; i++)
                for (var j = i + 1; j < newRows.Count; j++)
                    if (newRows[i].StartDate <= newRows[j].EndDate && newRows[i].EndDate >= newRows[j].StartDate)
                        throw new ValidationException("details", "Two lines in this request overlap the same dates.");

            // … nor overlap an existing pending/approved annual-leave request for this employee.
            var existing = await detailRepository.GetAll()
                .Join(repository.GetAll().Where(h => h.EmployeeId == dto.EmployeeId
                        && (h.Status == AnnualLeaveStatus.Pending || h.Status == AnnualLeaveStatus.Approved)),
                    d => d.AnnualLeaveHeaderId, h => h.Id, (d, h) => new { d.StartDate, d.EndDate })
                .ToListAsync();
            foreach (var nr in newRows)
                if (existing.Any(e => e.StartDate <= nr.EndDate && e.EndDate >= nr.StartDate))
                    throw new ValidationException("details", "A line overlaps a date range this employee already has pending or approved.");

            // Probation guard (min-experience rule for this annual-leave setting).
            var setting = await leaveSettings.GetAll().FirstOrDefaultAsync(s =>
                s.FiscalYearId == ledger.FiscalYearId && s.LeaveTypeId == leaveType.Id && s.IsActive);
            if (setting is not null && setting.MinExperienceMonths > 0 && emp.HireDate.HasValue)
            {
                var refDate = header.Details.Min(x => x.StartDate);
                var hire = emp.HireDate.Value;
                var serviceMonths = Math.Max(0, ((refDate.Year - hire.Year) * 12) + refDate.Month - hire.Month
                    - (refDate.Day < hire.Day ? 1 : 0));
                if (serviceMonths < setting.MinExperienceMonths)
                    throw new ValidationException("employeeId",
                        $"This employee has {serviceMonths} month(s) of service; {setting.MinExperienceMonths} are required for {leaveType.Code}.");
            }

            // Balance check against the ledger.
            if (leaveType.AccrualMethod != LeaveAccrualMethod.None)
            {
                var available = await balanceService.GetAvailableAsync(ledger.EmployeeId, leaveType.Id, ledger.FiscalYearId);
                if (header.TotalLeaveDays > available)
                    throw new ValidationException("details",
                        $"Insufficient {leaveType.Code} balance: requested {header.TotalLeaveDays} day(s) but only {available} available.");
            }

            await repository.AddAsync(header);
            foreach (var d in header.Details)
                if (string.IsNullOrEmpty(d.TenantId)) d.TenantId = header.TenantId;
            await repository.SaveChangesAsync();

            var name = $"{emp.First} {emp.Grand}".Trim();
            var summary = $"{name} ({emp.EmployeeNumber}): annual leave, {header.Details.Count} line(s), {header.TotalLeaveDays}d";

            // Route through the approval workflow. The request stays Pending here; the ledger is
            // debited ONLY by AnnualLeaveWorkflowHandler.OnApprovedAsync after the FINAL stage
            // approves (rejected / cancelled / pending requests never touch the balance).
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.AnnualLeave, header.Id, dto.EmployeeId, summary);

            logger.LogInformation("Annual leave {Id} submitted for approval ({Days}d across {Rows} line(s))",
                header.Id, header.TotalLeaveDays, header.Details.Count);

            return header.Id;
        }
    }

    // ---- Cancel -------------------------------------------------------------
    public class CancelAnnualLeave(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<LeaveBalance> ledgers,
        ILeaveBalanceService balanceService,
        IWorkflowGate workflowGate,
        ILogger<CancelAnnualLeave> logger) : ICancelAnnualLeave
    {
        public async Task CancelAsync(CancelAnnualLeaveDto dto)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AnnualLeaveHeader), dto.Id.ToString());

            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.AnnualLeave, header.Id);

            var wasApproved = header.HoldsBalance;
            header.Cancel();

            if (wasApproved)
            {
                var ledger = await ledgers.GetAll().FirstOrDefaultAsync(b => b.Id == header.AnnualLeaveLedgerId)
                    ?? throw new NotFoundException(nameof(LeaveBalance), header.AnnualLeaveLedgerId.ToString());
                await balanceService.ReverseAsync(ledger.EmployeeId, ledger.LeaveTypeId, ledger.FiscalYearId,
                    header.TotalLeaveDays, header.Id, "Annual leave cancelled");
            }
            else
            {
                await repository.SaveChangesAsync();
            }

            logger.LogInformation("Annual leave {Id} cancelled (balance reversed: {Reversed})", header.Id, wasApproved);
        }
    }

    // ---- Reads --------------------------------------------------------------
    public class GetAnnualLeaveById(IRepository<AnnualLeaveHeader> repository) : IGetAnnualLeaveById
    {
        public async Task<AnnualLeaveHeaderDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(r => r.Id == id).Select(AnnualLeaveMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(AnnualLeaveHeader), id.ToString());
    }

    public class GetAllAnnualLeaves(IRepository<AnnualLeaveHeader> repository) : IGetAllAnnualLeaves
    {
        public async Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<AnnualLeaveStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestDate)
                .Skip(skip).Take(take).Select(AnnualLeaveMapper.Projection).ToListAsync();

            return new PaginatedResponse<AnnualLeaveHeaderDto> { Total = total, Data = data };
        }
    }

    internal static class AnnualLeaveMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<AnnualLeaveHeader, AnnualLeaveHeaderDto>> Projection = r => new AnnualLeaveHeaderDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null && r.Employee.Person != null
                ? (r.Employee.Person.FirstName + " " + r.Employee.Person.GrandFatherName).Trim() : null,
            EmployeeNumber = r.Employee != null ? r.Employee.EmployeeNumber : null,
            AnnualLeaveLedgerId = r.AnnualLeaveLedgerId,
            FiscalYearName = r.Ledger != null && r.Ledger.FiscalYear != null ? r.Ledger.FiscalYear.Name : null,
            LedgerAvailable = r.Ledger != null ? r.Ledger.Available : 0,
            RequestDate = r.RequestDate,
            Remark = r.Remark,
            TotalLeaveDays = r.TotalLeaveDays,
            Status = r.Status.ToString(),
            Details = r.Details.OrderBy(d => d.StartDate).Select(d => new AnnualLeaveDetailDto
            {
                Id = d.Id,
                LeaveUsage = d.LeaveUsage.ToString(),
                HalfDayPart = d.HalfDayPart != null ? d.HalfDayPart.ToString() : null,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                LeaveDays = d.LeaveDays
            }).ToList()
        };
    }

    // ---- Workflow outcome handler (plugs into the generic engine) -----------
    public class AnnualLeaveWorkflowHandler(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<LeaveBalance> ledgers,
        ILeaveBalanceService balanceService,
        ILogger<AnnualLeaveWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.AnnualLeave, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (header is null || header.Status != AnnualLeaveStatus.Pending) return;

            var ledger = await ledgers.GetAll().FirstOrDefaultAsync(b => b.Id == header.AnnualLeaveLedgerId);
            if (ledger is null) return;

            header.Approve();
            await balanceService.DeductAsync(ledger.EmployeeId, ledger.LeaveTypeId, ledger.FiscalYearId,
                header.TotalLeaveDays, header.Id, "Annual leave approved");
            logger.LogInformation("Annual leave {Id} approved via workflow; balance debited {Days}d", header.Id, header.TotalLeaveDays);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (header is null || header.Status != AnnualLeaveStatus.Pending) return;

            header.Reject();
            await repository.SaveChangesAsync();
            logger.LogInformation("Annual leave {Id} rejected via workflow", header.Id);
        }
    }
}
