using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.SalaryScales
{
    // ---- DTOs ---------------------------------------------------------------
    public class SalaryScaleDto
    {
        public Guid Id { get; set; }
        public Guid JobGradeId { get; set; }
        public string? JobGrade { get; set; }
        public Guid StepId { get; set; }
        public string? Step { get; set; }
        public decimal Salary { get; set; }
    }

    public class SaveSalaryScaleDto
    {
        public Guid? Id { get; set; }
        public Guid JobGradeId { get; set; }
        public Guid StepId { get; set; }
        public decimal Salary { get; set; }
    }

    public class SaveSalaryScaleDtoValidator : AbstractValidator<SaveSalaryScaleDto>
    {
        public SaveSalaryScaleDtoValidator()
        {
            RuleFor(x => x.JobGradeId).NotEmpty().WithMessage("Job grade is required.");
            RuleFor(x => x.StepId).NotEmpty().WithMessage("Step is required.");
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveSalaryScale { Task<Guid> SaveAsync(SaveSalaryScaleDto dto); }
    public interface IGetSalaryScaleById { Task<SalaryScaleDto> GetAsync(Guid id); }
    public interface IGetAllSalaryScales { Task<PaginatedResponse<SalaryScaleDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteSalaryScale { Task DeleteAsync(Guid id); }

    // ---- Save (create/update) ----------------------------------------------
    public class SaveSalaryScale(
        IRepository<SalaryScale> repository,
        IRepository<JobGrade> jobGradeRepository,
        IRepository<Step> stepRepository,
        IValidator<SaveSalaryScaleDto> validator,
        ILogger<SaveSalaryScale> logger) : ISaveSalaryScale
    {
        public async Task<Guid> SaveAsync(SaveSalaryScaleDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await jobGradeRepository.GetAll().AnyAsync(x => x.Id == dto.JobGradeId))
                throw new NotFoundException(nameof(JobGrade), dto.JobGradeId.ToString());
            if (!await stepRepository.GetAll().AnyAsync(x => x.Id == dto.StepId))
                throw new NotFoundException(nameof(Step), dto.StepId.ToString());

            // A grade/step pair may carry only one salary.
            if (await repository.GetAll().AnyAsync(x => x.JobGradeId == dto.JobGradeId && x.StepId == dto.StepId && x.Id != dto.Id))
                throw new ValidationException("stepId", "This step already has a salary defined for the selected job grade.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(SalaryScale), dto.Id.Value.ToString());
                entity.Update(dto.JobGradeId, dto.StepId, dto.Salary);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = SalaryScale.Create(dto.JobGradeId, dto.StepId, dto.Salary);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created SalaryScale {Id} (grade {GradeId}, step {StepId})", created.Id, dto.JobGradeId, dto.StepId);
            return created.Id;
        }
    }

    // ---- Get by id ----------------------------------------------------------
    public class GetSalaryScaleById(IRepository<SalaryScale> repository) : IGetSalaryScaleById
    {
        public async Task<SalaryScaleDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll()
                .Where(x => x.Id == id)
                .Select(x => new SalaryScaleDto
                {
                    Id = x.Id,
                    JobGradeId = x.JobGradeId,
                    JobGrade = x.JobGrade.Name,
                    StepId = x.StepId,
                    Step = x.Step.Name,
                    Salary = x.Salary
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(SalaryScale), id.ToString());
            return dto;
        }
    }

    // ---- Get all (paged, filtered by job grade) -----------------------------
    public class GetAllSalaryScales(IRepository<SalaryScale> repository) : IGetAllSalaryScales
    {
        public async Task<PaginatedResponse<SalaryScaleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // The grid is always scoped to a single job grade; without one, return nothing.
            if (!request.JobGradeId.HasValue || request.JobGradeId.Value == Guid.Empty)
                return new PaginatedResponse<SalaryScaleDto> { Total = 0, Data = [] };

            query = query.Where(x => x.JobGradeId == request.JobGradeId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Step.Name.Contains(term) || x.Step.Code.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.Step.Code).ThenBy(x => x.Step.Name)
                .Skip(skip).Take(take)
                .Select(x => new SalaryScaleDto
                {
                    Id = x.Id,
                    JobGradeId = x.JobGradeId,
                    JobGrade = x.JobGrade.Name,
                    StepId = x.StepId,
                    Step = x.Step.Name,
                    Salary = x.Salary
                })
                .ToListAsync();

            return new PaginatedResponse<SalaryScaleDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteSalaryScale(
        IRepository<SalaryScale> repository,
        IRepository<PositionClass> positionClassRepository,
        ILogger<DeleteSalaryScale> logger) : IDeleteSalaryScale
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(SalaryScale), id.ToString());

            if (await positionClassRepository.GetAll().AnyAsync(p => p.SalaryScaleId == id))
                throw new ValidationException(nameof(id), "Cannot delete a salary scale that is used by one or more position classes.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted SalaryScale {Id}", id);
        }
    }
}
