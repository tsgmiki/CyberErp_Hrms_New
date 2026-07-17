using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
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

    // ---- Interfaces ---------------------------------------------------------
    public interface IInviteAppraisalPeers { Task InviteAsync(InviteAppraisalPeersDto dto); }
    public interface ISubmitAppraisalPeerReview { Task SubmitAsync(SubmitAppraisalPeerReviewDto dto); }
    public interface IRemoveAppraisalPeerReview { Task RemoveAsync(Guid id); }
    public interface IGetAppraisalPeerReviews { Task<List<AppraisalPeerReviewDto>> GetAsync(Guid appraisalId); }

    // ---- Handlers -----------------------------------------------------------
    public class InviteAppraisalPeers(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceHistoryWriter history,
        IValidator<InviteAppraisalPeersDto> validator,
        ILogger<InviteAppraisalPeers> logger) : IInviteAppraisalPeers
    {
        public async Task InviteAsync(InviteAppraisalPeersDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await appraisalRepository.GetAll().AnyAsync(a => a.Id == dto.AppraisalId))
                throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.ToString());

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
        IPerformanceHistoryWriter history,
        ILogger<SubmitAppraisalPeerReview> logger) : ISubmitAppraisalPeerReview
    {
        public async Task SubmitAsync(SubmitAppraisalPeerReviewDto dto)
        {
            var review = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AppraisalPeerReview), dto.Id.ToString());
            review.Submit(dto.Score, dto.Comments);
            await history.WriteAsync("Appraisal", review.AppraisalId, "PeerSubmitted",
                $"Peer review submitted (score {dto.Score?.ToString() ?? "—"}).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Submitted peer review {Id}", dto.Id);
        }
    }

    public class RemoveAppraisalPeerReview(
        IRepository<AppraisalPeerReview> repository,
        ILogger<RemoveAppraisalPeerReview> logger) : IRemoveAppraisalPeerReview
    {
        public async Task RemoveAsync(Guid id)
        {
            var review = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AppraisalPeerReview), id.ToString());
            repository.Delete(review);
            await repository.SaveChangesAsync();
            logger.LogInformation("Removed peer review {Id}", id);
        }
    }

    public class GetAppraisalPeerReviews(
        IRepository<AppraisalPeerReview> repository,
        IRepository<Employee> employeeRepository) : IGetAppraisalPeerReviews
    {
        public async Task<List<AppraisalPeerReviewDto>> GetAsync(Guid appraisalId)
        {
            var reviews = await repository.GetAll().AsNoTracking()
                .Where(p => p.AppraisalId == appraisalId).ToListAsync();
            var employees = employeeRepository.GetAll();
            var data = new List<AppraisalPeerReviewDto>(reviews.Count);
            foreach (var r in reviews)
            {
                var name = await employees.Where(e => e.Id == r.PeerEmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
                data.Add(new AppraisalPeerReviewDto
                {
                    Id = r.Id,
                    AppraisalId = r.AppraisalId,
                    PeerEmployeeId = r.PeerEmployeeId,
                    PeerEmployeeName = name,
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
