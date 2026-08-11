using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class AppraisalPeerReviewDto
    {
        public Guid Id { get; set; }
        public Guid AppraisalId { get; set; }
        public Guid PeerEmployeeId { get; set; }
        public string? PeerEmployeeName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class InviteAppraisalPeersDto
    {
        public Guid AppraisalId { get; set; }
        public List<Guid> PeerEmployeeIds { get; set; } = [];
    }

    public class SubmitAppraisalPeerReviewDto
    {
        public Guid Id { get; set; }
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
    }

    public class InviteAppraisalPeersDtoValidator : AbstractValidator<InviteAppraisalPeersDto>
    {
        public InviteAppraisalPeersDtoValidator()
        {
            RuleFor(x => x.AppraisalId).NotEmpty();
            RuleFor(x => x.PeerEmployeeIds).NotEmpty().WithMessage("Select at least one peer.");
        }
    }

    /// <summary>A peer-review assignment as seen by the PEER reviewer — the appraisee + cycle only, never the
    /// self/manager ratings (peers assess independently).</summary>
    public class MyPeerReviewDto
    {
        public Guid Id { get; set; }
        public Guid AppraisalId { get; set; }
        public string? EmployeeName { get; set; }
        public string? ReviewCycleName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IInviteAppraisalPeers { Task InviteAsync(InviteAppraisalPeersDto dto); }
    public interface ISubmitAppraisalPeerReview { Task SubmitAsync(SubmitAppraisalPeerReviewDto dto); }
    public interface IRemoveAppraisalPeerReview { Task RemoveAsync(Guid id); }
    public interface IGetAppraisalPeerReviews { Task<List<AppraisalPeerReviewDto>> GetAsync(Guid appraisalId); }
    public interface IGetMyPeerReviews { Task<List<MyPeerReviewDto>> GetAsync(); }

    // ---- Handlers -----------------------------------------------------------
    public class InviteAppraisalPeers(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<Employee> employeeRepository,
        IRepository<User> userRepository,
        IAppraisalWorkflowService workflowService,
        IPerformanceHistoryWriter history,
        IPortalNotifier portalNotifier,
        IValidator<InviteAppraisalPeersDto> validator,
        ILogger<InviteAppraisalPeers> logger) : IInviteAppraisalPeers
    {
        public async Task InviteAsync(InviteAppraisalPeersDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await appraisalRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AppraisalId)
                ?? throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.ToString());

            // Peer invitations belong to the appraisee's manager (or HR) — not the appraisee, not bystanders.
            if (!await workflowService.CanAdministerAsync() && !await workflowService.CanManageEmployeeAsync(appraisal.EmployeeId))
                throw new ValidationException(nameof(dto.AppraisalId), "Only the employee's manager or an HR administrator can invite peer reviewers.");

            // 360 integrity: the employee being appraised can never be their own peer reviewer.
            if (dto.PeerEmployeeIds.Contains(appraisal.EmployeeId))
                throw new ValidationException(nameof(dto.PeerEmployeeIds), "The employee being appraised cannot be invited as their own peer reviewer.");

            var existing = await repository.GetAll()
                .Where(p => p.AppraisalId == dto.AppraisalId)
                .Select(p => p.PeerEmployeeId).ToListAsync();

            // Validate all invited employees exist in ONE query (was an AnyAsync per peer).
            var candidateIds = dto.PeerEmployeeIds.Where(pid => pid != Guid.Empty).Distinct().ToList();
            var validIds = (await employeeRepository.GetAll()
                .Where(e => candidateIds.Contains(e.Id)).Select(e => e.Id).ToListAsync()).ToHashSet();

            var invited = 0;
            // (review id, peer employee id) — each peer's alert is correlated to THEIR OWN review row so
            // that one peer submitting clears only their alert, not every peer's.
            var invitedReviews = new List<(Guid ReviewId, Guid PeerEmployeeId)>();
            foreach (var peerId in candidateIds)
            {
                if (existing.Contains(peerId)) continue;
                if (!validIds.Contains(peerId))
                    throw new NotFoundException(nameof(Employee), peerId.ToString());
                var review = AppraisalPeerReview.Create(dto.AppraisalId, peerId);
                await repository.AddAsync(review);
                invitedReviews.Add((review.Id, peerId));
                invited++;
            }

            if (invited > 0)
                await history.WriteAsync("Appraisal", dto.AppraisalId, "PeerInvited", $"Invited {invited} peer reviewer(s).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Invited {Count} peers to Appraisal {Id}", invited, dto.AppraisalId);

            // Tell the peers. Without this the assignment was silent — it only appeared if the reviewer
            // happened to open My Peer Reviews. Raised AFTER SaveChanges so an alert never points at a
            // review that failed to persist, and best-effort so a portal hiccup cannot fail the invite.
            if (invitedReviews.Count > 0)
                await NotifyInvitedPeersAsync(dto.AppraisalId, appraisal.EmployeeId, invitedReviews);
        }

        private async Task NotifyInvitedPeersAsync(
            Guid appraisalId, Guid appraiseeId, List<(Guid ReviewId, Guid PeerEmployeeId)> invitedReviews)
        {
            try
            {
                // Notifications address Core.User ids, not employees; a peer with no portal account is
                // simply skipped rather than failing the invite.
                var peerIds = invitedReviews.Select(r => r.PeerEmployeeId).ToList();
                var userByEmployee = await userRepository.GetAll().AsNoTracking()
                    .Where(u => u.EmployeeId != null && peerIds.Contains(u.EmployeeId.Value))
                    .Select(u => new { EmployeeId = u.EmployeeId!.Value, u.Id })
                    .ToDictionaryAsync(x => x.EmployeeId, x => x.Id);
                if (userByEmployee.Count == 0)
                {
                    logger.LogInformation(
                        "Appraisal {Id}: {Count} peer(s) invited but none has a portal account — no alert raised.",
                        appraisalId, invitedReviews.Count);
                    return;
                }

                var appraiseeName = await employeeRepository.GetAll().AsNoTracking()
                    .Where(e => e.Id == appraiseeId && e.Person != null)
                    .Select(e => (e.Person!.FirstName + " " + e.Person.GrandFatherName).Trim())
                    .FirstOrDefaultAsync();
                var body = string.IsNullOrWhiteSpace(appraiseeName)
                    ? "You have been asked to complete a peer review."
                    : $"You have been asked to complete a peer review for {appraiseeName}.";

                // One call per review so each alert carries its own correlation id (see above).
                foreach (var (reviewId, peerEmployeeId) in invitedReviews)
                {
                    if (!userByEmployee.TryGetValue(peerEmployeeId, out var userId)) continue;
                    await portalNotifier.NotifyUsersAsync(
                        [userId], "Peer review assigned", body, "/myPeerReviews",
                        "Action", PeerReviewSource, reviewId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Appraisal {Id}: failed to raise peer-review portal alerts", appraisalId);
            }
        }

        /// <summary>Correlation key for peer-review alerts — kept distinct from the appraisal's own
        /// workflow alerts so resolving one never clears the other.</summary>
        internal const string PeerReviewSource = "AppraisalPeerReview";
    }

    public class SubmitAppraisalPeerReview(
        IRepository<AppraisalPeerReview> repository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser,
        IPerformanceHistoryWriter history,
        IPortalNotifier portalNotifier,
        ILogger<SubmitAppraisalPeerReview> logger) : ISubmitAppraisalPeerReview
    {
        public async Task SubmitAsync(SubmitAppraisalPeerReviewDto dto)
        {
            var review = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AppraisalPeerReview), dto.Id.ToString());

            // Self-service: a peer review is submitted by the assigned peer themselves — nobody else.
            var userId = currentUser.GetCurrentUserId();
            var myEmployeeId = userId is null ? null : await userRepository.GetAll()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
            if (myEmployeeId is null || myEmployeeId.Value != review.PeerEmployeeId)
                throw new ValidationException("peer", "You can only submit your own peer review.");

            review.Submit(dto.Score, dto.Comments);
            await history.WriteAsync("Appraisal", review.AppraisalId, "PeerSubmitted",
                $"Peer review submitted (score {dto.Score?.ToString() ?? "—"}).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Submitted peer review {Id}", dto.Id);

            // The assignment alert has served its purpose — clear it so the bell does not keep nagging
            // for work already done. Best-effort: never fail a submitted review over a portal write.
            try { await portalNotifier.ResolveAsync(InviteAppraisalPeers.PeerReviewSource, dto.Id); }
            catch (Exception ex) { logger.LogWarning(ex, "Peer review {Id}: failed to clear its portal alert", dto.Id); }
        }
    }

    /// <summary>The current user's own peer-review assignments (the peer's worklist).</summary>
    public class GetMyPeerReviews(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetMyPeerReviews
    {
        public async Task<List<MyPeerReviewDto>> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return [];
            var myEmployeeId = await userRepository.GetAll()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
            if (myEmployeeId is null) return [];

            var reviews = await repository.GetAll().AsNoTracking()
                .Where(p => p.PeerEmployeeId == myEmployeeId.Value).ToListAsync();
            if (reviews.Count == 0) return [];

            // PERFORMANCE: batch-load appraisal meta + names in 3 queries total (was 3 PER review).
            // Only the appraisee + cycle — never the self/manager scores (peers assess independently).
            var appraisalIds = reviews.Select(r => r.AppraisalId).Distinct().ToList();
            var metas = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => appraisalIds.Contains(a.Id))
                .Select(a => new { a.Id, a.EmployeeId, a.ReviewCycleId })
                .ToDictionaryAsync(a => a.Id);
            var metaEmpIds = metas.Values.Select(m => m.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => metaEmpIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            var metaCycleIds = metas.Values.Select(m => m.ReviewCycleId).Distinct().ToList();
            var cycleNames = await reviewCycleRepository.GetAll().AsNoTracking()
                .Where(c => metaCycleIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<MyPeerReviewDto>(reviews.Count);
            foreach (var r in reviews)
            {
                var meta = metas.GetValueOrDefault(r.AppraisalId);
                data.Add(new MyPeerReviewDto
                {
                    Id = r.Id,
                    AppraisalId = r.AppraisalId,
                    EmployeeName = meta is null ? null : employeeNames.GetValueOrDefault(meta.EmployeeId),
                    ReviewCycleName = meta is null ? null : cycleNames.GetValueOrDefault(meta.ReviewCycleId),
                    Status = r.Status.ToString(),
                    Score = r.Score,
                    Comments = r.Comments,
                    SubmittedAt = r.SubmittedAt
                });
            }
            return data;
        }
    }

    public class RemoveAppraisalPeerReview(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Appraisal> appraisalRepository,
        IAppraisalWorkflowService workflowService,
        ILogger<RemoveAppraisalPeerReview> logger) : IRemoveAppraisalPeerReview
    {
        public async Task RemoveAsync(Guid id)
        {
            var review = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AppraisalPeerReview), id.ToString());
            var appraisal = await appraisalRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(a => a.Id == review.AppraisalId)
                ?? throw new NotFoundException(nameof(Appraisal), review.AppraisalId.ToString());
            if (!await workflowService.CanAdministerAsync() && !await workflowService.CanManageEmployeeAsync(appraisal.EmployeeId))
                throw new ValidationException(nameof(id), "Only the employee's manager or an HR administrator can remove a peer reviewer.");
            repository.Delete(review);
            await repository.SaveChangesAsync();
            logger.LogInformation("Removed peer review {Id}", id);
        }
    }

    public class GetAppraisalPeerReviews(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAppraisalPeerReviews
    {
        public async Task<List<AppraisalPeerReviewDto>> GetAsync(Guid appraisalId)
        {
            // Individual peer scores/identities are HR-only — the appraisee sees only the average elsewhere.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR can view individual peer reviews.");

            var reviews = await repository.GetAll().AsNoTracking()
                .Where(p => p.AppraisalId == appraisalId).ToListAsync();

            // PERFORMANCE: batch-load the peer names in ONE query (was one per review).
            var peerIds = reviews.Select(r => r.PeerEmployeeId).Distinct().ToList();
            var peerNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => peerIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<AppraisalPeerReviewDto>(reviews.Count);
            foreach (var r in reviews)
            {
                data.Add(new AppraisalPeerReviewDto
                {
                    Id = r.Id,
                    AppraisalId = r.AppraisalId,
                    PeerEmployeeId = r.PeerEmployeeId,
                    PeerEmployeeName = peerNames.GetValueOrDefault(r.PeerEmployeeId),
                    Status = r.Status.ToString(),
                    Score = r.Score,
                    Comments = r.Comments,
                    SubmittedAt = r.SubmittedAt
                });
            }
            return data;
        }
    }
}
