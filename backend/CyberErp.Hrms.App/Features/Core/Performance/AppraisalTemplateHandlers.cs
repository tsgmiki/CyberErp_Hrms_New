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
    public class AppraisalTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal GoalsWeight { get; set; }
        public decimal CompetenciesWeight { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAppraisalTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal GoalsWeight { get; set; }
        public decimal CompetenciesWeight { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAppraisalTemplateDto : CreateAppraisalTemplateDto
    {
        public Guid Id { get; set; }
    }

    public class CreateAppraisalTemplateDtoValidator : AbstractValidator<CreateAppraisalTemplateDto>
    {
        public CreateAppraisalTemplateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.GoalsWeight).InclusiveBetween(0, 100);
            RuleFor(x => x.CompetenciesWeight).InclusiveBetween(0, 100);
            RuleFor(x => x).Must(x => x.GoalsWeight + x.CompetenciesWeight == 100)
                .WithMessage("Goals and competencies weights must add up to 100%.")
                .OverridePropertyName("goalsWeight");
        }
    }

    public class UpdateAppraisalTemplateDtoValidator : AbstractValidator<UpdateAppraisalTemplateDto>
    {
        public UpdateAppraisalTemplateDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.GoalsWeight).InclusiveBetween(0, 100);
            RuleFor(x => x.CompetenciesWeight).InclusiveBetween(0, 100);
            RuleFor(x => x).Must(x => x.GoalsWeight + x.CompetenciesWeight == 100)
                .WithMessage("Goals and competencies weights must add up to 100%.")
                .OverridePropertyName("goalsWeight");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateAppraisalTemplate { Task<Guid> CreateAsync(CreateAppraisalTemplateDto dto); }
    public interface IUpdateAppraisalTemplate { Task UpdateAsync(UpdateAppraisalTemplateDto dto); }
    public interface IDeleteAppraisalTemplate { Task DeleteAsync(Guid id); }
    public interface IGetAppraisalTemplateById { Task<AppraisalTemplateDto> GetAsync(Guid id); }
    public interface IGetAllAppraisalTemplates { Task<PaginatedResponse<AppraisalTemplateDto>> GetAsync(GetAllRequest request); }

    internal static class AppraisalTemplateMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<AppraisalTemplate, AppraisalTemplateDto>> Projection = x => new AppraisalTemplateDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            GoalsWeight = x.GoalsWeight,
            CompetenciesWeight = x.CompetenciesWeight,
            IsActive = x.IsActive
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class CreateAppraisalTemplate(
        IRepository<AppraisalTemplate> repository,
        IValidator<CreateAppraisalTemplateDto> validator,
        ILogger<CreateAppraisalTemplate> logger) : ICreateAppraisalTemplate
    {
        public async Task<Guid> CreateAsync(CreateAppraisalTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(AppraisalTemplate), nameof(dto.Name), dto.Name);

            var entity = AppraisalTemplate.Create(dto.Name, dto.GoalsWeight, dto.CompetenciesWeight, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created AppraisalTemplate {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    public class UpdateAppraisalTemplate(
        IRepository<AppraisalTemplate> repository,
        IValidator<UpdateAppraisalTemplateDto> validator,
        ILogger<UpdateAppraisalTemplate> logger) : IUpdateAppraisalTemplate
    {
        public async Task UpdateAsync(UpdateAppraisalTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AppraisalTemplate), dto.Id.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(AppraisalTemplate), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, dto.GoalsWeight, dto.CompetenciesWeight, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated AppraisalTemplate {Id}", entity.Id);
        }
    }

    public class DeleteAppraisalTemplate(
        IRepository<AppraisalTemplate> repository,
        ILogger<DeleteAppraisalTemplate> logger) : IDeleteAppraisalTemplate
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AppraisalTemplate), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted AppraisalTemplate {Id}", id);
        }
    }

    public class GetAppraisalTemplateById(IRepository<AppraisalTemplate> repository) : IGetAppraisalTemplateById
    {
        public async Task<AppraisalTemplateDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(x => x.Id == id)
                .Select(AppraisalTemplateMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(AppraisalTemplate), id.ToString());
        }
    }

    public class GetAllAppraisalTemplates(IRepository<AppraisalTemplate> repository) : IGetAllAppraisalTemplates
    {
        public async Task<PaginatedResponse<AppraisalTemplateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(AppraisalTemplateMapper.Projection).ToListAsync();

            return new PaginatedResponse<AppraisalTemplateDto> { Total = total, Data = data };
        }
    }
}
