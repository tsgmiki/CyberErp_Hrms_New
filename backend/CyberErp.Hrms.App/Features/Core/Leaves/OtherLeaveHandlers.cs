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
    // ---- Setting DTOs -------------------------------------------------------
    public class OtherLeaveSettingDto
    {
        public Guid Id { get; set; }
        public Guid FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        /// <summary>The LeaveType master relationship (replaces the former free-text name).</summary>
        public Guid LeaveTypeId { get; set; }
        /// <summary>Display name — projected from the related LeaveType.</summary>
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = nameof(GenderEligibility.All);
        public decimal StandardDays { get; set; }
        public decimal ManagerialDays { get; set; }
        public bool IsLumpSum { get; set; }
        /// <summary>WorkingDays (skip holidays/weekends) or CalendarDays (count them).</summary>
        public string DayCounting { get; set; } = nameof(LeaveDayCounting.WorkingDays);
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }

    public class SaveOtherLeaveSettingDto
    {
        public Guid? Id { get; set; }
        public Guid FiscalYearId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string Gender { get; set; } = nameof(GenderEligibility.All);
        public decimal StandardDays { get; set; }
        public decimal ManagerialDays { get; set; }
        public bool IsLumpSum { get; set; }
        public string DayCounting { get; set; } = nameof(LeaveDayCounting.WorkingDays);
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }

    public class SaveOtherLeaveSettingDtoValidator : AbstractValidator<SaveOtherLeaveSettingDto>
    {
        public SaveOtherLeaveSettingDtoValidator()
        {
            RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("The fiscal year is required.");
            RuleFor(x => x.LeaveTypeId).NotEmpty().WithMessage("The leave type is required.");
            RuleFor(x => x.Gender).NotEmpty()
                .Must(v => Enum.TryParse<GenderEligibility>(v, true, out _))
                .WithMessage("Gender must be All, Female or Male.");
            RuleFor(x => x.DayCounting).NotEmpty()
                .Must(v => Enum.TryParse<LeaveDayCounting>(v, true, out _))
                .WithMessage("Day counting must be WorkingDays or CalendarDays.");
            RuleFor(x => x.StandardDays).GreaterThan(0).WithMessage("Standard days must be positive.");
            RuleFor(x => x.ManagerialDays).GreaterThan(0).WithMessage("Managerial days must be positive.");
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    // ---- Request DTOs -------------------------------------------------------
    public class OtherLeaveDetailDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LeaveDays { get; set; }
    }

    public class OtherLeaveHeaderDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid OtherLeaveSettingId { get; set; }
        public string? LeaveName { get; set; }
        public string? FiscalYearName { get; set; }
        public bool IsLumpSum { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Remark { get; set; }
        public decimal TotalLeaveDays { get; set; }
        public string Status { get; set; } = nameof(OtherLeaveStatus.Pending);
        public List<OtherLeaveDetailDto> Details { get; set; } = [];
    }

    public class SaveOtherLeaveDetailDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SaveOtherLeaveDto
    {
        public Guid EmployeeId { get; set; }
        public Guid OtherLeaveSettingId { get; set; }
        public string? Remark { get; set; }
        public List<SaveOtherLeaveDetailDto> Details { get; set; } = [];
    }

    public class SaveOtherLeaveDtoValidator : AbstractValidator<SaveOtherLeaveDto>
    {
        public SaveOtherLeaveDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required.");
            RuleFor(x => x.OtherLeaveSettingId).NotEmpty().WithMessage("The leave type is required.");
            RuleFor(x => x.Remark).MaximumLength(1000);
            RuleFor(x => x.Details).NotEmpty().WithMessage("Add at least one leave line.");
            RuleForEach(x => x.Details).ChildRules(d =>
            {
                d.RuleFor(y => y.StartDate).NotEmpty();
                d.RuleFor(y => y.EndDate).NotEmpty().GreaterThanOrEqualTo(y => y.StartDate)
                    .WithMessage("End date cannot be before start date.");
            });
        }
    }

    public class CancelOtherLeaveDto
    {
        public Guid Id { get; set; }
    }

    /// <summary>One selectable entitlement on the request form: the employee's static allocation
    /// for the ACTIVE fiscal year with what is already reserved (pending + approved).</summary>
    public class OtherLeaveBalanceDto
    {
        public Guid OtherLeaveSettingId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? FiscalYearName { get; set; }
        public string Gender { get; set; } = nameof(GenderEligibility.All);
        public bool IsLumpSum { get; set; }
        /// <summary>WorkingDays (holidays/weekends skipped) or CalendarDays (counted).</summary>
        public string DayCounting { get; set; } = nameof(LeaveDayCounting.WorkingDays);
        public decimal Allocation { get; set; }
        public decimal Reserved { get; set; }
        public decimal Remaining { get; set; }
    }

    /// <summary>Server-computed end date of a lump-sum block (allocation working days from start).</summary>
    public class LumpSumEndDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LeaveDays { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveOtherLeaveSetting { Task<Guid> SaveAsync(SaveOtherLeaveSettingDto dto); }
    public interface IDeleteOtherLeaveSetting { Task DeleteAsync(Guid id); }
    public interface IGetOtherLeaveSettingById { Task<OtherLeaveSettingDto> GetAsync(Guid id); }
    public interface IGetAllOtherLeaveSettings { Task<PaginatedResponse<OtherLeaveSettingDto>> GetAsync(GetAllRequest request); }
    public interface IGetOtherLeaveBalances { Task<List<OtherLeaveBalanceDto>> GetAsync(Guid employeeId); }
    public interface IGetLumpSumEndDate { Task<LumpSumEndDto> GetAsync(Guid employeeId, Guid otherLeaveSettingId, DateTime startDate); }
    public interface ISubmitOtherLeave { Task<Guid> SubmitAsync(SaveOtherLeaveDto dto); }
    public interface ICancelOtherLeave { Task CancelAsync(CancelOtherLeaveDto dto); }
    public interface IGetOtherLeaveById { Task<OtherLeaveHeaderDto> GetAsync(Guid id); }
    public interface IGetAllOtherLeaves { Task<PaginatedResponse<OtherLeaveHeaderDto>> GetAsync(GetAllRequest request); }

    // ---- Setting CRUD -------------------------------------------------------
    public class SaveOtherLeaveSetting(
        IRepository<OtherLeaveSetting> repository,
        IRepository<FiscalYear> fiscalYears,
        IRepository<LeaveType> leaveTypes,
        IValidator<SaveOtherLeaveSettingDto> validator,
        ILogger<SaveOtherLeaveSetting> logger) : ISaveOtherLeaveSetting
    {
        public async Task<Guid> SaveAsync(SaveOtherLeaveSettingDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await fiscalYears.GetAll().AnyAsync(f => f.Id == dto.FiscalYearId))
                throw new NotFoundException(nameof(FiscalYear), dto.FiscalYearId.ToString());
            if (!await leaveTypes.GetAll().AnyAsync(t => t.Id == dto.LeaveTypeId))
                throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId.ToString());
            if (await repository.GetAll().AnyAsync(s =>
                    s.FiscalYearId == dto.FiscalYearId && s.LeaveTypeId == dto.LeaveTypeId && s.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(OtherLeaveSetting), nameof(dto.LeaveTypeId), dto.LeaveTypeId.ToString());

            var gender = Enum.Parse<GenderEligibility>(dto.Gender, true);
            var counting = Enum.Parse<LeaveDayCounting>(dto.DayCounting, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(OtherLeaveSetting), dto.Id.Value.ToString());
                entity.Update(dto.FiscalYearId, dto.LeaveTypeId, gender, dto.StandardDays, dto.ManagerialDays,
                    dto.IsLumpSum, counting, dto.IsActive, dto.Description);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = OtherLeaveSetting.Create(dto.FiscalYearId, dto.LeaveTypeId, gender, dto.StandardDays,
                dto.ManagerialDays, dto.IsLumpSum, counting, dto.IsActive, dto.Description);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created OtherLeaveSetting {Id} (leave type {LeaveTypeId})", created.Id, created.LeaveTypeId);
            return created.Id;
        }
    }

    public class DeleteOtherLeaveSetting(
        IRepository<OtherLeaveSetting> repository,
        IRepository<OtherLeaveHeader> requests) : IDeleteOtherLeaveSetting
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(OtherLeaveSetting), id.ToString());
            if (await requests.GetAll().AnyAsync(r => r.OtherLeaveSettingId == id))
                throw new ValidationException(nameof(id),
                    "Leave requests reference this setting. Deactivate it instead of deleting.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetOtherLeaveSettingById(IRepository<OtherLeaveSetting> repository) : IGetOtherLeaveSettingById
    {
        public async Task<OtherLeaveSettingDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(OtherLeaveMapper.SettingProjection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(OtherLeaveSetting), id.ToString());
    }

    public class GetAllOtherLeaveSettings(IRepository<OtherLeaveSetting> repository) : IGetAllOtherLeaveSettings
    {
        public async Task<PaginatedResponse<OtherLeaveSettingDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.LeaveType != null && x.LeaveType.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.LeaveType!.Name)
                .Skip(skip).Take(take).Select(OtherLeaveMapper.SettingProjection).ToListAsync();
            return new PaginatedResponse<OtherLeaveSettingDto> { Total = total, Data = data };
        }
    }

    // ---- Balances (the request form's entitlement dropdown) ------------------
    public class GetOtherLeaveBalances(
        IRepository<OtherLeaveSetting> settings,
        IRepository<OtherLeaveHeader> requests,
        IRepository<Employee> employees) : IGetOtherLeaveBalances
    {
        public async Task<List<OtherLeaveBalanceDto>> GetAsync(Guid employeeId)
        {
            var emp = await employees.GetAll().Where(e => e.Id == employeeId)
                .Select(e => new { e.IsManagerial, Gender = e.Person != null ? (Gender?)e.Person.Gender : null })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            // Only the ACTIVE fiscal year's settings apply, filtered by the employee's gender.
            var applicable = await settings.GetAll()
                .Where(x => x.IsActive && x.FiscalYear != null && x.FiscalYear.IsActive)
                .Select(OtherLeaveMapper.SettingProjection)
                .ToListAsync();
            applicable = applicable.Where(x =>
                    x.Gender == nameof(GenderEligibility.All)
                    || (emp.Gender == Gender.Female && x.Gender == nameof(GenderEligibility.Female))
                    || (emp.Gender == Gender.Male && x.Gender == nameof(GenderEligibility.Male)))
                .ToList();
            if (applicable.Count == 0) return [];

            var settingIds = applicable.Select(x => x.Id).ToList();
            var reserved = await requests.GetAll()
                .Where(r => r.EmployeeId == employeeId && settingIds.Contains(r.OtherLeaveSettingId)
                    && (r.Status == OtherLeaveStatus.Pending || r.Status == OtherLeaveStatus.Approved))
                .GroupBy(r => r.OtherLeaveSettingId)
                .Select(g => new { SettingId = g.Key, Days = g.Sum(x => x.TotalLeaveDays) })
                .ToDictionaryAsync(x => x.SettingId, x => x.Days);

            return applicable.Select(x =>
            {
                var allocation = emp.IsManagerial ? x.ManagerialDays : x.StandardDays;
                var taken = reserved.GetValueOrDefault(x.Id);
                return new OtherLeaveBalanceDto
                {
                    OtherLeaveSettingId = x.Id,
                    Name = x.Name,
                    FiscalYearName = x.FiscalYearName,
                    Gender = x.Gender,
                    IsLumpSum = x.IsLumpSum,
                    DayCounting = x.DayCounting,
                    Allocation = allocation,
                    Reserved = taken,
                    Remaining = Math.Max(0, allocation - taken)
                };
            }).ToList();
        }
    }

    // ---- Lump-sum end-date helper (guided form) ------------------------------
    public class GetLumpSumEndDate(
        IRepository<OtherLeaveSetting> settings,
        IRepository<Employee> employees,
        IWorkingCalendar calendar) : IGetLumpSumEndDate
    {
        public async Task<LumpSumEndDto> GetAsync(Guid employeeId, Guid otherLeaveSettingId, DateTime startDate)
        {
            var setting = await settings.GetAll().FirstOrDefaultAsync(x => x.Id == otherLeaveSettingId)
                ?? throw new NotFoundException(nameof(OtherLeaveSetting), otherLeaveSettingId.ToString());
            var isManagerial = await employees.GetAll().Where(e => e.Id == employeeId)
                .Select(e => (bool?)e.IsManagerial).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var allocation = setting.AllocationFor(isManagerial);
            var wholeDays = (int)Math.Ceiling(allocation);

            // CalendarDays counting: holidays/weekends COUNT — the block is simply consecutive days.
            if (setting.DayCounting == LeaveDayCounting.CalendarDays)
                return new LumpSumEndDto
                {
                    StartDate = startDate.Date,
                    EndDate = startDate.Date.AddDays(wholeDays - 1),
                    LeaveDays = allocation
                };

            var date = startDate.Date;
            var counted = 0;
            var end = date;
            // WorkingDays counting: walk forward until the block holds the full allocation of
            // WORKING days (holidays/weekends skipped; guarded).
            for (var i = 0; i < 400 && counted < wholeDays; i++)
            {
                if (await calendar.IsWorkingDayAsync(date)) { counted++; end = date; }
                date = date.AddDays(1);
            }
            if (counted < wholeDays)
                throw new ValidationException("startDate", "Could not fit the lump-sum block — check the working-week configuration.");
            return new LumpSumEndDto { StartDate = startDate.Date, EndDate = end, LeaveDays = allocation };
        }
    }

    // ---- Submit (mirrors SubmitAnnualLeave; NEVER touches the annual ledger) --
    public class SubmitOtherLeave(
        IRepository<OtherLeaveHeader> repository,
        IRepository<OtherLeaveDetail> detailRepository,
        IRepository<AnnualLeaveHeader> annualHeaders,
        IRepository<AnnualLeaveDetail> annualDetails,
        IRepository<OtherLeaveSetting> settings,
        IRepository<Employee> employees,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IWorkingCalendar calendar,
        IWorkflowService workflowService,
        Performance.IPerformanceVisibilityService visibility,
        IValidator<SaveOtherLeaveDto> validator,
        ILogger<SubmitOtherLeave> logger) : ISubmitOtherLeave
    {
        public async Task<Guid> SubmitAsync(SaveOtherLeaveDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("employeeId", "You can only submit leave for yourself.");

            // Same strictness as Annual Leave: a request must route through the approval workflow.
            if (!await workflowDefinitions.GetAll().AnyAsync(d =>
                    d.EntityType == WorkflowEntityTypes.OtherLeave && d.IsActive))
                throw new ValidationException("workflow",
                    "No active approval workflow is configured for Other Leave. Ask an administrator to add one under Workflow Definitions (Process: Other Leave) before submitting requests.");
            await workflowService.EnsureStartableAsync(WorkflowEntityTypes.OtherLeave, dto.EmployeeId);

            var setting = await settings.GetAll()
                .Include(x => x.FiscalYear)
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == dto.OtherLeaveSettingId)
                ?? throw new NotFoundException(nameof(OtherLeaveSetting), dto.OtherLeaveSettingId.ToString());
            var leaveName = setting.LeaveType?.Name ?? "This leave";
            if (!setting.IsActive)
                throw new ValidationException("otherLeaveSettingId", $"{leaveName} is inactive.");
            // Rule: settings apply to the ACTIVE fiscal year only.
            if (setting.FiscalYear is null || !setting.FiscalYear.IsActive)
                throw new ValidationException("otherLeaveSettingId",
                    $"{leaveName} belongs to a closed fiscal year — only the active fiscal year's settings can be used.");

            var emp = await employees.GetAll().Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    e.IsManagerial,
                    Gender = e.Person != null ? (Gender?)e.Person.Gender : null,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : ""
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            // Rule: gender-specific leave types.
            if (setting.Gender == GenderEligibility.Female && emp.Gender != Gender.Female)
                throw new ValidationException("otherLeaveSettingId", $"{leaveName} is available to female employees only.");
            if (setting.Gender == GenderEligibility.Male && emp.Gender != Gender.Male)
                throw new ValidationException("otherLeaveSettingId", $"{leaveName} is available to male employees only.");

            var fyStart = setting.FiscalYear.StartDate.ToDateTimeUtc().Date;
            var fyEnd = setting.FiscalYear.EndDate.ToDateTimeUtc().Date;

            var header = OtherLeaveHeader.Create(dto.EmployeeId, setting.Id, DateTime.UtcNow, dto.Remark);

            // Rule: lump-sum leaves (maternity/paternity/mourning) are taken all at once — one block.
            if (setting.IsLumpSum && dto.Details.Count != 1)
                throw new ValidationException("details", $"{leaveName} is taken as ONE continuous block — submit a single line.");

            foreach (var d in dto.Details)
            {
                var start = d.StartDate.Date;
                var end = d.EndDate.Date;
                if (start < fyStart || end > fyEnd)
                    throw new ValidationException("details",
                        $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} falls outside the fiscal year ({fyStart:yyyy-MM-dd}–{fyEnd:yyyy-MM-dd}).");

                // Day-counting config: CalendarDays charges EVERY day (holidays/weekends count);
                // WorkingDays skips them via the working calendar.
                decimal leaveDays;
                if (setting.DayCounting == LeaveDayCounting.CalendarDays)
                {
                    leaveDays = (decimal)(end - start).TotalDays + 1;
                }
                else
                {
                    try { leaveDays = await calendar.CountWorkingDaysAsync(start, end); }
                    catch (ArgumentException ex) { throw new ValidationException("details", ex.Message); }
                    if (leaveDays <= 0)
                        throw new ValidationException("details", $"Line {start:yyyy-MM-dd}→{end:yyyy-MM-dd} contains no working days (only rest days/holidays).");
                }

                header.AddDetail(start, end, leaveDays);
            }

            // Overlap — within the request …
            var newRows = header.Details.Select(x => (x.StartDate, x.EndDate)).ToList();
            for (var i = 0; i < newRows.Count; i++)
                for (var j = i + 1; j < newRows.Count; j++)
                    if (newRows[i].StartDate <= newRows[j].EndDate && newRows[i].EndDate >= newRows[j].StartDate)
                        throw new ValidationException("details", "Two lines in this request overlap the same dates.");

            // … against existing pending/approved OTHER leave …
            var existingOther = await detailRepository.GetAll()
                .Join(repository.GetAll().Where(h => h.EmployeeId == dto.EmployeeId
                        && (h.Status == OtherLeaveStatus.Pending || h.Status == OtherLeaveStatus.Approved)),
                    d => d.OtherLeaveHeaderId, h => h.Id, (d, h) => new { d.StartDate, d.EndDate })
                .ToListAsync();
            // … and against pending/approved ANNUAL leave (an employee cannot be on two leaves at once).
            var existingAnnual = await annualDetails.GetAll()
                .Join(annualHeaders.GetAll().Where(h => h.EmployeeId == dto.EmployeeId
                        && (h.Status == AnnualLeaveStatus.Pending || h.Status == AnnualLeaveStatus.Approved)),
                    d => d.AnnualLeaveHeaderId, h => h.Id, (d, h) => new { d.StartDate, d.EndDate })
                .ToListAsync();
            foreach (var nr in newRows)
            {
                if (existingOther.Any(e => e.StartDate <= nr.EndDate && e.EndDate >= nr.StartDate))
                    throw new ValidationException("details", "A line overlaps other leave this employee already has pending or approved.");
                if (existingAnnual.Any(e => e.StartDate <= nr.EndDate && e.EndDate >= nr.StartDate))
                    throw new ValidationException("details", "A line overlaps annual leave this employee already has pending or approved.");
            }

            // Rule: static, position-based allocation — never accrues, never touches the annual ledger.
            var allocation = setting.AllocationFor(emp.IsManagerial);
            var reserved = await repository.GetAll()
                .Where(r => r.EmployeeId == dto.EmployeeId && r.OtherLeaveSettingId == setting.Id
                    && (r.Status == OtherLeaveStatus.Pending || r.Status == OtherLeaveStatus.Approved))
                .SumAsync(r => (decimal?)r.TotalLeaveDays) ?? 0;
            var remaining = allocation - reserved;

            if (setting.IsLumpSum)
            {
                if (reserved > 0)
                    throw new ValidationException("details", $"{leaveName} has already been taken this fiscal year (it is granted once, as one block).");
                if (header.TotalLeaveDays != allocation)
                    throw new ValidationException("details",
                        $"{leaveName} must be taken all at once: the block must cover exactly {allocation} working day(s), but the selected range covers {header.TotalLeaveDays}.");
            }
            else if (header.TotalLeaveDays > remaining)
            {
                throw new ValidationException("details",
                    $"Insufficient {leaveName} balance: requested {header.TotalLeaveDays} day(s) but only {remaining} of the {allocation}-day allocation remain.");
            }

            await repository.AddAsync(header);
            foreach (var d in header.Details)
                if (string.IsNullOrEmpty(d.TenantId)) d.TenantId = header.TenantId;
            await repository.SaveChangesAsync();

            var name = $"{emp.First} {emp.Grand}".Trim();
            var summary = $"{name} ({emp.EmployeeNumber}): {leaveName}, {header.TotalLeaveDays}d";

            // Same approval mechanism as Annual Leave: the request stays Pending until the FINAL
            // stage approves (OtherLeaveWorkflowHandler applies the outcome).
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.OtherLeave, header.Id, dto.EmployeeId, summary);

            logger.LogInformation("Other leave {Id} ({Name}) submitted for approval ({Days}d)",
                header.Id, leaveName, header.TotalLeaveDays);
            return header.Id;
        }
    }

    // ---- Cancel -------------------------------------------------------------
    public class CancelOtherLeave(
        IRepository<OtherLeaveHeader> repository,
        IWorkflowGate workflowGate,
        ILogger<CancelOtherLeave> logger) : ICancelOtherLeave
    {
        public async Task CancelAsync(CancelOtherLeaveDto dto)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(OtherLeaveHeader), dto.Id.ToString());
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.OtherLeave, header.Id);

            // The balance is DERIVED from pending/approved requests against the static allocation,
            // so cancelling releases the days automatically — there is no ledger to reverse.
            header.Cancel();
            await repository.SaveChangesAsync();
            logger.LogInformation("Other leave {Id} cancelled", header.Id);
        }
    }

    // ---- Reads --------------------------------------------------------------
    public class GetOtherLeaveById(
        IRepository<OtherLeaveHeader> repository,
        Performance.IPerformanceVisibilityService visibility) : IGetOtherLeaveById
    {
        public async Task<OtherLeaveHeaderDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll().Where(r => r.Id == id)
                .Select(OtherLeaveMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(OtherLeaveHeader), id.ToString());
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("access", "You do not have access to this leave request.");
            return dto;
        }
    }

    public class GetAllOtherLeaves(
        IRepository<OtherLeaveHeader> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility) : IGetAllOtherLeaves
    {
        public async Task<PaginatedResponse<OtherLeaveHeaderDto>> GetAsync(GetAllRequest request)
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
                Enum.TryParse<OtherLeaveStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestDate)
                .Skip(skip).Take(take).Select(OtherLeaveMapper.Projection).ToListAsync();
            return new PaginatedResponse<OtherLeaveHeaderDto> { Total = total, Data = data };
        }
    }

    internal static class OtherLeaveMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<OtherLeaveSetting, OtherLeaveSettingDto>> SettingProjection =
            x => new OtherLeaveSettingDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearName = x.FiscalYear != null ? x.FiscalYear.Name : null,
                LeaveTypeId = x.LeaveTypeId,
                Name = x.LeaveType != null ? x.LeaveType.Name : string.Empty,
                Gender = x.Gender.ToString(),
                StandardDays = x.StandardDays,
                ManagerialDays = x.ManagerialDays,
                IsLumpSum = x.IsLumpSum,
                DayCounting = x.DayCounting.ToString(),
                IsActive = x.IsActive,
                Description = x.Description
            };

        public static readonly System.Linq.Expressions.Expression<Func<OtherLeaveHeader, OtherLeaveHeaderDto>> Projection =
            r => new OtherLeaveHeaderDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee != null && r.Employee.Person != null
                    ? (r.Employee.Person.FirstName + " " + r.Employee.Person.GrandFatherName).Trim() : null,
                EmployeeNumber = r.Employee != null ? r.Employee.EmployeeNumber : null,
                OtherLeaveSettingId = r.OtherLeaveSettingId,
                LeaveName = r.Setting != null && r.Setting.LeaveType != null ? r.Setting.LeaveType.Name : null,
                FiscalYearName = r.Setting != null && r.Setting.FiscalYear != null ? r.Setting.FiscalYear.Name : null,
                IsLumpSum = r.Setting != null && r.Setting.IsLumpSum,
                RequestDate = r.RequestDate,
                Remark = r.Remark,
                TotalLeaveDays = r.TotalLeaveDays,
                Status = r.Status.ToString(),
                Details = r.Details.OrderBy(d => d.StartDate).Select(d => new OtherLeaveDetailDto
                {
                    Id = d.Id,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    LeaveDays = d.LeaveDays
                }).ToList()
            };
    }

    // ---- Workflow outcome handler (same mechanism as Annual Leave) ----------
    public class OtherLeaveWorkflowHandler(
        IRepository<OtherLeaveHeader> repository,
        ILogger<OtherLeaveWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.OtherLeave, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (header is null || header.Status != OtherLeaveStatus.Pending) return;
            // The static allocation is drawn down by the request itself (derived balance) —
            // approval finalizes the reservation; the ANNUAL ledger is never touched.
            header.Approve();
            await repository.SaveChangesAsync();
            logger.LogInformation("Other leave {Id} approved via workflow", header.Id);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var header = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (header is null || header.Status != OtherLeaveStatus.Pending) return;
            header.Reject();
            await repository.SaveChangesAsync();
            logger.LogInformation("Other leave {Id} rejected via workflow", header.Id);
        }
    }
}
