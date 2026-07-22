using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class RecognitionWallItemDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeColor { get; set; }
        public string? BadgeIcon { get; set; }
        public string? RewardKind { get; set; }
        public string Citation { get; set; } = string.Empty;
        public DateTime RecognizedOn { get; set; }
    }

    // ---- Interface ----------------------------------------------------------
    /// <summary>HC184 — the company-wide public recognition feed, visible to every employee.</summary>
    public interface IGetRecognitionWall { Task<PaginatedResponse<RecognitionWallItemDto>> GetAsync(GetAllRequest request); }

    // ---- Handler ------------------------------------------------------------
    public class GetRecognitionWall(
        IRepository<EmployeeRecognition> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository) : IGetRecognitionWall
    {
        public async Task<PaginatedResponse<RecognitionWallItemDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            // Public grants only — a single projected query over the (TenantId, IsPublic, RecognizedOn) index.
            var query = repository.GetAll().AsNoTracking().Where(x => x.IsPublic);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);

            var employees = employeeRepository.GetAll();
            var badges = badgeRepository.GetAll();

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RecognizedOn).ThenByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new RecognitionWallItemDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    BadgeName = badges.Where(b => b.Id == x.RecognitionBadgeId).Select(b => b.Name).FirstOrDefault(),
                    BadgeColor = badges.Where(b => b.Id == x.RecognitionBadgeId).Select(b => b.Color).FirstOrDefault(),
                    BadgeIcon = badges.Where(b => b.Id == x.RecognitionBadgeId).Select(b => b.Icon).FirstOrDefault(),
                    RewardKind = badges.Where(b => b.Id == x.RecognitionBadgeId)
                        .Select(b => b.RewardKind.ToString()).FirstOrDefault(),
                    Citation = x.Citation,
                    RecognizedOn = x.RecognizedOn
                }).ToListAsync();

            return new PaginatedResponse<RecognitionWallItemDto> { Total = total, Data = data };
        }
    }
}
