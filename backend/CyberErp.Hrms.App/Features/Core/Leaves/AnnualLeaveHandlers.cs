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
        /// <summary>Days actually taken; null until the return is settled.</summary>
        public decimal? ActualLeaveDays { get; set; }
        /// <summary>The employee may confirm their return — drives the action on the row.</summary>
        public bool CanConfirmReturn { get; set; }
        /// <summary>Last approved day, so the return form can default to it.</summary>
        public DateTime? PlannedEndDate { get; set; }
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
    public interface IGetAllAnnualLeaves
    {
        Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAsync(GetAllRequest request);
        /// <summary>
        /// STRICT self-service list — ALWAYS the caller's own requests only, regardless of admin/
        /// manager privileges. Backs the Home portal's "my Annual Leave" grid so an admin-flagged
        /// self-service user (e.g. a head-office account) can never see other employees' requests.
        /// </summary>
        Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetMineAsync(GetAllRequest request);
    }

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
        Performance.IPerformanceVisibilityService visibility,
        IValidator<SaveAnnualLeaveDto> validator,
        ILeaveNotifier notifier,
        ILogger<SubmitAnnualLeave> logger) : ISubmitAnnualLeave
    {
        public async Task<Guid> SubmitAsync(SaveAnnualLeaveDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Employees submit for THEMSELVES (the screen locks the field to the signed-in employee);
            // HR admins and unit managers may still record for employees in their scope (profile tab).
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("employeeId", "You can only submit annual leave for yourself.");

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

            // The ledger row fixes employee + fiscal year — that is why the request carries no
            // LeaveType field. Annual leave has no leave type at all (see AnnualLeave).
            var ledger = await ledgers.GetAll()
                .Include(b => b.FiscalYear)
                .FirstOrDefaultAsync(b => b.Id == dto.AnnualLeaveLedgerId)
                ?? throw new NotFoundException(nameof(LeaveBalance), dto.AnnualLeaveLedgerId.ToString());

            if (ledger.EmployeeId != dto.EmployeeId)
                throw new ValidationException("annualLeaveLedgerId", "The selected ledger does not belong to this employee.");
            if (ledger.LeaveTypeId is not null)
                throw new ValidationException("annualLeaveLedgerId",
                    "The selected ledger is not an annual-leave ledger. Annual leave is charged against the annual balance only.");

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

            // The fiscal year's leave policy — probation gate AND the consecutive-day cap
            // (MaxConsecutiveDays moved here from LeaveType) both come from it.
            var setting = await leaveSettings.GetAll().FirstOrDefaultAsync(s =>
                s.FiscalYearId == ledger.FiscalYearId && s.IsActive);

            // ⚠️ ENFORCED HERE, NOT ONLY IN THE DROPDOWN. The form now lists selectable ledgers only,
            // but the ledger id arrives in the request body — a stale tab, a bookmarked payload or a
            // direct call could still name a closed year or one whose policy has been switched off.
            // There was previously NO check at all: `setting` was allowed to come back null and was
            // used only for the probation gate, so a request against a dead year sailed through
            // (logic §12.102).
            if (ledger.FiscalYear!.IsClosed)
                throw new ValidationException("annualLeaveLedgerId",
                    $"The fiscal year {ledger.FiscalYear.Name} is closed and can no longer be charged.");
            if (setting is null)
                throw new ValidationException("annualLeaveLedgerId",
                    $"There is no active annual-leave policy for {ledger.FiscalYear.Name}, so leave cannot be requested against it.");

            // Validate + cost each detail row against the ledger's fiscal year.
            foreach (var d in dto.Details)
            {
                var start = d.StartDate.Date;
                var end = d.EndDate.Date;
                if (start < fyStart || end > fyEnd)
                    throw new ValidationException("details",
                        $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} falls outside the ledger's fiscal year ({fyStart:yyyy-MM-dd}–{fyEnd:yyyy-MM-dd}).");

                var halfDay = d.LeaveUsage == AnnualLeaveUsage.HalfDay;
                // Half-day permission now lives on the fiscal year's annual policy (moved off LeaveType).
                // No policy row => fall back to allowing it, as the entitlement default does.
                if (halfDay && setting is { AllowHalfDay: false })
                    throw new ValidationException("details", "The annual leave policy does not allow half-day leave.");

                decimal leaveDays;
                try { leaveDays = await calendar.CountWorkingDaysAsync(start, end, halfDay); }
                catch (ArgumentException ex) { throw new ValidationException("details", ex.Message); }
                if (leaveDays <= 0)
                    throw new ValidationException("details", $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} contains no working days (only rest days/holidays).");

                if (setting?.MaxConsecutiveDays is int maxRun && leaveDays > maxRun)
                    throw new ValidationException("details", $"The leave policy allows at most {maxRun} consecutive days per request line.");

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

            // Probation guard (min-experience rule for the fiscal year's annual-leave policy).
            if (setting is not null && setting.MinExperienceMonths > 0 && emp.HireDate.HasValue)
            {
                var refDate = header.Details.Min(x => x.StartDate);
                var hire = emp.HireDate.Value;
                var serviceMonths = Math.Max(0, ((refDate.Year - hire.Year) * 12) + refDate.Month - hire.Month
                    - (refDate.Day < hire.Day ? 1 : 0));
                if (serviceMonths < setting.MinExperienceMonths)
                    throw new ValidationException("employeeId",
                        $"This employee has {serviceMonths} month(s) of service; {setting.MinExperienceMonths} are required for annual leave.");
            }

            // Balance check against the ledger. Annual leave always accrues, so this is unconditional
            // (it used to be gated on the leave type's accrual method).
            var available = await balanceService.GetAvailableAsync(
                ledger.EmployeeId, ledger.LeaveTypeId, ledger.FiscalYearId);
            if (header.TotalLeaveDays > available)
                throw new ValidationException("details",
                    $"Insufficient annual leave balance: requested {header.TotalLeaveDays} day(s) but only {available} available.");

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

            // AFTER the workflow exists, never before: a "current approver" recipient rule resolves
            // against the running instance step, so with no instance there is nobody to tell.
            // Never throws - the notifier swallows its own failures, because a request must not
            // fail because an e-mail could not be sent.
            await notifier.AnnualLeaveSubmittedAsync(header.Id);

            logger.LogInformation("Annual leave {Id} submitted for approval ({Days}d across {Rows} line(s))",
                header.Id, header.TotalLeaveDays, header.Details.Count);

            return header.Id;
        }
    }

    // ---- Cancel -------------------------------------------------------------
    public class CancelAnnualLeave(
        IRepository<AnnualLeaveHeader> repository,
        Performance.IPerformanceVisibilityService visibility,
        IRepository<LeaveBalance> ledgers,
        ILeaveBalanceService balanceService,
        IWorkflowGate workflowGate,
        ILogger<CancelAnnualLeave> logger) : ICancelAnnualLeave
    {
        public async Task CancelAsync(CancelAnnualLeaveDto dto)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AnnualLeaveHeader), dto.Id.ToString());
            // ⚠️ The annual header carries the LEDGER, not the employee — resolve through it.
            var ownerId = await ledgers.GetAll().Where(b => b.Id == header.AnnualLeaveLedgerId)
                .Select(b => (Guid?)b.EmployeeId).FirstOrDefaultAsync();
            if (ownerId is null || !await visibility.CanAccessEmployeeAsync(ownerId.Value))
                throw new ValidationException("id", "You can only cancel leave for yourself or your team.");

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
    public class GetAnnualLeaveById(
        IRepository<AnnualLeaveHeader> repository,
        Performance.IPerformanceVisibilityService visibility) : IGetAnnualLeaveById
    {
        public async Task<AnnualLeaveHeaderDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll().Where(r => r.Id == id).Select(AnnualLeaveMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(AnnualLeaveHeader), id.ToString());
            // HR admin, the employee themselves, or their manager (subtree) only.
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("access", "You do not have access to this annual leave request.");
            return dto;
        }
    }

    public class GetAllAnnualLeaves(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility) : IGetAllAnnualLeaves
    {
        public Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAsync(GetAllRequest request)
            => QueryAsync(request, mineOnly: false);

        public Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetMineAsync(GetAllRequest request)
            => QueryAsync(request, mineOnly: true);

        private async Task<PaginatedResponse<AnnualLeaveHeaderDto>> QueryAsync(GetAllRequest request, bool mineOnly)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            var scope = await visibility.GetScopeAsync();

            if (mineOnly)
            {
                // Self-service: ONLY the caller's own requests, no admin/manager widening. An account
                // with no linked employee sees nothing (never a broader set).
                if (scope.EmployeeId is not Guid myOwn)
                    return new PaginatedResponse<AnnualLeaveHeaderDto> { Total = 0, Data = [] };
                query = query.Where(x => x.EmployeeId == myOwn);
            }
            else if (!scope.IsAdmin)
            {
                // Role-based visibility: a manager sees their unit subtree, everyone else own only.
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
                Enum.TryParse<AnnualLeaveStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestDate)
                .Skip(skip).Take(take).Select(AnnualLeaveMapper.Projection).ToListAsync();

            return new PaginatedResponse<AnnualLeaveHeaderDto> { Total = total, Data = data };
        }
    }

    // ---- My leave balances (self-service dashboard widget) ----------------------

    /// <summary>One leave type's balance in one active fiscal year for the signed-in employee.</summary>
    public class MyAnnualLeaveBalanceItemDto
    {
        public Guid FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        /// <summary>Null on the annual row — annual leave has no leave type.</summary>
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        /// <summary>True for the annual-leave row — the "annual leave" figure for KPIs.</summary>
        public bool IsAnnual { get; set; }
        public decimal Entitled { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Adjusted { get; set; }
        public decimal Taken { get; set; }
        public decimal Available { get; set; }
    }

    public class MyAnnualLeaveBalancesDto
    {
        /// <summary>False when the account has no linked employee.</summary>
        public bool HasData { get; set; }
        /// <summary>
        /// Per leave type, per ACTIVE fiscal year (newest year first, annual types first).
        /// ⚠️ REAL BALANCES ONLY — every row here is something the employee can actually draw on.
        /// </summary>
        public List<MyAnnualLeaveBalanceItemDto> Items { get; set; } = [];
        /// <summary>
        /// Fiscal years that have an active annual-leave policy but NO generated ledger row for this
        /// employee — "HR has not run Calculate yet", not "you have no leave".
        /// </summary>
        /// <remarks>
        /// ⚠️ A SEPARATE SIGNAL, NOT A FAKE BALANCE. This used to be reported as an ordinary item
        /// carrying the policy's <c>DefaultAnnualEntitlement</c> as both Entitled and Available, so the
        /// dashboard advertised days that could not be requested: submitting annual leave requires a
        /// real <c>LeaveBalance</c> row to charge against, and there was none (logic §12.99).
        /// </remarks>
        public List<string> AwaitingGeneration { get; set; } = [];
    }

    public interface IGetMyAnnualLeaveBalance { Task<MyAnnualLeaveBalancesDto> GetAsync(); }

    /// <summary>
    /// The signed-in employee's leave balances for the dashboard cards, across every OPEN (not
    /// closed) fiscal year. Strictly self-scoped (the caller's own employee id from the visibility
    /// scope); never returns another employee's figures.
    ///
    /// <para>⚠️ The two kinds of leave span different sets of years, deliberately. ANNUAL balances are
    /// reported for every open year, because the employee holds real ledger rows in each and can draw
    /// on them. The OTHER leaves come from <see cref="IGetOtherLeaveBalances"/>, which is tied to the
    /// single current year — and must stay that way, because <c>SubmitOtherLeave</c> rejects a request
    /// whose setting is not on the active year. Widening only the dropdown would offer allocations the
    /// submit path then refuses (logic §12.101).</para>
    ///
    /// <para>Robustness (the "dashboard shows zero" class of bugs):</para>
    /// <list type="bullet">
    /// <item>Driven by the employee's OWN LeaveBalance rows — a real balance is shown even when the
    /// year's leave policy row is missing or inactive.</item>
    /// <item>Multi-type safe: one row per leave type (annual flagged and listed first) — it never
    /// binds to a single resolved "annual" type, so misconfigured or overlapping accrual methods can
    /// neither throw nor silently pick the wrong type.</item>
    /// <item>A year with an active policy but no generated row is reported through
    /// <see cref="MyAnnualLeaveBalancesDto.AwaitingGeneration"/> — NOT as an invented balance.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// ⚠️ TWO SOURCES, BECAUSE THE TWO KINDS OF LEAVE ARE STORED DIFFERENTLY. Annual leave is
    /// ledger-backed — a real <see cref="LeaveBalance"/> row per fiscal year. The OTHER leaves
    /// (maternity, paternity, …) are a static allocation on <c>OtherLeaveSetting</c> with the taken
    /// days derived from the requests themselves, and they never write a <c>LeaveBalance</c> row at
    /// all. Reading only the ledger therefore showed annual leave and nothing else, which is what made
    /// both Home cards look like annual-only widgets (logic §12.98).
    /// </remarks>
    public class GetMyAnnualLeaveBalance(
        IRepository<LeaveBalance> balances,
        IRepository<AnnualLeaveSetting> settings,
        IRepository<FiscalYear> fiscalYears,
        IRepository<LeaveType> leaveTypes,
        IGetOtherLeaveBalances otherLeaveBalances,
        Performance.IPerformanceVisibilityService visibility) : IGetMyAnnualLeaveBalance
    {
        public async Task<MyAnnualLeaveBalancesDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (scope.EmployeeId is not Guid employeeId)
                return new MyAnnualLeaveBalancesDto { HasData = false };

            // The usable fiscal years, newest first — active policy AND not closed
            // (see LeaveYearRule; §12.101 got half of this right by dropping FiscalYear.IsActive, but
            // then admitted years whose POLICY had been deactivated).
            var usableIds = LeaveYearRule.UsableFiscalYearIds(settings);
            var openYears = await fiscalYears.GetAll()
                .Where(f => usableIds.Contains(f.Id))
                .OrderByDescending(f => f.StartDate)
                .Select(f => new { f.Id, f.Name })
                .ToListAsync();
            if (openYears.Count == 0)
                return new MyAnnualLeaveBalancesDto { HasData = true };

            var yearIds = openYears.Select(f => f.Id).ToList();
            var yearNames = openYears.ToDictionary(f => f.Id, f => f.Name);
            var yearRank = openYears.Select((f, i) => (f.Id, i)).ToDictionary(x => x.Id, x => x.i);

            // The employee's balances for all open years in ONE query. Deliberately NOT joined to
            // LeaveType: the annual rows have no type (that is what makes them annual), so an inner
            // join would drop exactly the figure this widget exists to show.
            var rows = await balances.GetAll()
                .Where(b => b.EmployeeId == employeeId && yearIds.Contains(b.FiscalYearId))
                .Select(b => new
                {
                    b.FiscalYearId,
                    b.LeaveTypeId,
                    b.Entitled,
                    b.CarriedForward,
                    b.Adjusted,
                    b.Taken,
                })
                .ToListAsync();

            // Names for the typed (non-annual) rows, resolved in one lookup.
            var typeIds = rows.Where(r => r.LeaveTypeId.HasValue).Select(r => r.LeaveTypeId!.Value).Distinct().ToList();
            var typeNames = typeIds.Count == 0
                ? []
                : await leaveTypes.GetAll().Where(t => typeIds.Contains(t.Id))
                    .ToDictionaryAsync(t => t.Id, t => t.Name);

            var dto = new MyAnnualLeaveBalancesDto { HasData = true };
            foreach (var r in rows)
            {
                dto.Items.Add(new MyAnnualLeaveBalanceItemDto
                {
                    FiscalYearId = r.FiscalYearId,
                    FiscalYearName = yearNames.GetValueOrDefault(r.FiscalYearId),
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveTypeId is Guid t ? typeNames.GetValueOrDefault(t) : AnnualLeave.DisplayName,
                    IsAnnual = r.LeaveTypeId is null,
                    Entitled = r.Entitled,
                    CarriedForward = r.CarriedForward,
                    Adjusted = r.Adjusted,
                    Taken = r.Taken,
                    Available = r.Entitled + r.CarriedForward + r.Adjusted - r.Taken,
                });
            }

            // ⚠️ THIS USED TO INVENT A BALANCE. An active policy year with no generated row for the
            // employee had the policy's DefaultAnnualEntitlement reported as a normal item, Entitled
            // AND Available both set to it — so the Home tile showed "16 days available" for someone
            // whose ledger had never been calculated.
            //
            // Nothing else in the system honours that number. DefaultAnnualEntitlement is read in this
            // one place and nowhere else; SubmitAnnualLeave charges against a real LeaveBalance row and
            // throws NotFoundException without one, so the employee could not request a single one of
            // the days the dashboard promised. It also hid the thing HR actually needed to know —
            // that the entitlements had not been generated — behind a plausible-looking figure, and
            // the flat policy default is not even the right number (the accrual engine would have
            // given this employee 24 by service).
            //
            // Reported as a named signal instead, so the UI can say "not generated yet" rather than
            // quoting an unspendable balance (logic §12.99).
            var awaiting = await settings.GetAll()
                .Where(s => s.IsActive && yearIds.Contains(s.FiscalYearId))
                .Select(s => s.FiscalYearId)
                .Distinct()
                .ToListAsync();
            dto.AwaitingGeneration = awaiting
                .Where(fyId => !dto.Items.Any(i => i.FiscalYearId == fyId && i.IsAnnual))
                .OrderBy(fyId => yearRank.GetValueOrDefault(fyId, int.MaxValue))
                .Select(fyId => yearNames.GetValueOrDefault(fyId) ?? string.Empty)
                .Where(n => n.Length > 0)
                .ToList();

            // The OTHER leaves (maternity, paternity, …). Derived, not ledger-backed: the allocation
            // is a static figure on the policy and "taken" is the sum of the employee's own pending +
            // approved requests. Reused from the request form's handler rather than recomputed, so
            // the dashboard can never disagree with the entitlement dropdown about what is left.
            //
            // ⚠️ Self-scoped: employeeId comes from the visibility scope, never from the caller.
            foreach (var o in await otherLeaveBalances.GetAsync(employeeId))
            {
                // Only the active years this widget already covers; a policy on some other year is
                // not something the employee can draw on now.
                if (!yearNames.ContainsKey(o.FiscalYearId)) continue;

                // ⚠️ A ledger row wins if one somehow exists for the same type and year. Nothing
                // creates both today, but the card keys its rows on (fiscalYearId, leaveTypeId) — a
                // duplicate would be a React key collision, and two rows claiming different balances
                // for one entitlement is worse than either alone.
                if (dto.Items.Any(i => i.FiscalYearId == o.FiscalYearId && i.LeaveTypeId == o.LeaveTypeId)) continue;

                dto.Items.Add(new MyAnnualLeaveBalanceItemDto
                {
                    FiscalYearId = o.FiscalYearId,
                    FiscalYearName = o.FiscalYearName ?? yearNames.GetValueOrDefault(o.FiscalYearId),
                    LeaveTypeId = o.LeaveTypeId,
                    LeaveTypeName = o.Name,
                    IsAnnual = false,
                    Entitled = o.Allocation,
                    // No carry-forward or adjustment exists for these — the allocation resets with the
                    // fiscal year. Left at zero rather than invented so the card's arithmetic
                    // (entitled + carried + adjusted − taken) still reads true.
                    CarriedForward = 0,
                    Adjusted = 0,
                    Taken = o.Reserved,
                    Available = o.Remaining,
                });
            }

            dto.Items = dto.Items
                .OrderBy(i => yearRank.GetValueOrDefault(i.FiscalYearId, int.MaxValue))
                .ThenByDescending(i => i.IsAnnual)
                .ThenBy(i => i.LeaveTypeName)
                .ToList();
            return dto;
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
            ActualLeaveDays = r.ActualLeaveDays,
            CanConfirmReturn = r.Status == AnnualLeaveStatus.Approved,
            PlannedEndDate = r.Details.Max(d => (DateTime?)d.EndDate),
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
        ILeaveNotifier notifier,
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

            // AFTER the balance is debited, and never throwing — see OtherLeaveWorkflowHandler.
            await notifier.AnnualLeaveApprovedAsync(header.Id);
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
