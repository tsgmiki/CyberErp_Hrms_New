using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>One entry in a leave request's lifecycle, whatever produced it.</summary>
    public class AnnualLeaveHistoryEntryDto
    {
        public DateTime At { get; set; }
        /// <summary>Submitted | Workflow | Return | Settled — lets the UI pick an icon and a tone.</summary>
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public string? Actor { get; set; }
        /// <summary>Approve | Reject | Confirm | … — the workflow action where there is one.</summary>
        public string? Action { get; set; }
        public string? Comment { get; set; }
        /// <summary>Which approval stage, for workflow rows.</summary>
        public int? StepOrder { get; set; }
        public string? StepName { get; set; }
    }

    /// <summary>The full lifecycle of one annual leave request, for the approver's history popup.</summary>
    public class AnnualLeaveHistoryDto
    {
        public Guid Id { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ApprovedDays { get; set; }
        public decimal? ActualDays { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        /// <summary>Signed days moved by the settled return: negative = credited back, positive = extra.</summary>
        public decimal? AdjustmentDays { get; set; }
        public string? ReturnType { get; set; }
        public List<AnnualLeaveHistoryEntryDto> Entries { get; set; } = [];
    }

    public interface IGetAnnualLeaveHistory { Task<AnnualLeaveHistoryDto> GetAsync(Guid headerId); }

    /// <summary>
    /// Builds ONE ordered timeline out of the three places a leave request's story is written: the
    /// request itself, the workflow action log (for both the original approval and any return
    /// adjustment), and the return confirmations.
    ///
    /// <para>Assembled server-side on purpose. An approver deciding on an adjustment needs to see what
    /// was originally approved, who approved it, what the employee then said, and what any earlier
    /// attempt was rejected for — stitching that together in the client would mean four round trips and
    /// four chances to show a partial story.</para>
    /// </summary>
    public class GetAnnualLeaveHistory(
        IRepository<AnnualLeaveHeader> headers,
        IRepository<AnnualLeaveReturn> returns,
        IRepository<WorkflowInstance> instances,
        IRepository<WorkflowActionLog> actionLogs,
        Performance.IPerformanceVisibilityService visibility) : IGetAnnualLeaveHistory
    {
        public async Task<AnnualLeaveHistoryDto> GetAsync(Guid id)
        {
            // Accept EITHER the request id or a RETURN id.
            //
            // An approver reaches this from the workflow inbox, and an adjustment's workflow instance
            // carries the AnnualLeaveReturn id — the approver has no way to know it is holding a
            // different kind of id, and should not have to. Resolving it here is one extra lookup on a
            // miss, versus making every caller learn the difference.
            var header = await headers.GetAll().AsNoTracking()
                .Include(h => h.Details)
                .Include(h => h.Employee).ThenInclude(e => e!.Person)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (header is null)
            {
                var ownerId = await returns.GetAll().AsNoTracking()
                    .Where(r => r.Id == id)
                    .Select(r => (Guid?)r.AnnualLeaveHeaderId)
                    .FirstOrDefaultAsync();
                if (ownerId is not null)
                    header = await headers.GetAll().AsNoTracking()
                        .Include(h => h.Details)
                        .Include(h => h.Employee).ThenInclude(e => e!.Person)
                        .FirstOrDefaultAsync(h => h.Id == ownerId.Value);
            }

            if (header is null) throw new NotFoundException(nameof(AnnualLeaveHeader), id.ToString());
            var headerId = header.Id;

            // Same visibility rule as the request itself: own records, or those of employees in scope.
            if (!await visibility.CanAccessEmployeeAsync(header.EmployeeId))
                throw new ValidationException("employeeId", "You cannot view this leave request.");

            var returnRows = await returns.GetAll().AsNoTracking()
                .Where(r => r.AnnualLeaveHeaderId == headerId)
                .OrderBy(r => r.ConfirmedAt)
                .ToListAsync();

            // ONE query for every instance in this story — the request's own, plus one per return
            // attempt — rather than a lookup per row.
            var returnIds = returnRows.Select(r => r.Id).ToList();
            var entityIds = new List<Guid>(returnIds) { headerId };
            var relevant = await instances.GetAll().AsNoTracking()
                .Where(i => entityIds.Contains(i.EntityId)
                            && (i.EntityType == WorkflowEntityTypes.AnnualLeave
                                || i.EntityType == WorkflowEntityTypes.AnnualLeaveReturn))
                .Select(i => new { i.Id, i.EntityId, i.EntityType, i.RequestedBy })
                .ToListAsync();

            var instanceIds = relevant.Select(i => i.Id).ToList();
            var logs = instanceIds.Count == 0
                ? []
                : await actionLogs.GetAll().AsNoTracking()
                    .Where(a => instanceIds.Contains(a.InstanceId))
                    .OrderBy(a => a.ActedAt)
                    .ToListAsync();

            var isReturnInstance = relevant
                .Where(i => i.EntityType == WorkflowEntityTypes.AnnualLeaveReturn)
                .Select(i => i.Id).ToHashSet();

            var entries = new List<AnnualLeaveHistoryEntryDto>
            {
                new()
                {
                    At = header.RequestDate,
                    Kind = "Submitted",
                    Title = $"Leave requested — {header.TotalLeaveDays:0.##} day(s)",
                    Detail = DescribeRange(header),
                    Actor = header.CreatedBy,
                    Comment = header.Remark
                }
            };

            entries.AddRange(logs.Select(a => new AnnualLeaveHistoryEntryDto
            {
                At = a.ActedAt,
                Kind = "Workflow",
                // The same step names appear on both chains, so say which decision this was.
                Title = isReturnInstance.Contains(a.InstanceId)
                    ? $"Return adjustment — {a.StepName}"
                    : $"Leave request — {a.StepName}",
                Action = a.Action.ToString(),
                Actor = a.ActedBy,
                Comment = a.Comment,
                StepOrder = a.StepOrder,
                StepName = a.StepName
            }));

            foreach (var r in returnRows)
            {
                var word = r.ReturnType switch
                {
                    AnnualLeaveReturnType.Early => $"Returned early — {Math.Abs(r.AdjustmentDays):0.##} day(s) fewer",
                    AnnualLeaveReturnType.Late => $"Returned late — {r.AdjustmentDays:0.##} extra day(s) requested",
                    _ => "Returned as approved"
                };
                entries.Add(new AnnualLeaveHistoryEntryDto
                {
                    At = r.ConfirmedAt,
                    Kind = "Return",
                    Title = word,
                    Detail = $"Approved to {r.PlannedEndDate:yyyy-MM-dd} ({r.ApprovedDays:0.##}d); "
                           + $"actually to {r.ActualEndDate:yyyy-MM-dd} ({r.ActualDays:0.##}d)",
                    Actor = r.CreatedBy,
                    Comment = r.Comment,
                    Action = r.Status.ToString()
                });
            }

            if (header.Status == AnnualLeaveStatus.Closed && header.UpdatedAt is not null)
                entries.Add(new AnnualLeaveHistoryEntryDto
                {
                    At = header.UpdatedAt.Value.ToDateTimeUtc(),
                    Kind = "Settled",
                    Title = $"Closed — {header.ActualLeaveDays:0.##} day(s) charged",
                    Actor = header.UpdatedBy
                });

            var settled = returnRows.LastOrDefault(r => r.Status == AnnualLeaveReturnStatus.Approved);
            var latest = returnRows.LastOrDefault();

            return new AnnualLeaveHistoryDto
            {
                Id = header.Id,
                EmployeeName = header.Employee?.Person is null ? null
                    : $"{header.Employee.Person.FirstName} {header.Employee.Person.GrandFatherName}".Trim(),
                EmployeeNumber = header.Employee?.EmployeeNumber,
                RequestDate = header.RequestDate,
                Status = header.Status.ToString(),
                ApprovedDays = header.TotalLeaveDays,
                ActualDays = header.ActualLeaveDays,
                PlannedEndDate = latest?.PlannedEndDate
                    ?? (header.Details.Count == 0 ? null : header.Details.Max(d => d.EndDate)),
                ActualEndDate = latest?.ActualEndDate,
                AdjustmentDays = settled?.AdjustmentDays,
                ReturnType = latest?.ReturnType.ToString(),
                Entries = [.. entries.OrderBy(e => e.At)]
            };
        }

        private static string DescribeRange(AnnualLeaveHeader header)
        {
            if (header.Details.Count == 0) return string.Empty;
            var from = header.Details.Min(d => d.StartDate);
            var to = header.Details.Max(d => d.EndDate);
            return $"{from:yyyy-MM-dd} → {to:yyyy-MM-dd}";
        }
    }
}
