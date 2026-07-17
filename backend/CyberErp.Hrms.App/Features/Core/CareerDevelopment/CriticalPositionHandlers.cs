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
    public class CriticalPositionDto
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionTitle { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string RiskLevel { get; set; } = nameof(CriticalityLevel.Medium);
        public string? Reason { get; set; }
        public string? Criteria { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveCriticalPositionDto
    {
        public Guid? Id { get; set; }
        public Guid PositionId { get; set; }
        public string RiskLevel { get; set; } = nameof(CriticalityLevel.Medium);
        public string? Reason { get; set; }
        public string? Criteria { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveCriticalPositionDtoValidator : AbstractValidator<SaveCriticalPositionDto>
    {
        public SaveCriticalPositionDtoValidator()
        {
            RuleFor(x => x.PositionId).NotEmpty();
            RuleFor(x => x.RiskLevel).NotEmpty().Must(v => Enum.TryParse<CriticalityLevel>(v, out _))
                .WithMessage("Invalid risk level.");
            RuleFor(x => x.Reason).MaximumLength(1000);
            RuleFor(x => x.Criteria).MaximumLength(2000);
        }
    }

    internal static class CriticalPositionMapper
    {
        internal static readonly Expression<Func<CriticalPosition, CriticalPositionDto>> Projection = c => new CriticalPositionDto
        {
            Id = c.Id,
            PositionId = c.PositionId,
            PositionCode = c.Position != null ? c.Position.Code : null,
            PositionTitle = c.Position != null && c.Position.PositionClass != null ? c.Position.PositionClass.Title : null,
            OrganizationUnitName = c.Position != null && c.Position.OrganizationUnit != null ? c.Position.OrganizationUnit.Name : null,
            RiskLevel = c.RiskLevel.ToString(),
            Reason = c.Reason,
            Criteria = c.Criteria,
            IsActive = c.IsActive
        };
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveCriticalPosition { Task<Guid> SaveAsync(SaveCriticalPositionDto dto); }
    public interface IDeleteCriticalPosition { Task DeleteAsync(Guid id); }
    public interface IGetCriticalPositionById { Task<CriticalPositionDto> GetAsync(Guid id); }
    public interface IGetAllCriticalPositions { Task<PaginatedResponse<CriticalPositionDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveCriticalPosition(
        IRepository<CriticalPosition> repository,
        IRepository<Position> positionRepository,
        IValidator<SaveCriticalPositionDto> validator,
        ILogger<SaveCriticalPosition> logger) : ISaveCriticalPosition
    {
        public async Task<Guid> SaveAsync(SaveCriticalPositionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await positionRepository.GetAll().AnyAsync(p => p.Id == dto.PositionId))
                throw new NotFoundException(nameof(Position), dto.PositionId.ToString());

            // A position can only be flagged critical once.
            if (await repository.GetAll().AnyAsync(x => x.PositionId == dto.PositionId && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(CriticalPosition), nameof(dto.PositionId), dto.PositionId.ToString());

            var risk = Enum.Parse<CriticalityLevel>(dto.RiskLevel);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(CriticalPosition), dto.Id.Value.ToString());
                entity.Update(dto.PositionId, risk, dto.Reason, dto.Criteria, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated CriticalPosition {Id}", entity.Id);
                return entity.Id;
            }

            var created = CriticalPosition.Create(dto.PositionId, risk, dto.Reason, dto.Criteria, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CriticalPosition {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteCriticalPosition(
        IRepository<CriticalPosition> repository,
        IRepository<SuccessionPlan> planRepository,
        ILogger<DeleteCriticalPosition> logger) : IDeleteCriticalPosition
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CriticalPosition), id.ToString());
            if (await planRepository.GetAll().AnyAsync(p => p.CriticalPositionId == id))
                throw new ValidationException(nameof(id), "Cannot delete a critical position that has succession plans.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CriticalPosition {Id}", id);
        }
    }

    public class GetCriticalPositionById(IRepository<CriticalPosition> repository) : IGetCriticalPositionById
    {
        public async Task<CriticalPositionDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(CriticalPositionMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(CriticalPosition), id.ToString());
    }

    public class GetAllCriticalPositions(IRepository<CriticalPosition> repository) : IGetAllCriticalPositions
    {
        public async Task<PaginatedResponse<CriticalPositionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Position != null && (x.Position.Code.Contains(term)
                    || (x.Position.PositionClass != null && x.Position.PositionClass.Title.Contains(term))));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RiskLevel)
                .Skip(skip).Take(take).Select(CriticalPositionMapper.Projection).ToListAsync();

            return new PaginatedResponse<CriticalPositionDto> { Total = total, Data = data };
        }
    }
}
