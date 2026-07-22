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
    public class GrievanceNoteDto
    {
        public Guid Id { get; set; }
        public Guid AuthorEmployeeId { get; set; }
        public string? AuthorName { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime NotedAt { get; set; }
    }

    public class GrievanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool IsConfidential { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedToEmployeeId { get; set; }
        public string? AssignedToName { get; set; }
        public string? Resolution { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public List<GrievanceNoteDto> Notes { get; set; } = [];
    }

    public class SubmitGrievanceDto
    {
        public string Category { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        /// <summary>Low | Medium | High | Critical.</summary>
        public string Severity { get; set; } = nameof(GrievanceSeverity.Medium);
        public bool IsConfidential { get; set; } = true;
    }

    public class ResolveGrievanceDto
    {
        public string Resolution { get; set; } = string.Empty;
    }

    public class GrievanceNoteCreateDto
    {
        public string Note { get; set; } = string.Empty;
    }

    public class SubmitGrievanceDtoValidator : AbstractValidator<SubmitGrievanceDto>
    {
        public SubmitGrievanceDtoValidator()
        {
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Details).NotEmpty().MaximumLength(4000);
            RuleFor(x => x.Severity)
                .Must(v => Enum.TryParse<GrievanceSeverity>(v, true, out _))
                .WithMessage("Severity must be Low, Medium, High or Critical.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitGrievance { Task<Guid> SubmitAsync(SubmitGrievanceDto dto); }
    public interface IAssignGrievance { Task AssignAsync(Guid id, Guid assigneeEmployeeId); }
    public interface IResolveGrievance { Task ResolveAsync(Guid id, ResolveGrievanceDto dto); }
    public interface ICloseGrievance { Task CloseAsync(Guid id); }
    public interface IAddGrievanceNote { Task AddAsync(Guid id, GrievanceNoteCreateDto dto); }
    public interface IGetGrievanceById { Task<GrievanceDto> GetAsync(Guid id); }
    public interface IGetAllGrievances { Task<PaginatedResponse<GrievanceDto>> GetAsync(GetAllRequest request); }

    internal static class GrievanceShared
    {
        /// <summary>Grievant, assigned handler or HR — managers get NO subtree view by design.</summary>
        internal static async Task<(VisibilityScope Scope, Grievance Entity)> LoadGatedAsync(
            IRepository<Grievance> repository, IPerformanceVisibilityService visibility, Guid id,
            bool includeNotes = false)
        {
            var query = repository.GetAll().AsQueryable();
            if (includeNotes) query = query.Include(x => x.Notes);
            var entity = await query.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Grievance), id.ToString());

            var scope = await visibility.GetScopeAsync();
            var allowed = scope.IsAdmin
                || entity.EmployeeId == scope.EmployeeId
                || (entity.AssignedToEmployeeId.HasValue && entity.AssignedToEmployeeId == scope.EmployeeId);
            if (!allowed)
                throw new ValidationException("id", "You do not have access to this grievance.");
            return (scope, entity);
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SubmitGrievance(
        IRepository<Grievance> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SubmitGrievanceDto> validator,
        ILogger<SubmitGrievance> logger) : ISubmitGrievance
    {
        public async Task<Guid> SubmitAsync(SubmitGrievanceDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException(nameof(dto.Subject), "Your account is not linked to an employee record.");

            var created = Grievance.Create(scope.EmployeeId.Value, dto.Category.Trim(), dto.Subject.Trim(),
                dto.Details.Trim(), Enum.Parse<GrievanceSeverity>(dto.Severity, true), dto.IsConfidential,
                DateTime.UtcNow.Date);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Grievance {Id} submitted by employee {Employee}", created.Id, scope.EmployeeId);
            return created.Id;
        }
    }

    public class AssignGrievance(
        IRepository<Grievance> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        ILogger<AssignGrievance> logger) : IAssignGrievance
    {
        public async Task AssignAsync(Guid id, Guid assigneeEmployeeId)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR administrators can assign grievances.");
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == assigneeEmployeeId))
                throw new NotFoundException(nameof(Employee), assigneeEmployeeId.ToString());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Grievance), id.ToString());
            if (entity.Status is GrievanceStatus.Resolved or GrievanceStatus.Closed)
                throw new ValidationException(nameof(id), $"The grievance is already {entity.Status}.");
            entity.Assign(assigneeEmployeeId);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Grievance {Id} assigned to {Assignee}", id, assigneeEmployeeId);
        }
    }

    public class ResolveGrievance(
        IRepository<Grievance> repository,
        IPerformanceVisibilityService visibility,
        ILogger<ResolveGrievance> logger) : IResolveGrievance
    {
        public async Task ResolveAsync(Guid id, ResolveGrievanceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Resolution))
                throw new ValidationException(nameof(dto.Resolution), "A resolution summary is required.");

            var (scope, entity) = await GrievanceShared.LoadGatedAsync(repository, visibility, id);
            if (!scope.IsAdmin && entity.AssignedToEmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only HR or the assigned handler can resolve a grievance.");
            if (entity.Status is GrievanceStatus.Resolved or GrievanceStatus.Closed)
                throw new ValidationException(nameof(id), $"The grievance is already {entity.Status}.");

            entity.Resolve(dto.Resolution.Trim(), DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Grievance {Id} resolved", id);
        }
    }

    public class CloseGrievance(
        IRepository<Grievance> repository,
        IPerformanceVisibilityService visibility,
        ILogger<CloseGrievance> logger) : ICloseGrievance
    {
        public async Task CloseAsync(Guid id)
        {
            var (scope, entity) = await GrievanceShared.LoadGatedAsync(repository, visibility, id);
            // Closure confirms the outcome — the grievant accepts it, or HR closes the file.
            if (!scope.IsAdmin && entity.EmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only the grievant or HR can close a grievance.");
            if (entity.Status != GrievanceStatus.Resolved)
                throw new ValidationException(nameof(id), $"Only a resolved grievance can be closed (current: {entity.Status}).");

            entity.Close();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Grievance {Id} closed", id);
        }
    }

    public class AddGrievanceNote(
        IRepository<Grievance> repository,
        IRepository<GrievanceNote> noteRepository,
        IPerformanceVisibilityService visibility,
        ILogger<AddGrievanceNote> logger) : IAddGrievanceNote
    {
        public async Task AddAsync(Guid id, GrievanceNoteCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Note))
                throw new ValidationException(nameof(dto.Note), "A note cannot be empty.");
            if (dto.Note.Length > 2000)
                throw new ValidationException(nameof(dto.Note), "A note is at most 2000 characters.");

            var (scope, entity) = await GrievanceShared.LoadGatedAsync(repository, visibility, id);
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException(nameof(id), "Your account is not linked to an employee record.");
            if (entity.Status == GrievanceStatus.Closed)
                throw new ValidationException(nameof(id), "The grievance is closed.");

            // Inserted through its OWN repository — routing a new child through the tracked
            // aggregate root trips the optimistic-concurrency check.
            var note = GrievanceNote.Create(entity.Id, scope.EmployeeId.Value, dto.Note.Trim());
            await noteRepository.AddAsync(note);
            await noteRepository.SaveChangesAsync();
            logger.LogInformation("Note added to grievance {Id}", id);
        }
    }

    public class GetGrievanceById(
        IRepository<Grievance> repository,
        IRepository<GrievanceNote> noteRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetGrievanceById
    {
        public async Task<GrievanceDto> GetAsync(Guid id)
        {
            var (_, entity) = await GrievanceShared.LoadGatedAsync(repository, visibility, id);
            var notes = await noteRepository.GetAll().AsNoTracking()
                .Where(n => n.GrievanceId == id).OrderBy(n => n.CreatedAt).ToListAsync();

            var ids = new List<Guid> { entity.EmployeeId };
            if (entity.AssignedToEmployeeId.HasValue) ids.Add(entity.AssignedToEmployeeId.Value);
            ids.AddRange(notes.Select(n => n.AuthorEmployeeId));
            var names = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => ids.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                }).ToDictionaryAsync(x => x.Id, x => x.Name);

            return new GrievanceDto
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeName = names.GetValueOrDefault(entity.EmployeeId),
                Category = entity.Category,
                Subject = entity.Subject,
                Details = entity.Details,
                Severity = entity.Severity.ToString(),
                IsConfidential = entity.IsConfidential,
                Status = entity.Status.ToString(),
                AssignedToEmployeeId = entity.AssignedToEmployeeId,
                AssignedToName = entity.AssignedToEmployeeId.HasValue
                    ? names.GetValueOrDefault(entity.AssignedToEmployeeId.Value) : null,
                Resolution = entity.Resolution,
                SubmittedOn = entity.SubmittedOn,
                ResolvedOn = entity.ResolvedOn,
                Notes = notes.Select(n => new GrievanceNoteDto
                {
                    Id = n.Id,
                    AuthorEmployeeId = n.AuthorEmployeeId,
                    AuthorName = names.GetValueOrDefault(n.AuthorEmployeeId),
                    Note = n.Note,
                    NotedAt = n.CreatedAt.ToDateTimeUtc()
                }).ToList()
            };
        }
    }

    public class GetAllGrievances(
        IRepository<Grievance> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllGrievances
    {
        public async Task<PaginatedResponse<GrievanceDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // HR sees all; otherwise: my own + those assigned to me. No manager subtree — by design.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                query = query.Where(x => x.EmployeeId == myEmp || x.AssignedToEmployeeId == myEmp);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<GrievanceStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Subject.Contains(term) || x.Category.Contains(term));
            }

            var employees = employeeRepository.GetAll();
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.SubmittedOn).ThenByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new GrievanceDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    Category = x.Category,
                    Subject = x.Subject,
                    Details = x.Details,
                    Severity = x.Severity.ToString(),
                    IsConfidential = x.IsConfidential,
                    Status = x.Status.ToString(),
                    AssignedToEmployeeId = x.AssignedToEmployeeId,
                    AssignedToName = employees.Where(e => e.Id == x.AssignedToEmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    Resolution = x.Resolution,
                    SubmittedOn = x.SubmittedOn,
                    ResolvedOn = x.ResolvedOn
                }).ToListAsync();

            return new PaginatedResponse<GrievanceDto> { Total = total, Data = data };
        }
    }
}
