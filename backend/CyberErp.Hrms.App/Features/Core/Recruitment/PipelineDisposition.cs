using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    /// <summary>
    /// Shared pipeline clean-up: whenever a vacancy or candidate leaves the process (vacancy
    /// filled/closed/cancelled, candidate hired elsewhere or anonymized), the applications and
    /// offers left behind are DISPOSITIONED — never stranded in active stages. Every move is
    /// stage-logged; live offers are withdrawn with the same note.
    /// </summary>
    internal static class PipelineDisposition
    {
        internal static bool IsActive(ApplicationStage stage) =>
            stage is not (ApplicationStage.Hired or ApplicationStage.Rejected or ApplicationStage.Withdrawn);

        /// <summary>
        /// Moves the given ACTIVE applications to a final stage (Rejected | Withdrawn) with a
        /// logged note, withdrawing their live offers (Draft/Approved/Sent). Offers stuck at
        /// PendingApproval stay with their running workflow — resolved from the workflow screen.
        /// Applications must be loaded WITH their stage log. Nothing is saved here — the caller
        /// owns the transaction.
        /// </summary>
        internal static async Task CloseOutAsync(
            IRepository<JobApplication> applicationRepository,
            IRepository<JobApplicationStageLog> stageLogRepository,
            IRepository<JobOffer> offerRepository,
            IReadOnlyCollection<JobApplication> applications,
            ApplicationStage finalStage, string note, string? actedBy)
        {
            var active = applications.Where(a => IsActive(a.Stage)).ToList();
            if (active.Count == 0) return;

            var ids = active.Select(a => a.Id).ToList();
            var liveOffers = await offerRepository.GetAll()
                .Where(o => ids.Contains(o.ApplicationId) &&
                    (o.Status == OfferStatus.Draft || o.Status == OfferStatus.Approved || o.Status == OfferStatus.Sent))
                .ToListAsync();
            foreach (var offer in liveOffers)
            {
                offer.Withdraw(note);
                offerRepository.UpdateAsync(offer);
            }

            foreach (var application in active)
            {
                var before = application.StageLog.Select(l => l.Id).ToHashSet();
                application.MoveToStage(finalStage, note, actedBy);
                foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
                {
                    if (string.IsNullOrEmpty(log.TenantId)) log.TenantId = application.TenantId;
                    await stageLogRepository.AddAsync(log);
                }
                applicationRepository.UpdateAsync(application);
            }
        }
    }
}
