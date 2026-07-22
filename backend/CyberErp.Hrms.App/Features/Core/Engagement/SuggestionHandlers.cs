using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Engagement
{
    // ---- DTOs ---------------------------------------------------------------
    public class SuggestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public Guid? EmployeeId { get; set; }
        /// <summary>"Anonymous" for anonymous rows — no identifying data ever leaves the API.</summary>
        public string? EmployeeName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ManagementResponse { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? RespondedOn { get; set; }
    }

    public class SubmitSuggestionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        /// <summary>HC207 — no employee link and no CreatedBy stamp when set.</summary>
        public bool IsAnonymous { get; set; }
    }

    public class RespondSuggestionDto
    {
        public Guid Id { get; set; }
        /// <summary>UnderReview | Actioned | Closed.</summary>
        public string Status { get; set; } = nameof(SuggestionStatus.UnderReview);
        public string? ManagementResponse { get; set; }
    }

    public class SubmitSuggestionDtoValidator : AbstractValidator<SubmitSuggestionDto>
    {
        public SubmitSuggestionDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        }
    }

    public class RespondSuggestionDtoValidator : AbstractValidator<RespondSuggestionDto>
    {
        public RespondSuggestionDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status)
                .Must(v => Enum.TryParse<SuggestionStatus>(v, true, out var st) && st != SuggestionStatus.New)
                .WithMessage("Status must be UnderReview, Actioned or Closed.");
            RuleFor(x => x.ManagementResponse).MaximumLength(2000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitSuggestion { Task<Guid> SubmitAsync(SubmitSuggestionDto dto); }
    public interface IRespondSuggestion { Task RespondAsync(RespondSuggestionDto dto); }
    public interface IDeleteSuggestion { Task DeleteAsync(Guid id); }
    public interface IGetAllSuggestions { Task<PaginatedResponse<SuggestionDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SubmitSuggestion(
        IRepository<Suggestion> repository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SubmitSuggestionDto> validator,
        ILogger<SubmitSuggestion> logger) : ISubmitSuggestion
    {
        public async Task<Guid> SubmitAsync(SubmitSuggestionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException(nameof(dto.Title), "Your account is not linked to an employee record.");

            var created = Suggestion.Create(dto.Title.Trim(), dto.Body.Trim(), dto.IsAnonymous,
                dto.IsAnonymous ? null : scope.EmployeeId, DateTime.UtcNow.Date);
            await repository.AddAsync(created);
            // HC207 — AddAsync stamps CreatedBy with the current user; re-clearing it BEFORE the save
            // leaves nothing identifying in the row (and Suggestion is not IAuditable, so the audit
            // trail records nothing either).
            if (dto.IsAnonymous) created.Create(null);
            // HC209 — NAMED suggestions earn engagement points; anonymous ones earn nothing
            // (crediting the ledger would link the submission to a person).
            if (!dto.IsAnonymous)
                await pointsRepository.AddAsync(RewardPointsTransaction.Create(scope.EmployeeId.Value,
                    EngagementPoints.Suggestion, RewardPointsSource.Engagement, DateTime.UtcNow.Date,
                    created.Id, "Engagement: suggestion"));
            await repository.SaveChangesAsync();
            // Deliberately no submitter in this log line for anonymous rows.
            logger.LogInformation("Suggestion {Id} submitted ({Mode})", created.Id, dto.IsAnonymous ? "anonymous" : "named");
            return created.Id;
        }
    }

    public class RespondSuggestion(
        IRepository<Suggestion> repository,
        IPerformanceVisibilityService visibility,
        IValidator<RespondSuggestionDto> validator,
        ILogger<RespondSuggestion> logger) : IRespondSuggestion
    {
        public async Task RespondAsync(RespondSuggestionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(dto.Id), "Only HR administrators can respond to suggestions.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Suggestion), dto.Id.ToString());
            entity.Respond(Enum.Parse<SuggestionStatus>(dto.Status, true),
                string.IsNullOrWhiteSpace(dto.ManagementResponse) ? null : dto.ManagementResponse.Trim(),
                DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Suggestion {Id} moved to {Status}", dto.Id, dto.Status);
        }
    }

    public class DeleteSuggestion(
        IRepository<Suggestion> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteSuggestion> logger) : IDeleteSuggestion
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR administrators can delete suggestions.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Suggestion), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted suggestion {Id}", id);
        }
    }

    public class GetAllSuggestions(
        IRepository<Suggestion> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllSuggestions
    {
        public async Task<PaginatedResponse<SuggestionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // HR reviews everything; an employee sees only their own NAMED submissions —
            // anonymous rows are unlinkable by design, even to their author.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                query = query.Where(x => x.EmployeeId == myEmp);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<SuggestionStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Title.Contains(term) || x.Body.Contains(term));
            }

            var employees = employeeRepository.GetAll();
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.SubmittedOn).ThenByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new SuggestionDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Body = x.Body,
                    IsAnonymous = x.IsAnonymous,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.IsAnonymous
                        ? "Anonymous"
                        : employees.Where(e => e.Id == x.EmployeeId)
                            .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                            .FirstOrDefault(),
                    Status = x.Status.ToString(),
                    ManagementResponse = x.ManagementResponse,
                    SubmittedOn = x.SubmittedOn,
                    RespondedOn = x.RespondedOn
                }).ToListAsync();

            return new PaginatedResponse<SuggestionDto> { Total = total, Data = data };
        }
    }
}
