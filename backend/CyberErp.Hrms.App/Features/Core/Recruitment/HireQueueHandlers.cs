using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs -----------------------------------------------------------------

    /// <summary>One hire-ready (or waitlisted) applicant on the "Hire Employee" screen.</summary>
    public class HireQueueRowDto
    {
        public Guid RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public string? RequisitionTitle { get; set; }
        public int NumberOfPositions { get; set; }
        public int HiredCount { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid CandidateId { get; set; }
        public string? CandidateNumber { get; set; }
        public string? CandidateName { get; set; }
        public string Stage { get; set; } = string.Empty;
        public decimal? TotalScore { get; set; }
        public int? Rank { get; set; }
        /// <summary>Eligible | Waitlisted (only these two appear on the queue).</summary>
        public string HireEligibility { get; set; } = string.Empty;
        public string? LatestOfferStatus { get; set; }
        public bool ComplianceComplete { get; set; }
        public List<string> MissingComplianceDocuments { get; set; } = [];
        /// <summary>True when every hire precondition is met (eligible + stage + offer + compliance).</summary>
        public bool CanHire { get; set; }
        /// <summary>The first unmet precondition, for the row's tooltip.</summary>
        public string? BlockedReason { get; set; }
    }

    public interface IGetHireQueue { Task<List<HireQueueRowDto>> GetAsync(); }

    /// <summary>
    /// The "Hire Employee" queue: ONLY fully qualified, ranked applicants of open vacancies —
    /// top-N Eligible first, then the waitlist. Requisitions without weighted criteria fall back
    /// to their Selected/OfferPending applicants (legacy, unranked).
    /// </summary>
    public class GetHireQueue(
        IRepository<JobRequisition> requisitionRepository,
        IRepository<JobApplication> applicationRepository,
        IRepository<CandidateDocument> candidateDocumentRepository,
        IGetApplicationRanking rankingHandler) : IGetHireQueue
    {
        public async Task<List<HireQueueRowDto>> GetAsync()
        {
            // Vacancies still hiring: approved/posted requisitions with any non-terminal application.
            var requisitions = await requisitionRepository.GetAll()
                .Where(q => q.Status == RequisitionStatus.Approved || q.Status == RequisitionStatus.Posted)
                .Select(q => new { q.Id, q.RequisitionNumber, q.Title, q.NumberOfPositions })
                .ToListAsync();
            if (requisitions.Count == 0) return [];

            var requisitionIds = requisitions.Select(q => q.Id).ToList();
            var hasApplications = await applicationRepository.GetAll()
                .Where(a => requisitionIds.Contains(a.RequisitionId))
                .Select(a => a.RequisitionId)
                .Distinct()
                .ToListAsync();

            var result = new List<HireQueueRowDto>();
            foreach (var q in requisitions.Where(q => hasApplications.Contains(q.Id)))
            {
                var ranking = await rankingHandler.GetAsync(q.Id);
                var hired = ranking.Count(r => r.Stage == nameof(ApplicationStage.Hired));
                var hasCriteria = ranking.Any(r => r.TotalCriteria > 0);

                IEnumerable<ApplicationRankingRowDto> pool = hasCriteria
                    // Ranked vacancies: strictly the qualified, ranked applicants.
                    ? ranking.Where(r => r.HireEligibility is "Eligible" or "Waitlisted")
                    // Legacy (no criteria): fall back to hire-stage applicants.
                    : ranking.Where(r => r.Stage is nameof(ApplicationStage.Selected) or nameof(ApplicationStage.OfferPending)
                        or nameof(ApplicationStage.OfferAccepted));

                // One batched read for the whole pool — the previous per-candidate query was a
                // classic N+1 (one DB roundtrip per row on the hire screen).
                var poolRows = pool.ToList();
                var poolCandidateIds = poolRows.Select(r => r.CandidateId).Distinct().ToList();
                var documentsByCandidate = (await candidateDocumentRepository.GetAll().AsNoTracking()
                        .Where(d => poolCandidateIds.Contains(d.CandidateId))
                        .Select(d => new { d.CandidateId, d.DocumentType })
                        .ToListAsync())
                    .GroupBy(d => d.CandidateId)
                    .ToDictionary(g => g.Key, g => g.Select(d => d.DocumentType).ToList());

                foreach (var r in poolRows)
                {
                    var eligibility = hasCriteria ? r.HireEligibility! : "Eligible";
                    var stageOk = r.Stage is nameof(ApplicationStage.Selected) or nameof(ApplicationStage.OfferPending)
                        or nameof(ApplicationStage.OfferAccepted);
                    var offerOk = r.LatestOfferStatus is null or nameof(OfferStatus.Accepted);

                    var missing = CandidateShared.MissingComplianceDocuments(
                        documentsByCandidate.GetValueOrDefault(r.CandidateId) ?? []);

                    var blocked =
                        eligibility != "Eligible" ? $"Waitlisted at rank #{r.Rank} — a higher-ranked candidate holds the slot" :
                        !stageOk ? $"Application is at {r.Stage} — move it to Selected first" :
                        !offerOk ? $"Latest offer is {r.LatestOfferStatus} — an accepted offer is required" :
                        missing.Count > 0 ? $"Missing compliance documents: {string.Join(", ", missing)}" :
                        null;

                    result.Add(new HireQueueRowDto
                    {
                        RequisitionId = q.Id,
                        RequisitionNumber = q.RequisitionNumber,
                        RequisitionTitle = q.Title,
                        NumberOfPositions = q.NumberOfPositions,
                        HiredCount = hired,
                        ApplicationId = r.ApplicationId,
                        CandidateId = r.CandidateId,
                        CandidateNumber = r.CandidateNumber,
                        CandidateName = r.CandidateName,
                        Stage = r.Stage,
                        TotalScore = r.TotalScore,
                        Rank = r.Rank,
                        HireEligibility = eligibility,
                        LatestOfferStatus = r.LatestOfferStatus,
                        ComplianceComplete = missing.Count == 0,
                        MissingComplianceDocuments = missing,
                        CanHire = blocked is null,
                        BlockedReason = blocked
                    });
                }
            }

            return result
                .OrderBy(r => r.RequisitionNumber)
                .ThenBy(r => r.HireEligibility == "Eligible" ? 0 : 1)
                .ThenBy(r => r.Rank ?? int.MaxValue)
                .ToList();
        }
    }
}
