using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    /// <summary>
    /// HC153 — when an appraisal is finalized, recompute the employee's succession readiness so it always
    /// reflects the latest performance history (no manual recompute needed). Reuses the on-demand readiness
    /// computation; a handful of indexed queries per candidacy the employee appears in.
    /// </summary>
    public class SuccessionReadinessRefreshHandler(
        IRepository<SuccessionCandidate> candidateRepository,
        IComputeSuccessionCandidateReadiness readinessHandler,
        ILogger<SuccessionReadinessRefreshHandler> logger) : IAppraisalCompletedHandler
    {
        public async Task OnAppraisalCompletedAsync(Guid appraisalId, Guid employeeId)
        {
            var candidateIds = await candidateRepository.GetAll()
                .Where(c => c.EmployeeId == employeeId).Select(c => c.Id).ToListAsync();
            foreach (var candidateId in candidateIds)
                await readinessHandler.ComputeAsync(candidateId);

            if (candidateIds.Count > 0)
                logger.LogInformation("Refreshed readiness for {N} succession candidacy(ies) of employee {Emp} after appraisal {Appraisal}",
                    candidateIds.Count, employeeId, appraisalId);
        }
    }
}
