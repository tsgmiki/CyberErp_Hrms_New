using System.Linq.Expressions;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class TalentReviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Cycle { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string Status { get; set; } = nameof(TalentReviewStatus.Draft);
        public string? Notes { get; set; }
    }

    public class SaveTalentReviewDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Cycle { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string Status { get; set; } = nameof(TalentReviewStatus.Draft);
        public string? Notes { get; set; }
    }

    public class SaveTalentReviewDtoValidator : AbstractValidator<SaveTalentReviewDto>
    {
        public SaveTalentReviewDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Cycle).MaximumLength(60);
            RuleFor(x => x.Status).NotEmpty().Must(v => Enum.TryParse<TalentReviewStatus>(v, out _)).WithMessage("Invalid status.");
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    /// <summary>One cell of the 9-box grid (HC150) — a performance × potential band and its head-count.</summary>
    public class NineBoxCellDto
    {
        public int PerformanceBand { get; set; }
        public int PotentialBand { get; set; }
        public int Count { get; set; }
    }

    public class NineBoxDto
    {
        public Guid TalentReviewId { get; set; }
        public int Total { get; set; }
        public int HiPoCount { get; set; }
        public List<NineBoxCellDto> Cells { get; set; } = [];
    }

    internal static class TalentReviewMapper
    {
        internal static readonly Expression<Func<TalentReview, TalentReviewDto>> Projection = r => new TalentReviewDto
        {
            Id = r.Id,
            Name = r.Name,
            Cycle = r.Cycle,
            OrganizationUnitId = r.OrganizationUnitId,
            Status = r.Status.ToString(),
            Notes = r.Notes
        };
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTalentReview { Task<Guid> SaveAsync(SaveTalentReviewDto dto); }
    public interface IDeleteTalentReview { Task DeleteAsync(Guid id); }
    public interface IGetTalentReviewById { Task<TalentReviewDto> GetAsync(Guid id); }
    public interface IGetAllTalentReviews { Task<PaginatedResponse<TalentReviewDto>> GetAsync(GetAllRequest request); }
    public interface IGetTalentReviewNineBox { Task<NineBoxDto> GetAsync(Guid talentReviewId); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTalentReview(
        IRepository<TalentReview> repository,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IValidator<SaveTalentReviewDto> validator,
        ILogger<SaveTalentReview> logger) : ISaveTalentReview
    {
        public async Task<Guid> SaveAsync(SaveTalentReviewDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var status = Enum.Parse<TalentReviewStatus>(dto.Status);

            // HC149 — when an active approval chain governs talent reviews, a session must pass
            // through it before calibration begins. Without one the module operates directly.
            var workflowActive = await workflowDefinitions.GetAll()
                .AnyAsync(d => d.EntityType == WorkflowEntityTypes.TalentReview && d.IsActive);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                // No edits while an approval is in flight.
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.TalentReview, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TalentReview), dto.Id.Value.ToString());

                // Approval outcomes are workflow-owned: a Rejected/parked review cannot be hand-flipped
                // to an operational status — saving it RESUBMITS the review through the chain instead.
                var resubmit = workflowActive && entity.Status
                    is TalentReviewStatus.Rejected or TalentReviewStatus.PendingApproval;
                if (resubmit)
                {
                    await workflowService.EnsureStartableAsync(WorkflowEntityTypes.TalentReview, null);
                    status = TalentReviewStatus.PendingApproval;
                }

                entity.Update(dto.Name, dto.Cycle, dto.OrganizationUnitId, status, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();

                if (resubmit)
                    await workflowService.StartIfDefinedAsync(
                        WorkflowEntityTypes.TalentReview, entity.Id, null,
                        $"Talent review '{entity.Name}' (resubmitted)");
                return entity.Id;
            }

            if (workflowActive)
            {
                // Fail BEFORE persisting when the chain could never complete (unresolvable approvers).
                await workflowService.EnsureStartableAsync(WorkflowEntityTypes.TalentReview, null);
                status = TalentReviewStatus.PendingApproval;
            }

            var created = TalentReview.Create(dto.Name, dto.Cycle, dto.OrganizationUnitId, dto.Notes);
            if (status != TalentReviewStatus.Draft)
                created.Update(dto.Name, dto.Cycle, dto.OrganizationUnitId, status, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();

            if (workflowActive)
                await workflowService.StartIfDefinedAsync(
                    WorkflowEntityTypes.TalentReview, created.Id, null,
                    $"Talent review '{created.Name}'");

            logger.LogInformation("Created TalentReview {Id} ({Name}){Workflow}", created.Id, created.Name,
                workflowActive ? " — submitted for approval" : string.Empty);
            return created.Id;
        }
    }

    public class DeleteTalentReview(
        IRepository<TalentReview> repository,
        IWorkflowGate workflowGate,
        ILogger<DeleteTalentReview> logger) : IDeleteTalentReview
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.TalentReview, id);
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TalentReview), id.ToString());
            repository.Delete(entity); // FK cascade removes its assessments + ratings
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TalentReview {Id}", id);
        }
    }

    /// <summary>
    /// Applies the workflow outcome to a talent review (HC149): approval moves the session straight
    /// into calibration (InProgress), rejection returns it to an editable Rejected state.
    /// </summary>
    public class TalentReviewWorkflowHandler(
        IRepository<TalentReview> repository,
        ILogger<TalentReviewWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.TalentReview, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var review = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (review is null) return; // deleted mid-flight — nothing to apply
            review.ApproveViaWorkflow();
            repository.UpdateAsync(review);
            await repository.SaveChangesAsync();
            logger.LogInformation("TalentReview {Id} approved via workflow — now InProgress", entityId);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var review = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (review is null) return;
            review.RejectViaWorkflow();
            repository.UpdateAsync(review);
            await repository.SaveChangesAsync();
            logger.LogInformation("TalentReview {Id} rejected via workflow", entityId);
        }
    }

    public class GetTalentReviewById(IRepository<TalentReview> repository) : IGetTalentReviewById
    {
        public async Task<TalentReviewDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(TalentReviewMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(TalentReview), id.ToString());
    }

    public class GetAllTalentReviews(IRepository<TalentReview> repository) : IGetAllTalentReviews
    {
        public async Task<PaginatedResponse<TalentReviewDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.Cycle != null && x.Cycle.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TalentReviewStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take).Select(TalentReviewMapper.Projection).ToListAsync();

            return new PaginatedResponse<TalentReviewDto> { Total = total, Data = data };
        }
    }

    /// <summary>9-box / heat-map counts (HC150) — a single server-side GROUP BY over the covering index.</summary>
    public class GetTalentReviewNineBox(IRepository<TalentAssessment> assessmentRepository) : IGetTalentReviewNineBox
    {
        public async Task<NineBoxDto> GetAsync(Guid talentReviewId)
        {
            var baseQuery = assessmentRepository.GetAll().Where(a => a.TalentReviewId == talentReviewId);
            var cells = await baseQuery
                .GroupBy(a => new { a.PerformanceBand, a.PotentialBand })
                .Select(g => new NineBoxCellDto { PerformanceBand = g.Key.PerformanceBand, PotentialBand = g.Key.PotentialBand, Count = g.Count() })
                .ToListAsync();
            var total = cells.Sum(c => c.Count);
            var hiPo = await baseQuery.CountAsync(a => a.IsHiPo);
            return new NineBoxDto { TalentReviewId = talentReviewId, Total = total, HiPoCount = hiPo, Cells = cells };
        }
    }
}
