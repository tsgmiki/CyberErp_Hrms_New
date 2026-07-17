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
    public class CareerPathStepCompetencyDto
    {
        public Guid Id { get; set; }
        public Guid CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public decimal Weight { get; set; }
    }

    public class CareerPathStepDto
    {
        public Guid Id { get; set; }
        public Guid CareerPathId { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? PositionClassId { get; set; }
        public string? PositionClassName { get; set; }
        public Guid? JobGradeId { get; set; }
        public int? RequiredExperienceMonths { get; set; }
        public string? Certifications { get; set; }
        public string? Description { get; set; }
        public List<CareerPathStepCompetencyDto> Competencies { get; set; } = [];
    }

    public class SaveCareerPathStepCompetencyDto
    {
        public Guid CompetencyId { get; set; }
        public decimal Weight { get; set; } = 1m;
    }

    public class SaveCareerPathStepDto
    {
        public Guid? Id { get; set; }
        public Guid CareerPathId { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? PositionClassId { get; set; }
        public Guid? JobGradeId { get; set; }
        public int? RequiredExperienceMonths { get; set; }
        public string? Certifications { get; set; }
        public string? Description { get; set; }
        public List<SaveCareerPathStepCompetencyDto> Competencies { get; set; } = [];
    }

    public class SaveCareerPathStepDtoValidator : AbstractValidator<SaveCareerPathStepDto>
    {
        public SaveCareerPathStepDtoValidator()
        {
            RuleFor(x => x.CareerPathId).NotEmpty();
            RuleFor(x => x.StepOrder).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Certifications).MaximumLength(1000);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.RequiredExperienceMonths).GreaterThanOrEqualTo(0).When(x => x.RequiredExperienceMonths.HasValue);
            RuleForEach(x => x.Competencies).ChildRules(c =>
            {
                c.RuleFor(x => x.CompetencyId).NotEmpty();
                c.RuleFor(x => x.Weight).GreaterThanOrEqualTo(0);
            });
        }
    }

    internal static class CareerPathStepMapper
    {
        internal static readonly Expression<Func<CareerPathStep, CareerPathStepDto>> Projection = s => new CareerPathStepDto
        {
            Id = s.Id,
            CareerPathId = s.CareerPathId,
            StepOrder = s.StepOrder,
            Name = s.Name,
            PositionClassId = s.PositionClassId,
            PositionClassName = s.PositionClass != null ? s.PositionClass.Title : null,
            JobGradeId = s.JobGradeId,
            RequiredExperienceMonths = s.RequiredExperienceMonths,
            Certifications = s.Certifications,
            Description = s.Description,
            Competencies = s.Competencies.Select(c => new CareerPathStepCompetencyDto
            {
                Id = c.Id, CompetencyId = c.CompetencyId, Weight = c.Weight
            }).ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted competencies copy it here.</summary>
        internal static void StampCompetencyTenant(CareerPathStep s)
        {
            foreach (var c in s.Competencies)
                if (string.IsNullOrEmpty(c.TenantId)) c.TenantId = s.TenantId;
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveCareerPathStep { Task<Guid> SaveAsync(SaveCareerPathStepDto dto); }
    public interface IDeleteCareerPathStep { Task DeleteAsync(Guid id); }
    public interface IGetCareerPathStepById { Task<CareerPathStepDto> GetAsync(Guid id); }
    public interface IGetAllCareerPathSteps { Task<PaginatedResponse<CareerPathStepDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveCareerPathStep(
        IRepository<CareerPathStep> repository,
        IRepository<CareerPathStepCompetency> competencyRepository,
        IValidator<SaveCareerPathStepDto> validator,
        ILogger<SaveCareerPathStep> logger) : ISaveCareerPathStep
    {
        public async Task<Guid> SaveAsync(SaveCareerPathStepDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Step order is unique within a path.
            if (await repository.GetAll().AnyAsync(x => x.CareerPathId == dto.CareerPathId
                    && x.StepOrder == dto.StepOrder && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(CareerPathStep), nameof(dto.StepOrder), dto.StepOrder.ToString());

            var specs = dto.Competencies.Select(c => new CareerPathStepCompetencySpec(c.CompetencyId, c.Weight)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Competencies)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(CareerPathStep), dto.Id.Value.ToString());

                foreach (var old in entity.Competencies.ToList()) competencyRepository.Delete(old);
                entity.Update(dto.StepOrder, dto.Name, dto.PositionClassId, dto.JobGradeId,
                    dto.RequiredExperienceMonths, dto.Certifications, dto.Description);
                entity.SetCompetencies(specs);
                CareerPathStepMapper.StampCompetencyTenant(entity);
                foreach (var c in entity.Competencies) await competencyRepository.AddAsync(c);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = CareerPathStep.Create(dto.CareerPathId, dto.StepOrder, dto.Name, dto.PositionClassId,
                dto.JobGradeId, dto.RequiredExperienceMonths, dto.Certifications, dto.Description);
            created.SetCompetencies(specs);
            await repository.AddAsync(created);
            CareerPathStepMapper.StampCompetencyTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CareerPathStep {Id} for path {PathId}", created.Id, dto.CareerPathId);
            return created.Id;
        }
    }

    public class DeleteCareerPathStep(IRepository<CareerPathStep> repository) : IDeleteCareerPathStep
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CareerPathStep), id.ToString());
            repository.Delete(entity); // cascade removes competencies
            await repository.SaveChangesAsync();
        }
    }

    public class GetCareerPathStepById(IRepository<CareerPathStep> repository) : IGetCareerPathStepById
    {
        public async Task<CareerPathStepDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(CareerPathStepMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(CareerPathStep), id.ToString());
    }

    public class GetAllCareerPathSteps(IRepository<CareerPathStep> repository) : IGetAllCareerPathSteps
    {
        public async Task<PaginatedResponse<CareerPathStepDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            // Scoped to a career path (parentId) — the standard drilldown.
            if (request.ParentId.HasValue)
                query = query.Where(x => x.CareerPathId == request.ParentId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term));
            }

            var total = await query.CountAsync();
            // List is lightweight — competency rows load only on GetById.
            var data = await query.OrderBy(x => x.StepOrder).Skip(skip).Take(take)
                .Select(s => new CareerPathStepDto
                {
                    Id = s.Id, CareerPathId = s.CareerPathId, StepOrder = s.StepOrder, Name = s.Name,
                    PositionClassId = s.PositionClassId,
                    PositionClassName = s.PositionClass != null ? s.PositionClass.Title : null,
                    JobGradeId = s.JobGradeId, RequiredExperienceMonths = s.RequiredExperienceMonths,
                    Certifications = s.Certifications, Description = s.Description
                }).ToListAsync();

            return new PaginatedResponse<CareerPathStepDto> { Total = total, Data = data };
        }
    }
}
