using System.Linq.Expressions;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class TalentRatingDto
    {
        public Guid Id { get; set; }
        public Guid RaterEmployeeId { get; set; }
        public string? RaterRole { get; set; }
        public decimal? PerformanceScore { get; set; }
        public decimal? PotentialScore { get; set; }
        public string? Comment { get; set; }
    }

    public class TalentAssessmentDto
    {
        public Guid Id { get; set; }
        public Guid TalentReviewId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public int PerformanceBand { get; set; }
        public int PotentialBand { get; set; }
        public bool IsHiPo { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public string? Notes { get; set; }
        public List<TalentRatingDto> Ratings { get; set; } = [];
    }

    public class SaveTalentRatingDto
    {
        public Guid RaterEmployeeId { get; set; }
        public string? RaterRole { get; set; }
        public decimal? PerformanceScore { get; set; }
        public decimal? PotentialScore { get; set; }
        public string? Comment { get; set; }
    }

    public class SaveTalentAssessmentDto
    {
        public Guid? Id { get; set; }
        public Guid TalentReviewId { get; set; }
        public Guid EmployeeId { get; set; }
        public int PerformanceBand { get; set; } = 2;
        public int PotentialBand { get; set; } = 2;
        public bool IsHiPo { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public string? Notes { get; set; }
        public List<SaveTalentRatingDto> Ratings { get; set; } = [];
    }

    public class SaveTalentAssessmentDtoValidator : AbstractValidator<SaveTalentAssessmentDto>
    {
        public SaveTalentAssessmentDtoValidator()
        {
            RuleFor(x => x.TalentReviewId).NotEmpty();
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.PerformanceBand).InclusiveBetween(1, 3);
            RuleFor(x => x.PotentialBand).InclusiveBetween(1, 3);
            RuleFor(x => x.Readiness).NotEmpty().Must(v => Enum.TryParse<ReadinessLevel>(v, out _)).WithMessage("Invalid readiness level.");
            RuleFor(x => x.Notes).MaximumLength(2000);
            RuleForEach(x => x.Ratings).ChildRules(r =>
            {
                r.RuleFor(x => x.RaterEmployeeId).NotEmpty();
                r.RuleFor(x => x.RaterRole).MaximumLength(100);
                r.RuleFor(x => x.Comment).MaximumLength(1000);
            });
        }
    }

    internal static class TalentAssessmentMapper
    {
        internal static readonly Expression<Func<TalentAssessment, TalentAssessmentDto>> Projection = a => new TalentAssessmentDto
        {
            Id = a.Id,
            TalentReviewId = a.TalentReviewId,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null && a.Employee.Person != null
                ? (a.Employee.Person.FirstName + " " + a.Employee.Person.GrandFatherName) : null,
            EmployeeNumber = a.Employee != null ? a.Employee.EmployeeNumber : null,
            PerformanceBand = a.PerformanceBand,
            PotentialBand = a.PotentialBand,
            IsHiPo = a.IsHiPo,
            Readiness = a.Readiness.ToString(),
            Notes = a.Notes,
            Ratings = a.Ratings.Select(r => new TalentRatingDto
            {
                Id = r.Id, RaterEmployeeId = r.RaterEmployeeId, RaterRole = r.RaterRole,
                PerformanceScore = r.PerformanceScore, PotentialScore = r.PotentialScore, Comment = r.Comment
            }).ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted ratings copy it here.</summary>
        internal static void StampRatingTenant(TalentAssessment a)
        {
            foreach (var r in a.Ratings)
                if (string.IsNullOrEmpty(r.TenantId)) r.TenantId = a.TenantId;
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTalentAssessment { Task<Guid> SaveAsync(SaveTalentAssessmentDto dto); }
    public interface IDeleteTalentAssessment { Task DeleteAsync(Guid id); }
    public interface IGetTalentAssessmentById { Task<TalentAssessmentDto> GetAsync(Guid id); }
    public interface IGetAllTalentAssessments { Task<PaginatedResponse<TalentAssessmentDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTalentAssessment(
        IRepository<TalentAssessment> repository,
        IRepository<TalentRating> ratingRepository,
        IRepository<TalentReview> reviewRepository,
        IValidator<SaveTalentAssessmentDto> validator,
        ILogger<SaveTalentAssessment> logger) : ISaveTalentAssessment
    {
        public async Task<Guid> SaveAsync(SaveTalentAssessmentDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Calibration cannot proceed under a review that has not cleared (or has failed) approval.
            var reviewStatus = await reviewRepository.GetAll()
                .Where(r => r.Id == dto.TalentReviewId)
                .Select(r => (TalentReviewStatus?)r.Status)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TalentReview), dto.TalentReviewId.ToString());
            if (reviewStatus is TalentReviewStatus.PendingApproval or TalentReviewStatus.Rejected)
                throw new ValidationException(nameof(dto.TalentReviewId),
                    "This talent review is awaiting workflow approval — assessments can be recorded once it is approved.");

            // One assessment per employee per review.
            if (await repository.GetAll().AnyAsync(x => x.TalentReviewId == dto.TalentReviewId
                    && x.EmployeeId == dto.EmployeeId && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(TalentAssessment), nameof(dto.EmployeeId), dto.EmployeeId.ToString());

            var readiness = Enum.Parse<ReadinessLevel>(dto.Readiness);
            var specs = dto.Ratings.Select(r => new TalentRatingSpec(
                r.RaterEmployeeId, r.RaterRole, r.PerformanceScore, r.PotentialScore, r.Comment)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Ratings)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TalentAssessment), dto.Id.Value.ToString());

                foreach (var old in entity.Ratings.ToList()) ratingRepository.Delete(old);
                entity.Update(dto.EmployeeId, dto.PerformanceBand, dto.PotentialBand, dto.IsHiPo, readiness, dto.Notes);
                entity.SetRatings(specs);
                TalentAssessmentMapper.StampRatingTenant(entity);
                foreach (var r in entity.Ratings) await ratingRepository.AddAsync(r);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = TalentAssessment.Create(dto.TalentReviewId, dto.EmployeeId, dto.PerformanceBand,
                dto.PotentialBand, dto.IsHiPo, readiness, dto.Notes);
            created.SetRatings(specs);
            await repository.AddAsync(created);
            TalentAssessmentMapper.StampRatingTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TalentAssessment {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteTalentAssessment(IRepository<TalentAssessment> repository) : IDeleteTalentAssessment
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TalentAssessment), id.ToString());
            repository.Delete(entity); // cascade removes ratings
            await repository.SaveChangesAsync();
        }
    }

    public class GetTalentAssessmentById(IRepository<TalentAssessment> repository) : IGetTalentAssessmentById
    {
        public async Task<TalentAssessmentDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(TalentAssessmentMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(TalentAssessment), id.ToString());
    }

    public class GetAllTalentAssessments(IRepository<TalentAssessment> repository) : IGetAllTalentAssessments
    {
        public async Task<PaginatedResponse<TalentAssessmentDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            // Scoped to a talent review (parentId) — the standard tree/grid drilldown.
            if (request.ParentId.HasValue)
                query = query.Where(x => x.TalentReviewId == request.ParentId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Employee != null && x.Employee.Person != null
                    && (x.Employee.Person.FirstName.Contains(term) || x.Employee.EmployeeNumber.Contains(term)));
            }

            var total = await query.CountAsync();
            // List view is lightweight — the multi-rater rows are only loaded on GetById.
            var data = await query.OrderByDescending(x => x.PerformanceBand).ThenByDescending(x => x.PotentialBand)
                .Skip(skip).Take(take)
                .Select(a => new TalentAssessmentDto
                {
                    Id = a.Id, TalentReviewId = a.TalentReviewId, EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee != null && a.Employee.Person != null
                        ? (a.Employee.Person.FirstName + " " + a.Employee.Person.GrandFatherName) : null,
                    EmployeeNumber = a.Employee != null ? a.Employee.EmployeeNumber : null,
                    PerformanceBand = a.PerformanceBand, PotentialBand = a.PotentialBand,
                    IsHiPo = a.IsHiPo, Readiness = a.Readiness.ToString(), Notes = a.Notes
                }).ToListAsync();

            return new PaginatedResponse<TalentAssessmentDto> { Total = total, Data = data };
        }
    }
}
