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
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public DateTime PublishFrom { get; set; }
        public DateTime? PublishUntil { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveAnnouncementDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        /// <summary>All | Branch | Unit (HC206 targeting).</summary>
        public string Audience { get; set; } = nameof(AnnouncementAudience.All);
        public Guid? BranchId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public DateTime PublishFrom { get; set; }
        public DateTime? PublishUntil { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveAnnouncementDtoValidator : AbstractValidator<SaveAnnouncementDto>
    {
        public SaveAnnouncementDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
            RuleFor(x => x.Audience)
                .Must(v => Enum.TryParse<AnnouncementAudience>(v, true, out _))
                .WithMessage("Audience must be All, Branch or Unit.");
            RuleFor(x => x.PublishFrom).NotEmpty();
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveAnnouncement { Task<Guid> SaveAsync(SaveAnnouncementDto dto); }
    public interface IDeleteAnnouncement { Task DeleteAsync(Guid id); }
    public interface IGetAllAnnouncements { Task<PaginatedResponse<AnnouncementDto>> GetAsync(GetAllRequest request); }
    /// <summary>HC206 — the employee's targeted feed: published, in-window, aimed at them.</summary>
    public interface IGetAnnouncementFeed { Task<PaginatedResponse<AnnouncementDto>> GetAsync(GetAllRequest request); }

    internal static class AnnouncementQueryShared
    {
        internal static IQueryable<AnnouncementDto> Project(
            IQueryable<Announcement> query,
            IQueryable<Branch> branches,
            IQueryable<OrganizationUnit> units)
        {
            return query.Select(x => new AnnouncementDto
            {
                Id = x.Id,
                Title = x.Title,
                Body = x.Body,
                Audience = x.Audience.ToString(),
                BranchId = x.BranchId,
                BranchName = branches.Where(b => b.Id == x.BranchId).Select(b => b.Name).FirstOrDefault(),
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = units.Where(u => u.Id == x.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                PublishFrom = x.PublishFrom,
                PublishUntil = x.PublishUntil,
                IsPinned = x.IsPinned,
                IsActive = x.IsActive
            });
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveAnnouncement(
        IRepository<Announcement> repository,
        IRepository<Branch> branchRepository,
        IRepository<OrganizationUnit> unitRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveAnnouncementDto> validator,
        ILogger<SaveAnnouncement> logger) : ISaveAnnouncement
    {
        public async Task<Guid> SaveAsync(SaveAnnouncementDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(dto.Title), "Only HR administrators can publish announcements.");

            var audience = Enum.Parse<AnnouncementAudience>(dto.Audience, true);
            if (audience == AnnouncementAudience.Branch)
            {
                if (!dto.BranchId.HasValue || !await branchRepository.GetAll().AnyAsync(b => b.Id == dto.BranchId.Value))
                    throw new ValidationException(nameof(dto.BranchId), "A valid target branch is required.");
            }
            if (audience == AnnouncementAudience.Unit)
            {
                if (!dto.OrganizationUnitId.HasValue || !await unitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                    throw new ValidationException(nameof(dto.OrganizationUnitId), "A valid target unit is required.");
            }

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Announcement), dto.Id.Value.ToString());
                entity.Update(dto.Title, dto.Body, audience, dto.BranchId, dto.OrganizationUnitId,
                    dto.PublishFrom, dto.PublishUntil, dto.IsPinned, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated announcement {Id}", entity.Id);
                return entity.Id;
            }

            var created = Announcement.Create(dto.Title, dto.Body, audience, dto.BranchId, dto.OrganizationUnitId,
                dto.PublishFrom, dto.PublishUntil, dto.IsPinned, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created announcement {Id} ({Audience})", created.Id, audience);
            return created.Id;
        }
    }

    public class DeleteAnnouncement(
        IRepository<Announcement> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteAnnouncement> logger) : IDeleteAnnouncement
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR administrators can delete announcements.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Announcement), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted announcement {Id}", id);
        }
    }

    public class GetAllAnnouncements(
        IRepository<Announcement> repository,
        IRepository<Branch> branchRepository,
        IRepository<OrganizationUnit> unitRepository,
        IPerformanceVisibilityService visibility) : IGetAllAnnouncements
    {
        public async Task<PaginatedResponse<AnnouncementDto>> GetAsync(GetAllRequest request)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators manage announcements — employees read the feed.");

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await AnnouncementQueryShared.Project(
                    query.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.PublishFrom).Skip(skip).Take(take),
                    branchRepository.GetAll(), unitRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<AnnouncementDto> { Total = total, Data = data };
        }
    }

    public class GetAnnouncementFeed(
        IRepository<Announcement> repository,
        IRepository<Branch> branchRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAnnouncementFeed
    {
        public async Task<PaginatedResponse<AnnouncementDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var now = DateTime.UtcNow.Date;
            var query = repository.GetAll().AsNoTracking()
                .Where(x => x.IsActive && x.PublishFrom <= now && (x.PublishUntil == null || x.PublishUntil >= now));

            // Targeting (HC206): everyone gets All; branch/unit rows only reach their audience.
            // A unit target covers its whole SUBTREE — i.e. it matches any ancestor of MY unit.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                Guid? myBranchId = null;
                var ancestorIds = new List<Guid>();
                if (scope.EmployeeId.HasValue)
                {
                    var placement = await employeeRepository.GetAll().AsNoTracking()
                        .Where(e => e.Id == scope.EmployeeId.Value && e.Position != null)
                        .Select(e => new { UnitId = (Guid?)e.Position!.OrganizationUnitId, e.Position.BranchId })
                        .FirstOrDefaultAsync();
                    myBranchId = placement?.BranchId;

                    if (placement?.UnitId is Guid myUnit)
                    {
                        // Units are few — one projection, then an in-memory parent walk.
                        var parents = await unitRepository.GetAll().AsNoTracking()
                            .Select(u => new { u.Id, u.ParentId }).ToDictionaryAsync(u => u.Id, u => u.ParentId);
                        var cursor = (Guid?)myUnit;
                        var hops = 0;
                        while (cursor.HasValue && hops++ < 50)
                        {
                            ancestorIds.Add(cursor.Value);
                            cursor = parents.GetValueOrDefault(cursor.Value);
                        }
                    }
                }

                query = query.Where(x => x.Audience == AnnouncementAudience.All
                    || (x.Audience == AnnouncementAudience.Branch && x.BranchId == myBranchId)
                    || (x.Audience == AnnouncementAudience.Unit && x.OrganizationUnitId != null
                        && ancestorIds.Contains(x.OrganizationUnitId.Value)));
            }

            var total = await query.CountAsync();
            var data = await AnnouncementQueryShared.Project(
                    query.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.PublishFrom).Skip(skip).Take(take),
                    branchRepository.GetAll(), unitRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<AnnouncementDto> { Total = total, Data = data };
        }
    }
}
