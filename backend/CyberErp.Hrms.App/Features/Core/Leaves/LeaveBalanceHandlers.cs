using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class LeaveBalanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        /// <summary>Null on the ANNUAL balance, which has no leave type by design.</summary>
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        public decimal Entitled { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Adjusted { get; set; }
        public decimal Taken { get; set; }
        public decimal Available { get; set; }
    }

    public class SetLeaveBalanceDto
    {
        public Guid EmployeeId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public Guid FiscalYearId { get; set; }
        public decimal Entitled { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Adjusted { get; set; }
        public string? Reason { get; set; }
    }

    public class SetLeaveBalanceDtoValidator : AbstractValidator<SetLeaveBalanceDto>
    {
        public SetLeaveBalanceDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.LeaveTypeId).NotEmpty();
            RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required.");
            RuleFor(x => x.Entitled).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CarriedForward).GreaterThanOrEqualTo(0);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetLeaveBalances { Task<List<LeaveBalanceDto>> GetAsync(Guid employeeId, Guid? fiscalYearId); }
    public interface ISetLeaveBalance { Task SetAsync(SetLeaveBalanceDto dto); }

    // ---- Get balances for an employee ---------------------------------------
    public class GetLeaveBalances(
        IRepository<LeaveBalance> repository,
        Performance.IPerformanceVisibilityService visibility) : IGetLeaveBalances
    {
        public async Task<List<LeaveBalanceDto>> GetAsync(Guid employeeId, Guid? fiscalYearId)
        {
            // HR admin, the employee themselves, or their manager (subtree) only.
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("access", "You do not have access to this employee's leave balances.");

            var query = repository.GetAll().Where(b => b.EmployeeId == employeeId);
            if (fiscalYearId.HasValue && fiscalYearId.Value != Guid.Empty)
                query = query.Where(b => b.FiscalYearId == fiscalYearId.Value);

            return await query
                .OrderByDescending(b => b.FiscalYear!.StartDate).ThenBy(b => b.LeaveType!.Code)
                .Select(b => new LeaveBalanceDto
                {
                    Id = b.Id,
                    EmployeeId = b.EmployeeId,
                    LeaveTypeId = b.LeaveTypeId,
                    LeaveTypeCode = b.LeaveType != null ? b.LeaveType.Code : null,
                    // No type => this is the annual balance; label it rather than showing a blank.
                    LeaveTypeName = b.LeaveType != null ? b.LeaveType.Name : AnnualLeave.DisplayName,
                    FiscalYearId = b.FiscalYearId,
                    FiscalYearName = b.FiscalYear != null ? b.FiscalYear.Name : null,
                    Entitled = b.Entitled,
                    CarriedForward = b.CarriedForward,
                    Adjusted = b.Adjusted,
                    Taken = b.Taken,
                    Available = b.Entitled + b.CarriedForward + b.Adjusted - b.Taken
                })
                .ToListAsync();
        }
    }

    // ---- Set opening / adjust (HC033) ---------------------------------------
    public class SetLeaveBalance(
        ILeaveBalanceService balanceService,
        IRepository<LeaveType> leaveTypes,
        IRepository<Employee> employees,
        IRepository<FiscalYear> fiscalYears,
        Performance.IPerformanceVisibilityService visibility,
        IValidator<SetLeaveBalanceDto> validator) : ISetLeaveBalance
    {
        public async Task SetAsync(SetLeaveBalanceDto dto)
        {
            // ⚠️ HR ONLY, and checked BEFORE anything else.
            //
            // The read path next door uses CanAccessEmployeeAsync, which is right for reading a
            // balance and WRONG for setting one: it permits self, so an employee could grant
            // themselves an entitlement. An opening balance is policy, not a self-service field.
            //
            // First, not after validation, for two reasons: the NotFound checks below would
            // otherwise let an unauthorised caller probe which employee and leave-type ids exist,
            // and a guard sitting behind a validator is one a deny test never reaches (logic §12.52).
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                throw new ValidationException("access", "Only HR can set an employee's leave balance.");

            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await leaveTypes.GetAll().AnyAsync(t => t.Id == dto.LeaveTypeId))
                throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId.ToString());
            if (!await employees.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            var fy = await fiscalYears.GetAll().FirstOrDefaultAsync(f => f.Id == dto.FiscalYearId)
                ?? throw new NotFoundException(nameof(FiscalYear), dto.FiscalYearId.ToString());
            if (fy.IsClosed)
                throw new ValidationException("fiscalYearId", "This fiscal year is closed.");

            await balanceService.SetOpeningAsync(dto.EmployeeId, dto.LeaveTypeId, dto.FiscalYearId,
                dto.Entitled, dto.CarriedForward, dto.Adjusted, dto.Reason);
        }
    }
}
