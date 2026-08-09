using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------

    public class ConfirmAnnualLeaveReturnDto
    {
        public Guid AnnualLeaveHeaderId { get; set; }
        /// <summary>Last day actually on leave — the day BEFORE the employee resumed work.</summary>
        public DateTime ActualEndDate { get; set; }
        /// <summary>Required when the return differs from what was approved.</summary>
        public string? Comment { get; set; }
    }

    /// <summary>What the confirmation did, so the UI can say so without a second round trip.</summary>
    public class AnnualLeaveReturnResultDto
    {
        public Guid ReturnId { get; set; }
        public string ReturnType { get; set; } = nameof(AnnualLeaveReturnType.OnTime);
        public decimal ApprovedDays { get; set; }
        public decimal ActualDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        /// <summary>True when the adjustment went for approval instead of settling immediately.</summary>
        public bool RequiresApproval { get; set; }
        public string HeaderStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>A preview of what confirming a given return date would do — drives the form's live summary.</summary>
    public class AnnualLeaveReturnPreviewDto
    {
        public DateTime PlannedEndDate { get; set; }
        public decimal ApprovedDays { get; set; }
        public decimal ActualDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        public string ReturnType { get; set; } = nameof(AnnualLeaveReturnType.OnTime);
        public bool CommentRequired { get; set; }
        public bool RequiresApproval { get; set; }
        /// <summary>Balance available for a LATE return's extra days; null when not applicable.</summary>
        public decimal? AvailableForExtension { get; set; }
        public string? Warning { get; set; }
    }

    public interface IConfirmAnnualLeaveReturn { Task<AnnualLeaveReturnResultDto> ConfirmAsync(ConfirmAnnualLeaveReturnDto dto); }
    public interface IPreviewAnnualLeaveReturn { Task<AnnualLeaveReturnPreviewDto> PreviewAsync(Guid headerId, DateTime actualEndDate); }

    // ---- Shared -------------------------------------------------------------

    internal static class AnnualLeaveReturnShared
    {
        /// <summary>
        /// Recomputes the days actually taken over the REAL range, from the first approved leave day to
        /// the day the employee actually stopped being on leave.
        ///
        /// <para>Recomputed rather than arithmetic on the approved total, because the difference is not a
        /// count of calendar days: returning two days early over a weekend costs nothing, while the same
        /// two days midweek costs two. Only the working calendar knows which.</para>
        ///
        /// <para>Half days are the one thing it cannot see — a half-day request that ends early is
        /// re-counted as whole working days — so a return that lands inside a half-day row keeps the
        /// approved figure for that row.</para>
        /// </summary>
        internal static async Task<decimal> ActualDaysAsync(
            IWorkingCalendar calendar, IReadOnlyList<AnnualLeaveDetail> details, DateTime actualEndDate)
        {
            var end = actualEndDate.Date;
            decimal total = 0m;

            foreach (var d in details.OrderBy(x => x.StartDate))
            {
                if (d.StartDate > end) continue;                       // whole row is after the return
                if (d.EndDate <= end) { total += d.LeaveDays; continue; }  // row fully taken

                // The return lands INSIDE this row. A half day is atomic — it is either taken or not,
                // and it was taken, so it keeps its 0.5.
                if (d.LeaveUsage == AnnualLeaveUsage.HalfDay) { total += d.LeaveDays; continue; }

                total += await calendar.CountWorkingDaysAsync(d.StartDate, end);
            }

            // A LATE return runs past every approved row, so the loop above can only ever reach the
            // approved total — the extra days exist outside the request and have to be counted on their
            // own. Through the calendar again, so a weekend or holiday in the overrun is still free.
            var plannedEnd = PlannedEnd(details);
            if (end > plannedEnd)
                total += await calendar.CountWorkingDaysAsync(plannedEnd.AddDays(1), end);

            return total;
        }

        internal static DateTime PlannedEnd(IReadOnlyList<AnnualLeaveDetail> details) =>
            details.Count == 0 ? DateTime.UtcNow.Date : details.Max(d => d.EndDate);

        internal static DateTime PlannedStart(IReadOnlyList<AnnualLeaveDetail> details) =>
            details.Count == 0 ? DateTime.UtcNow.Date : details.Min(d => d.StartDate);
    }

    // ---- Preview ------------------------------------------------------------

    /// <summary>
    /// Read-only "what would happen" for a candidate return date. The form calls this as the date
    /// changes so the employee sees the day count and whether it needs approval BEFORE committing —
    /// the alternative is confirming blind and discovering the consequence afterwards.
    /// </summary>
    public class PreviewAnnualLeaveReturn(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<LeaveBalance> ledgers,
        IWorkingCalendar calendar,
        Performance.IPerformanceVisibilityService visibility) : IPreviewAnnualLeaveReturn
    {
        public async Task<AnnualLeaveReturnPreviewDto> PreviewAsync(Guid headerId, DateTime actualEndDate)
        {
            var header = await repository.GetAll().AsNoTracking()
                .Include(h => h.Details)
                .FirstOrDefaultAsync(h => h.Id == headerId)
                ?? throw new NotFoundException(nameof(AnnualLeaveHeader), headerId.ToString());

            if (!await visibility.CanAccessEmployeeAsync(header.EmployeeId))
                throw new ValidationException("employeeId", "You can only view your own leave requests.");

            var details = header.Details.ToList();
            var plannedEnd = AnnualLeaveReturnShared.PlannedEnd(details);
            var actualDays = await AnnualLeaveReturnShared.ActualDaysAsync(calendar, details, actualEndDate);
            var adjustment = actualDays - header.TotalLeaveDays;
            var type = adjustment < 0 ? AnnualLeaveReturnType.Early
                     : adjustment > 0 ? AnnualLeaveReturnType.Late
                     : AnnualLeaveReturnType.OnTime;

            var dto = new AnnualLeaveReturnPreviewDto
            {
                PlannedEndDate = plannedEnd,
                ApprovedDays = header.TotalLeaveDays,
                ActualDays = actualDays,
                AdjustmentDays = adjustment,
                ReturnType = type.ToString(),
                CommentRequired = type != AnnualLeaveReturnType.OnTime,
                RequiresApproval = type != AnnualLeaveReturnType.OnTime
            };

            if (type == AnnualLeaveReturnType.Late)
            {
                var ledger = await ledgers.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == header.AnnualLeaveLedgerId);
                dto.AvailableForExtension = ledger?.Available ?? 0m;
                if (dto.AvailableForExtension < adjustment)
                    dto.Warning = $"The extra {adjustment:0.##} day(s) exceed the {dto.AvailableForExtension:0.##} "
                        + "day(s) left on this entitlement.";
            }

            return dto;
        }
    }

    // ---- Confirm ------------------------------------------------------------

    /// <summary>
    /// The employee confirms they are back. Three outcomes, per the agreed process:
    ///
    /// <list type="bullet">
    /// <item><b>On time</b> — settles immediately. The ledger already holds exactly these days, so there
    /// is nothing to move and nothing to approve.</item>
    /// <item><b>Early</b> — the unused days are NOT credited here. The request goes back through the
    /// approval workflow and the ledger moves only when an approver accepts, so the balance never shows
    /// a credit nobody signed off.</item>
    /// <item><b>Late</b> — the extra days are an extension on the SAME request, so one record carries the
    /// whole story. It also goes for approval, and only then is the extra debited.</item>
    /// </list>
    /// </summary>
    public class ConfirmAnnualLeaveReturn(
        IRepository<AnnualLeaveHeader> repository,
        IRepository<AnnualLeaveReturn> returns,
        IRepository<LeaveBalance> ledgers,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IWorkingCalendar calendar,
        IWorkflowService workflowService,
        Performance.IPerformanceVisibilityService visibility,
        ILogger<ConfirmAnnualLeaveReturn> logger) : IConfirmAnnualLeaveReturn
    {
        public async Task<AnnualLeaveReturnResultDto> ConfirmAsync(ConfirmAnnualLeaveReturnDto dto)
        {
            var header = await repository.GetAll()
                .Include(h => h.Details)
                .FirstOrDefaultAsync(h => h.Id == dto.AnnualLeaveHeaderId)
                ?? throw new NotFoundException(nameof(AnnualLeaveHeader), dto.AnnualLeaveHeaderId.ToString());

            if (!await visibility.CanAccessEmployeeAsync(header.EmployeeId))
                throw new ValidationException("employeeId", "You can only confirm your own return.");

            if (!header.CanConfirmReturn)
                throw new ValidationException("status", header.Status switch
                {
                    AnnualLeaveStatus.ReturnPending => "This return is already awaiting approval.",
                    AnnualLeaveStatus.Closed => "This leave has already been closed.",
                    _ => $"Only an approved leave request can be confirmed as returned (current: {header.Status})."
                });

            var details = header.Details.ToList();
            var plannedStart = AnnualLeaveReturnShared.PlannedStart(details);
            var plannedEnd = AnnualLeaveReturnShared.PlannedEnd(details);
            var actualEnd = dto.ActualEndDate.Date;

            // A return before the leave even began is a data-entry slip, not a zero-day leave.
            if (actualEnd < plannedStart.AddDays(-1))
                throw new ValidationException(nameof(dto.ActualEndDate),
                    $"The return date cannot be before the leave started ({plannedStart:yyyy-MM-dd}).");

            var actualDays = await AnnualLeaveReturnShared.ActualDaysAsync(calendar, details, actualEnd);
            var adjustment = actualDays - header.TotalLeaveDays;

            // Guard the LATE case against overdrawing the entitlement, using the same available-balance
            // rule submission uses. Catching it here means the approver is never asked to rubber-stamp
            // an extension the balance cannot fund.
            if (adjustment > 0)
            {
                var ledger = await ledgers.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == header.AnnualLeaveLedgerId);
                var available = ledger?.Available ?? 0m;
                if (available < adjustment)
                    throw new ValidationException(nameof(dto.ActualEndDate),
                        $"The extra {adjustment:0.##} day(s) exceed the {available:0.##} day(s) left on this entitlement. "
                        + "Record the excess as unpaid leave instead.");
            }

            AnnualLeaveReturn record;
            try
            {
                record = AnnualLeaveReturn.Create(
                    header.Id, plannedEnd, actualEnd, header.TotalLeaveDays, actualDays, dto.Comment);
            }
            catch (ArgumentException ex)
            {
                // The entity guards the comment rule; surface it as a field error rather than a 500.
                throw new ValidationException(nameof(dto.Comment), ex.Message);
            }

            var result = new AnnualLeaveReturnResultDto
            {
                ApprovedDays = header.TotalLeaveDays,
                ActualDays = actualDays,
                AdjustmentDays = adjustment,
                ReturnType = record.ReturnType.ToString()
            };

            if (record.ReturnType == AnnualLeaveReturnType.OnTime)
            {
                header.CloseOnTimeReturn(actualDays);
                result.RequiresApproval = false;
                result.Message = $"Return confirmed — {actualDays:0.##} day(s) logged as approved.";
            }
            else
            {
                // An adjustment moves money, so it needs its own approval chain. Fail before persisting
                // anything when none is configured: a return stuck in ReturnPending with no workflow to
                // approve it would strand the request and its ledger debit.
                if (!await workflowDefinitions.GetAll().AnyAsync(d =>
                        d.EntityType == WorkflowEntityTypes.AnnualLeaveReturn && d.IsActive))
                    throw new ValidationException("workflow",
                        "No active approval workflow is configured for Annual Leave Returns. Ask an administrator "
                        + "to add one under Workflow Definitions (Process: Annual Leave Return) before adjusting a "
                        + "return. Returning exactly as approved needs no workflow and still works.");

                await workflowService.EnsureStartableAsync(WorkflowEntityTypes.AnnualLeaveReturn, header.EmployeeId);

                header.BeginReturnAdjustment();
                result.RequiresApproval = true;
                result.Message = record.ReturnType == AnnualLeaveReturnType.Early
                    ? $"Early return submitted — {Math.Abs(adjustment):0.##} day(s) will be credited back once approved."
                    : $"Extension submitted — {adjustment:0.##} extra day(s) will be deducted once approved.";
            }

            await returns.AddAsync(record);
            repository.UpdateAsync(header);
            await repository.SaveChangesAsync();

            if (result.RequiresApproval)
            {
                var verb = record.ReturnType == AnnualLeaveReturnType.Early ? "Early return" : "Late return";
                await workflowService.StartIfDefinedAsync(
                    WorkflowEntityTypes.AnnualLeaveReturn, record.Id, header.EmployeeId,
                    $"{verb} — {Math.Abs(adjustment):0.##} day(s) against leave of {header.TotalLeaveDays:0.##} day(s)");
            }

            result.ReturnId = record.Id;
            result.HeaderStatus = header.Status.ToString();
            logger.LogInformation(
                "Annual leave {Header}: return confirmed {Type}, approved {Approved}d vs actual {Actual}d",
                header.Id, record.ReturnType, header.TotalLeaveDays, actualDays);
            return result;
        }
    }

    // ---- Workflow outcome ---------------------------------------------------

    /// <summary>
    /// Applies an approved or rejected return adjustment. This is the ONLY place the ledger moves for a
    /// return, which is what keeps the balance equal to the sum of approved decisions.
    /// </summary>
    public class AnnualLeaveReturnWorkflowHandler(
        IRepository<AnnualLeaveReturn> returns,
        IRepository<AnnualLeaveHeader> headers,
        IRepository<LeaveBalance> ledgers,
        ILeaveBalanceService balanceService,
        ILogger<AnnualLeaveReturnWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.AnnualLeaveReturn, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var (record, header, ledger) = await LoadAsync(entityId);
            if (record is null || header is null || ledger is null) return;

            record.Approve();

            // Early → give back what was not taken. Late → take the extra. Both go through the same
            // ledger service the original approval used, so every movement stays one auditable row.
            if (record.AdjustmentDays < 0)
                await balanceService.ReverseAsync(ledger.EmployeeId, ledger.LeaveTypeId, ledger.FiscalYearId,
                    Math.Abs(record.AdjustmentDays), header.Id, "Annual leave early return — days credited back");
            else if (record.AdjustmentDays > 0)
                await balanceService.DeductAsync(ledger.EmployeeId, ledger.LeaveTypeId, ledger.FiscalYearId,
                    record.AdjustmentDays, header.Id, "Annual leave late return — extra days deducted");

            header.SettleReturn(record.ActualDays);
            headers.UpdateAsync(header);
            await headers.SaveChangesAsync();

            logger.LogInformation("Annual leave {Header}: return {Type} approved, ledger adjusted {Adj}d",
                header.Id, record.ReturnType, record.AdjustmentDays);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var (record, header, _) = await LoadAsync(entityId);
            if (record is null || header is null) return;

            // Nothing to unwind: the ledger was never moved for this adjustment. The request simply
            // returns to Approved so the employee can confirm again with a corrected date.
            record.Reject();
            header.RejectReturn();
            headers.UpdateAsync(header);
            await headers.SaveChangesAsync();

            logger.LogInformation("Annual leave {Header}: return adjustment rejected; back to Approved", header.Id);
        }

        private async Task<(AnnualLeaveReturn?, AnnualLeaveHeader?, LeaveBalance?)> LoadAsync(Guid returnId)
        {
            var record = await returns.GetAll().FirstOrDefaultAsync(r => r.Id == returnId);
            if (record is null || record.Status != AnnualLeaveReturnStatus.PendingApproval)
                return (null, null, null);       // already settled, or a replayed callback

            var header = await headers.GetAll().FirstOrDefaultAsync(h => h.Id == record.AnnualLeaveHeaderId);
            if (header is null || header.Status != AnnualLeaveStatus.ReturnPending) return (null, null, null);

            var ledger = await ledgers.GetAll().FirstOrDefaultAsync(b => b.Id == header.AnnualLeaveLedgerId);
            return (record, header, ledger);
        }
    }
}
