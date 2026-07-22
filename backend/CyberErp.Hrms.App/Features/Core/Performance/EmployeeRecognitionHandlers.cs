using CyberErp.Hrms.App.Common.DTOs;
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
    public class EmployeeRecognitionDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid RecognitionBadgeId { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeColor { get; set; }
        public string? BadgeIcon { get; set; }
        public string Citation { get; set; } = string.Empty;
        public DateTime RecognizedOn { get; set; }
        public bool IsPublic { get; set; }
    }

    public class SaveEmployeeRecognitionDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid RecognitionBadgeId { get; set; }
        public string Citation { get; set; } = string.Empty;
        public DateTime RecognizedOn { get; set; }
        public bool IsPublic { get; set; } = true;
    }

    public class SaveEmployeeRecognitionDtoValidator : AbstractValidator<SaveEmployeeRecognitionDto>
    {
        public SaveEmployeeRecognitionDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.RecognitionBadgeId).NotEmpty();
            RuleFor(x => x.Citation).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.RecognizedOn).NotEmpty();
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveEmployeeRecognition { Task<Guid> SaveAsync(SaveEmployeeRecognitionDto dto); }
    public interface IDeleteEmployeeRecognition { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeRecognitionById { Task<EmployeeRecognitionDto> GetAsync(Guid id); }
    public interface IGetAllEmployeeRecognitions { Task<PaginatedResponse<EmployeeRecognitionDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveEmployeeRecognition(
        IRepository<EmployeeRecognition> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IRepository<RewardDisbursement> disbursementRepository,
        IPerformanceHistoryWriter history,
        IValidator<SaveEmployeeRecognitionDto> validator,
        ILogger<SaveEmployeeRecognition> logger) : ISaveEmployeeRecognition
    {
        public async Task<Guid> SaveAsync(SaveEmployeeRecognitionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            var badge = await badgeRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == dto.RecognitionBadgeId)
                ?? throw new NotFoundException(nameof(RecognitionBadge), dto.RecognitionBadgeId.ToString());

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(EmployeeRecognition), dto.Id.Value.ToString());
                entity.Update(dto.EmployeeId, dto.RecognitionBadgeId, dto.Citation, dto.RecognizedOn, dto.IsPublic);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeRecognition {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeRecognition.Create(dto.EmployeeId, dto.RecognitionBadgeId, dto.Citation, dto.RecognizedOn, dto.IsPublic);
            await repository.AddAsync(created);
            // HC180/HC185 — a direct grant carries the badge's reward value like any approved nomination.
            await Rewards.RewardGrantShared.ApplyGrantSideEffectsAsync(created, badge, pointsRepository, disbursementRepository);
            await history.WriteAsync("Recognition", created.Id, "Recognized", $"Recognition granted: {dto.Citation}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeRecognition {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteEmployeeRecognition(
        IRepository<EmployeeRecognition> repository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IRepository<RewardDisbursement> disbursementRepository,
        ILogger<DeleteEmployeeRecognition> logger) : IDeleteEmployeeRecognition
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeRecognition), id.ToString());

            // Keep the ledger consistent: reverse points the grant earned and cancel its unpaid payment.
            var earned = await pointsRepository.GetAll()
                .Where(x => x.ReferenceId == id && x.Source == RewardPointsSource.Recognition)
                .SumAsync(x => (int?)x.Points) ?? 0;
            if (earned > 0)
            {
                var reversal = RewardPointsTransaction.Create(entity.EmployeeId, -earned, RewardPointsSource.Adjustment,
                    DateTime.UtcNow.Date, id, "Reversal — recognition deleted");
                if (string.IsNullOrEmpty(reversal.TenantId)) reversal.TenantId = entity.TenantId;
                await pointsRepository.AddAsync(reversal);
            }
            var pendingPayments = await disbursementRepository.GetAll()
                .Where(x => x.EmployeeRecognitionId == id && x.Status == DisbursementStatus.Pending)
                .ToListAsync();
            foreach (var payment in pendingPayments)
            {
                payment.Cancel();
                disbursementRepository.UpdateAsync(payment);
            }

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeRecognition {Id}", id);
        }
    }

    internal static class EmployeeRecognitionMapper
    {
        internal static EmployeeRecognitionDto Map(EmployeeRecognition x, string? employeeName,
            RecognitionBadge? badge) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            RecognitionBadgeId = x.RecognitionBadgeId,
            BadgeName = badge?.Name,
            BadgeColor = badge?.Color,
            BadgeIcon = badge?.Icon,
            Citation = x.Citation,
            RecognizedOn = x.RecognizedOn,
            IsPublic = x.IsPublic
        };
    }

    public class GetEmployeeRecognitionById(
        IRepository<EmployeeRecognition> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository) : IGetEmployeeRecognitionById
    {
        public async Task<EmployeeRecognitionDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeRecognition), id.ToString());
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            var badge = await badgeRepository.GetAll().FirstOrDefaultAsync(b => b.Id == entity.RecognitionBadgeId);
            return EmployeeRecognitionMapper.Map(entity, employeeName, badge);
        }
    }

    public class GetAllEmployeeRecognitions(
        IRepository<EmployeeRecognition> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository) : IGetAllEmployeeRecognitions
    {
        public async Task<PaginatedResponse<EmployeeRecognitionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (request.IsPublic.HasValue)
                query = query.Where(x => x.IsPublic == request.IsPublic.Value);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.RecognizedOn).Skip(skip).Take(take).ToListAsync();

            var badgeIds = rows.Select(r => r.RecognitionBadgeId).Distinct().ToList();
            var badges = await badgeRepository.GetAll().Where(b => badgeIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id);

            // PERFORMANCE: batch-load the employee names for the page in ONE query (was one per row).
            var empIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<EmployeeRecognitionDto>(rows.Count);
            foreach (var r in rows)
            {
                data.Add(EmployeeRecognitionMapper.Map(r, employeeNames.GetValueOrDefault(r.EmployeeId), badges.GetValueOrDefault(r.RecognitionBadgeId)));
            }
            return new PaginatedResponse<EmployeeRecognitionDto> { Total = total, Data = data };
        }
    }
}
