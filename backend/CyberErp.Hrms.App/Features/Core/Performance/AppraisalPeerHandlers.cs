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
        IAppraisalWorkflowService workflowService,
        IPerformanceHistoryWriter history,
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

            var invited = 0;
            foreach (var peerId in dto.PeerEmployeeIds.Distinct())
            {
                if (peerId == Guid.Empty || existing.Contains(peerId)) continue;
                if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == peerId))
                    throw new NotFoundException(nameof(Employee), peerId.ToString());
                await repository.AddAsync(AppraisalPeerReview.Create(dto.AppraisalId, peerId));
                invited++;
            }

            if (invited > 0)
                await history.WriteAsync("Appraisal", dto.AppraisalId, "PeerInvited", $"Invited {invited} peer reviewer(s).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Invited {Count} peers to Appraisal {Id}", invited, dto.AppraisalId);
        }
    }

    public class SubmitAppraisalPeerReview(
        IRepository<AppraisalPeerReview> repository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser,
        IPerformanceHistoryWriter history,
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
