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
    public class LeaveRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DayPart { get; set; } = nameof(LeaveDayPart.Full);
        public decimal WorkingDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = nameof(LeaveRequestStatus.Pending);
        public string? DecisionComment { get; set; }
        public string? CancelReason { get; set; }
    }

    public class SaveLeaveRequestDto
    {
        public Guid EmployeeId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveDayPart DayPart { get; set; } = LeaveDayPart.Full;
        public string? Reason { get; set; }
    }

    public class SaveLeaveRequestDtoValidator : AbstractValidator<SaveLeaveRequestDto>
    {
        public SaveLeaveRequestDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required.");
            RuleFor(x => x.LeaveTypeId).NotEmpty().WithMessage("Leave type is required.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("End date is required.");
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date cannot be before start date.");
            RuleFor(x => x.Reason).MaximumLength(1000);
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

    // ---- Submit -------------------------------------------------------------
    public class SubmitLeaveRequest(
        IRepository<LeaveRequest> repository,
        IRepository<LeaveType> leaveTypes,
        IRepository<Employee> employees,
        IRepository<AnnualLeaveSetting> leaveSettings,
        IFiscalYearResolver fiscalYearResolver,
        IWorkingCalendar calendar,
        ILeaveBalanceService balanceService,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IValidator<SaveLeaveRequestDto> validator,
        ILogger<SubmitLeaveRequest> logger) : ISubmitLeaveRequest
    {
        public async Task<Guid> SubmitAsync(SaveLeaveRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var leaveType = await leaveTypes.GetAll().FirstOrDefaultAsync(t => t.Id == dto.LeaveTypeId)
                ?? throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId.ToString());
            if (!leaveType.IsActive)
                throw new ValidationException("leaveTypeId", "This leave type is inactive.");

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

            // Every request is charged to the open fiscal year containing its start date; a request
            // may not straddle two fiscal years (balances are per-year — submit one per year instead).
            var fiscalYear = await fiscalYearResolver.ResolveForDateAsync(dto.StartDate);
            var fyEnd = fiscalYear.EndDate.ToDateTimeUtc().Date;
            if (dto.EndDate.Date > fyEnd)
                throw new ValidationException("endDate",
                    $"The request crosses the fiscal-year boundary ({fyEnd:yyyy-MM-dd}). Submit separate requests per fiscal year.");

            // Gender eligibility (maternity/paternity).
            if (leaveType.GenderEligibility != LeaveGenderEligibility.Any && emp.Gender.HasValue)
            {
                var required = leaveType.GenderEligibility == LeaveGenderEligibility.Male ? Gender.Male : Gender.Female;
                if (emp.Gender.Value != required)
                    throw new ValidationException("leaveTypeId", $"This leave type is restricted to {required} employees.");
            }

            var halfDay = dto.DayPart != LeaveDayPart.Full;
            if (halfDay && !leaveType.AllowHalfDay)
                throw new ValidationException("dayPart", "This leave type does not allow half-day leave.");

            decimal workingDays;
            try
            {
                workingDays = await calendar.CountWorkingDaysAsync(dto.StartDate, dto.EndDate, halfDay);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException("dayPart", ex.Message);
            }
            if (workingDays <= 0)
                throw new ValidationException("startDate", "The selected range contains no working days (only weekends/holidays).");

            if (leaveType.MaxConsecutiveDays.HasValue && workingDays > leaveType.MaxConsecutiveDays.Value)
                throw new ValidationException("endDate", $"This leave type allows at most {leaveType.MaxConsecutiveDays.Value} consecutive days.");

            var start = dto.StartDate.Date;
            var end = dto.EndDate.Date;
            var overlaps = await repository.GetAll().AnyAsync(r =>
                r.EmployeeId == dto.EmployeeId &&
                (r.Status == LeaveRequestStatus.Pending || r.Status == LeaveRequestStatus.Approved) &&
                r.StartDate <= end && r.EndDate >= start);
            if (overlaps)
                throw new ValidationException("startDate", "This employee already has a pending or approved leave request overlapping these dates.");

            // Probation guard (legacy MinExperience): when an accrual policy governs this type,
            // the employee must have served the minimum months before requesting.
            var setting = await leaveSettings.GetAll().FirstOrDefaultAsync(s =>
                s.FiscalYearId == fiscalYear.Id && s.LeaveTypeId == dto.LeaveTypeId && s.IsActive);
            if (setting is not null && setting.MinExperienceMonths > 0)
            {
                var serviceMonths = emp.HireDate.HasValue
                    ? Math.Max(0, ((start.Year - emp.HireDate.Value.Year) * 12) + start.Month - emp.HireDate.Value.Month
                        - (start.Day < emp.HireDate.Value.Day ? 1 : 0))
                    : 0;
                if (serviceMonths < setting.MinExperienceMonths)
                    throw new ValidationException("employeeId",
                        $"This employee has {serviceMonths} month(s) of service; {setting.MinExperienceMonths} are required before taking this leave.");
            }

            // Only entitlement-bearing types are balance-checked; unpaid/None accrual is always available.
            if (leaveType.AccrualMethod != LeaveAccrualMethod.None)
            {
                var available = await balanceService.GetAvailableAsync(dto.EmployeeId, dto.LeaveTypeId, fiscalYear.Id);
                if (workingDays > available)
                    throw new ValidationException("leaveTypeId",
                        $"Insufficient balance: requested {workingDays} day(s) but only {available} available.");
            }

            var request = LeaveRequest.Create(dto.EmployeeId, dto.LeaveTypeId, fiscalYear.Id, start, end, dto.DayPart, workingDays, dto.Reason);
            await repository.AddAsync(request);
            await repository.SaveChangesAsync();

            var name = $"{emp.First} {emp.Grand}".Trim();
            var summary = $"{leaveType.Code} — {name} ({emp.EmployeeNumber}): {start:yyyy-MM-dd}→{end:yyyy-MM-dd} ({workingDays}d)";

            if (leaveType.RequiresApproval)
                await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.LeaveRequest, request.Id, dto.EmployeeId, summary);

            // No workflow is running (approval not required, or no active definition) → auto-approve now.
            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.LeaveRequest, request.Id))
            {
                request.Approve();
                await balanceService.DeductAsync(dto.EmployeeId, dto.LeaveTypeId, fiscalYear.Id, workingDays, request.Id, "Leave auto-approved");
                await repository.SaveChangesAsync();
                logger.LogInformation("Leave request {Id} auto-approved ({Days}d)", request.Id, workingDays);
            }
            else
            {
                logger.LogInformation("Leave request {Id} submitted for approval ({Days}d)", request.Id, workingDays);
            }

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
            var request = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(LeaveRequest), dto.Id.ToString());

            // Can't cancel directly while an approval is in flight — reject it from the workflow instead.
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.LeaveRequest, request.Id);

            var wasApproved = request.HoldsBalance;
            request.Cancel(dto.Reason);

            if (wasApproved)
                await balanceService.ReverseAsync(request.EmployeeId, request.LeaveTypeId, request.FiscalYearId,
                    request.WorkingDays, request.Id, "Leave cancelled");
            else
                await repository.SaveChangesAsync();

            logger.LogInformation("Leave request {Id} cancelled (balance reversed: {Reversed})", request.Id, wasApproved);
        }
    }

    // ---- Reads --------------------------------------------------------------
    public class GetLeaveRequestById(IRepository<LeaveRequest> repository) : IGetLeaveRequestById
    {
        public async Task<LeaveRequestDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(r => r.Id == id).Select(LeaveRequestMapper.Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(LeaveRequest), id.ToString());
        }
    }

    public class GetAllLeaveRequests(IRepository<LeaveRequest> repository) : IGetAllLeaveRequests
    {
        public async Task<PaginatedResponse<LeaveRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<LeaveRequestStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StartDate)
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
            LeaveTypeId = r.LeaveTypeId,
            LeaveTypeCode = r.LeaveType != null ? r.LeaveType.Code : null,
            LeaveTypeName = r.LeaveType != null ? r.LeaveType.Name : null,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            DayPart = r.DayPart.ToString(),
            WorkingDays = r.WorkingDays,
            Reason = r.Reason,
            Status = r.Status.ToString(),
            DecisionComment = r.DecisionComment,
            CancelReason = r.CancelReason
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
            var request = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (request is null || request.Status != LeaveRequestStatus.Pending) return;

            request.Approve();
            await balanceService.DeductAsync(request.EmployeeId, request.LeaveTypeId, request.FiscalYearId,
                request.WorkingDays, request.Id, "Leave approved");
            logger.LogInformation("Leave request {Id} approved via workflow; balance debited {Days}d", request.Id, request.WorkingDays);
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
