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
    public class SuccessionPlanDto
    {
        public Guid Id { get; set; }
        public Guid CriticalPositionId { get; set; }
        public string? RoleTitle { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Horizon { get; set; } = nameof(SuccessionHorizon.MediumTerm);
        public string Status { get; set; } = nameof(SuccessionPlanStatus.Active);
        public string? Notes { get; set; }
    }

    public class SaveSuccessionPlanDto
    {
        public Guid? Id { get; set; }
        public Guid CriticalPositionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Horizon { get; set; } = nameof(SuccessionHorizon.MediumTerm);
        public string Status { get; set; } = nameof(SuccessionPlanStatus.Active);
        public string? Notes { get; set; }
    }

    public class SaveSuccessionPlanDtoValidator : AbstractValidator<SaveSuccessionPlanDto>
    {
        public SaveSuccessionPlanDtoValidator()
        {
            RuleFor(x => x.CriticalPositionId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Horizon).NotEmpty().Must(v => Enum.TryParse<SuccessionHorizon>(v, out _)).WithMessage("Invalid horizon.");
            RuleFor(x => x.Status).NotEmpty().Must(v => Enum.TryParse<SuccessionPlanStatus>(v, out _)).WithMessage("Invalid status.");
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    /// <summary>Lightweight successor-chart node (HC159) — no nested entities.</summary>
    public class SuccessionChartNodeDto
    {
        public Guid CandidateId { get; set; }
        public int Rank { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
    }

    public class SuccessionChartDto
    {
        public Guid SuccessionPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? RoleTitle { get; set; }
        public List<SuccessionChartNodeDto> Successors { get; set; } = [];
    }

    internal static class SuccessionPlanMapper
    {
        internal static readonly Expression<Func<SuccessionPlan, SuccessionPlanDto>> Projection = p => new SuccessionPlanDto
        {
            Id = p.Id,
            CriticalPositionId = p.CriticalPositionId,
            RoleTitle = p.CriticalPosition != null && p.CriticalPosition.Position != null && p.CriticalPosition.Position.PositionClass != null
                ? p.CriticalPosition.Position.PositionClass.Title : null,
            Name = p.Name,
            Horizon = p.Horizon.ToString(),
            Status = p.Status.ToString(),
            Notes = p.Notes
        };
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveSuccessionPlan { Task<Guid> SaveAsync(SaveSuccessionPlanDto dto); }
    public interface IDeleteSuccessionPlan { Task DeleteAsync(Guid id); }
    public interface IGetSuccessionPlanById { Task<SuccessionPlanDto> GetAsync(Guid id); }
    public interface IGetAllSuccessionPlans { Task<PaginatedResponse<SuccessionPlanDto>> GetAsync(GetAllRequest request); }
    public interface IGetSuccessionChart { Task<SuccessionChartDto> GetAsync(Guid successionPlanId); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveSuccessionPlan(
        IRepository<SuccessionPlan> repository,
        IRepository<CriticalPosition> criticalPositionRepository,
        IValidator<SaveSuccessionPlanDto> validator,
        ILogger<SaveSuccessionPlan> logger) : ISaveSuccessionPlan
    {
        public async Task<Guid> SaveAsync(SaveSuccessionPlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await criticalPositionRepository.GetAll().AnyAsync(c => c.Id == dto.CriticalPositionId))
                throw new NotFoundException(nameof(CriticalPosition), dto.CriticalPositionId.ToString());

            var horizon = Enum.Parse<SuccessionHorizon>(dto.Horizon);
            var status = Enum.Parse<SuccessionPlanStatus>(dto.Status);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(SuccessionPlan), dto.Id.Value.ToString());
                entity.Update(dto.CriticalPositionId, dto.Name, horizon, status, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = SuccessionPlan.Create(dto.CriticalPositionId, dto.Name, horizon, status, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created SuccessionPlan {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteSuccessionPlan(IRepository<SuccessionPlan> repository) : IDeleteSuccessionPlan
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(SuccessionPlan), id.ToString());
            repository.Delete(entity); // cascade removes candidates + their dev-actions/knowledge-transfer
            await repository.SaveChangesAsync();
        }
    }

    public class GetSuccessionPlanById(IRepository<SuccessionPlan> repository) : IGetSuccessionPlanById
    {
        public async Task<SuccessionPlanDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(SuccessionPlanMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(SuccessionPlan), id.ToString());
    }

    public class GetAllSuccessionPlans(IRepository<SuccessionPlan> repository) : IGetAllSuccessionPlans
    {
        public async Task<PaginatedResponse<SuccessionPlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (request.ParentId.HasValue) // scoped to one critical position
                query = query.Where(x => x.CriticalPositionId == request.ParentId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<SuccessionPlanStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name)
                .Skip(skip).Take(take).Select(SuccessionPlanMapper.Projection).ToListAsync();

            return new PaginatedResponse<SuccessionPlanDto> { Total = total, Data = data };
        }
    }

    /// <summary>Succession chart (HC159) — a lightweight ordered successor list, no nested entities loaded.</summary>
    public class GetSuccessionChart(
        IRepository<SuccessionPlan> planRepository,
        IRepository<SuccessionCandidate> candidateRepository) : IGetSuccessionChart
    {
        public async Task<SuccessionChartDto> GetAsync(Guid successionPlanId)
        {
            var plan = await planRepository.GetAll().Where(p => p.Id == successionPlanId)
                .Select(SuccessionPlanMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(SuccessionPlan), successionPlanId.ToString());

            var successors = await candidateRepository.GetAll()
                .Where(c => c.SuccessionPlanId == successionPlanId)
                .OrderBy(c => c.Rank)
                .Select(c => new SuccessionChartNodeDto
                {
                    CandidateId = c.Id, Rank = c.Rank, EmployeeId = c.EmployeeId,
                    EmployeeName = c.Employee != null && c.Employee.Person != null
                        ? (c.Employee.Person.FirstName + " " + c.Employee.Person.GrandFatherName) : null,
                    Readiness = c.Readiness.ToString(), ReadinessScore = c.ReadinessScore
                }).ToListAsync();

            return new SuccessionChartDto { SuccessionPlanId = successionPlanId, Name = plan.Name, RoleTitle = plan.RoleTitle, Successors = successors };
        }
    }
}
