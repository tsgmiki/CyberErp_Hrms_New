using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    public interface IActingEntitlementService
    {
        /// <summary>Give the deputy the post's allowances and benefits for the assignment period.</summary>
        Task<int> GrantAsync(ActingAssignment assignment);

        /// <summary>
        /// Take them back. <paramref name="endedOn"/> is the last covered day — the assignment's end
        /// date normally, or today when it was cut short.
        /// </summary>
        Task<int> WithdrawAsync(ActingAssignment assignment, DateTime endedOn);
    }

    /// <summary>
    /// Moves a post's entitlements onto a deputy while they act in it, and off again afterwards.
    /// </summary>
    /// <remarks>
    /// <para>Completes the half of the client's rule §12.118 could not: "the salary AND BENEFITS
    /// associated with the deputized position". Salary came from the post already; the rest had
    /// nowhere to come from until <see cref="PositionEntitlement"/> existed.</para>
    ///
    /// <para>⚠️ EVERY GRANT IS DATED TO THE ASSIGNMENT, not left open. Both compensation entities
    /// already carry an effective window, so an acting allowance expires on its own even if
    /// nothing ever runs again — which matters, because the failure mode of the alternative is an
    /// allowance somebody keeps drawing for years after the cover ended.</para>
    ///
    /// <para>⚠️ AND EVERY GRANT IS TAGGED. The remark carries the assignment id, which is how
    /// withdrawal finds exactly what this feature created and nothing else. Matching on employee +
    /// allowance type would be enough to catch a deputy's OWN pre-existing transport allowance and
    /// cancel it on the way out.</para>
    /// </remarks>
    public class ActingEntitlementService(
        IRepository<PositionEntitlement> entitlements,
        IRepository<Position> positions,
        IRepository<AllowanceType> allowanceTypes,
        IRepository<EmployeeAllowance> employeeAllowances,
        IRepository<EmployeeBenefitEnrollment> enrollments,
        ILogger<ActingEntitlementService> logger) : IActingEntitlementService
    {
        /// <summary>Marker written into every granted row, carrying the assignment id.</summary>
        internal static string Tag(Guid assignmentId) => $"[acting:{assignmentId}]";

        public async Task<int> GrantAsync(ActingAssignment assignment)
        {
            if (assignment.PositionId is not Guid positionId) return 0;

            var positionClassId = await positions.GetAll().AsNoTracking()
                .Where(p => p.Id == positionId)
                .Select(p => (Guid?)p.PositionClassId)
                .FirstOrDefaultAsync();
            if (positionClassId is null) return 0;

            var rows = await entitlements.GetAll().AsNoTracking()
                .Where(e => e.PositionClassId == positionClassId.Value
                            && e.IsActive && e.GrantedWhenActing)
                .ToListAsync();
            if (rows.Count == 0) return 0;

            var tag = Tag(assignment.Id);
            var remark = $"{tag} Acting {assignment.PositionTitle}";
            var granted = 0;

            foreach (var row in rows)
            {
                if (row.Kind == EntitlementKind.Allowance && row.AllowanceTypeId is Guid typeId)
                {
                    // The post's own figure wins; otherwise the catalogue's default rate. A type
                    // with neither is a configuration gap, not a zero — granting 0 would look like
                    // a decision somebody made.
                    var value = row.Value ?? await allowanceTypes.GetAll().AsNoTracking()
                        .Where(t => t.Id == typeId).Select(t => t.DefaultRate).FirstOrDefaultAsync();
                    if (value is not decimal amount)
                    {
                        logger.LogWarning(
                            "Acting {Id}: allowance type {TypeId} has no value on the entitlement and "
                            + "no default rate — skipped rather than granted as zero.",
                            assignment.Id, typeId);
                        continue;
                    }

                    await employeeAllowances.AddAsync(EmployeeAllowance.Create(
                        assignment.EmployeeId, typeId, amount,
                        assignment.StartDate, assignment.EndDate, remark));
                    granted++;
                }
                else if (row.Kind == EntitlementKind.BenefitPlan && row.BenefitPlanId is Guid planId)
                {
                    // ⚠️ Never enrol somebody twice. A deputy already in this plan on their own
                    // account keeps their own enrolment — adding a second would double the
                    // contribution and, on conclusion, withdrawing "the acting one" would leave
                    // behind a mess nobody could reconcile.
                    var alreadyIn = await enrollments.GetAll().AsNoTracking()
                        .AnyAsync(x => x.EmployeeId == assignment.EmployeeId
                                    && x.BenefitPlanId == planId
                                    && x.Status == BenefitEnrollmentStatus.Enrolled);
                    if (alreadyIn) continue;

                    await enrollments.AddAsync(EmployeeBenefitEnrollment.Create(
                        planId, assignment.EmployeeId, DateTime.UtcNow.Date,
                        assignment.StartDate, row.Value, remark));
                    granted++;
                }
            }

            if (granted > 0) await employeeAllowances.SaveChangesAsync();
            logger.LogInformation("Acting {Id}: granted {Count} post entitlement(s) to {Employee}",
                assignment.Id, granted, assignment.EmployeeId);
            return granted;
        }

        public async Task<int> WithdrawAsync(ActingAssignment assignment, DateTime endedOn)
        {
            var tag = Tag(assignment.Id);
            var closed = 0;

            // ⚠️ Found by TAG, never by employee + type. The deputy may hold the same allowance on
            // their own account, and closing that would take away something this feature never gave.
            var allowances = await employeeAllowances.GetAll()
                .Where(a => a.EmployeeId == assignment.EmployeeId
                            && a.Remark != null && a.Remark.Contains(tag))
                .ToListAsync();

            foreach (var a in allowances)
            {
                // ⚠️ CANCELLED BEFORE IT STARTED. A future-dated grant cannot be "ended early" —
                // giving it an EffectiveTo before its EffectiveFrom is rejected by the entity, and
                // the row was previously left OPEN when that happened: the deputy would have begun
                // drawing an allowance, on its original start date, for cover that was called off.
                // It never took effect, so it is removed rather than shortened.
                if (endedOn.Date < a.EffectiveFrom.Date)
                {
                    employeeAllowances.Delete(a);
                    closed++;
                    continue;
                }

                // Only shorten. An assignment that ran its full course already expires on its end
                // date, and pushing the window OUT would extend an allowance past the cover.
                if (a.EffectiveTo is DateTime to && to <= endedOn.Date) continue;
                a.Update(a.AllowanceTypeId, a.Value, a.EffectiveFrom, endedOn.Date, a.Remark);
                employeeAllowances.UpdateAsync(a);
                closed++;
            }

            var benefits = await enrollments.GetAll()
                .Where(e => e.EmployeeId == assignment.EmployeeId
                            && e.Status == BenefitEnrollmentStatus.Enrolled
                            && e.Remark != null && e.Remark.Contains(tag))
                .ToListAsync();

            foreach (var e in benefits)
            {
                // Same reasoning as the allowances above: an enrolment whose coverage had not begun
                // is removed, not terminated with a coverage end that precedes its start.
                if (endedOn.Date < e.CoverageStart.Date)
                {
                    enrollments.Delete(e);
                    closed++;
                    continue;
                }

                e.Terminate(endedOn.Date, $"{e.Remark} — cover ended");
                enrollments.UpdateAsync(e);
                closed++;
            }

            if (closed > 0) await employeeAllowances.SaveChangesAsync();
            logger.LogInformation("Acting {Id}: withdrew {Count} post entitlement(s) as at {Date:yyyy-MM-dd}",
                assignment.Id, closed, endedOn);
            return closed;
        }
    }
}
