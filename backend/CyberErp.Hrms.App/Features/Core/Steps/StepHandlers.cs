using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Steps
{
    // ---- DTOs ---------------------------------------------------------------
    public class StepDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SaveStepDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SaveStepDtoValidator : AbstractValidator<SaveStepDto>
    {
        public SaveStepDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveStep { Task<Guid> SaveAsync(SaveStepDto dto); }
    public interface IGetStepById { Task<StepDto> GetAsync(Guid id); }
    public interface IGetAllSteps { Task<PaginatedResponse<StepDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteStep { Task DeleteAsync(Guid id); }

    // ---- Save (create/update) ----------------------------------------------
    public class SaveStep(
        IRepository<Step> repository,
        IValidator<SaveStepDto> validator,
        ILogger<SaveStep> logger) : ISaveStep
    {
        public async Task<Guid> SaveAsync(SaveStepDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(Step), nameof(dto.Code), dto.Code);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Step), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = Step.Create(dto.Name, dto.Code);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Step {Id} ({Code})", created.Id, created.Code);
            return created.Id;
        }
    }

    // ---- Get by id ----------------------------------------------------------
    public class GetStepById(IRepository<Step> repository) : IGetStepById
    {
        public async Task<StepDto> GetAsync(Guid id)
        {
            var s = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Step), id.ToString());
            return new StepDto { Id = s.Id, Name = s.Name, Code = s.Code };
        }
    }

    // ---- Get all (paged) ----------------------------------------------------
    public class GetAllSteps(IRepository<Step> repository) : IGetAllSteps
    {
        public async Task<PaginatedResponse<StepDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.Code).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(x => new StepDto { Id = x.Id, Name = x.Name, Code = x.Code })
                .ToListAsync();

            return new PaginatedResponse<StepDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteStep(
        IRepository<Step> repository,
        IRepository<SalaryScale> salaryScaleRepository,
        ILogger<DeleteStep> logger) : IDeleteStep
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Step), id.ToString());

            if (await salaryScaleRepository.GetAll().AnyAsync(x => x.StepId == id))
                throw new ValidationException(nameof(id), "Cannot delete a step that is used by one or more salary scale entries.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Step {Id}", id);
        }
    }
}
