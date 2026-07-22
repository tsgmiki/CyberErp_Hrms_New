using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    // ---- DTOs ---------------------------------------------------------------
    public class AllowanceTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string CalcMethod { get; set; } = string.Empty;
        public decimal? DefaultRate { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveAllowanceTypeDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string CalcMethod { get; set; } = nameof(AllowanceCalcMethod.Fixed);
        public decimal? DefaultRate { get; set; }
        public bool IsTaxable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class SaveAllowanceTypeDtoValidator : AbstractValidator<SaveAllowanceTypeDto>
    {
        public SaveAllowanceTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Code).MaximumLength(50);
            RuleFor(x => x.CalcMethod).NotEmpty()
                .Must(v => Enum.TryParse<AllowanceCalcMethod>(v, true, out _))
                .WithMessage("Calc method must be Fixed or PercentOfBase.");
            RuleFor(x => x.DefaultRate).GreaterThanOrEqualTo(0).When(x => x.DefaultRate.HasValue);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveAllowanceType { Task<Guid> SaveAsync(SaveAllowanceTypeDto dto); }
    public interface IDeleteAllowanceType { Task DeleteAsync(Guid id); }
    public interface IGetAllowanceTypeById { Task<AllowanceTypeDto> GetAsync(Guid id); }
    public interface IGetAllAllowanceTypes { Task<PaginatedResponse<AllowanceTypeDto>> GetAsync(GetAllRequest request); }

    internal static class AllowanceTypeMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<AllowanceType, AllowanceTypeDto>> Projection = a => new AllowanceTypeDto
        {
            Id = a.Id,
            Name = a.Name,
            Code = a.Code,
            CalcMethod = a.CalcMethod.ToString(),
            DefaultRate = a.DefaultRate,
            IsTaxable = a.IsTaxable,
            IsActive = a.IsActive,
            SortOrder = a.SortOrder
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveAllowanceType(
        IRepository<AllowanceType> repository,
        IValidator<SaveAllowanceTypeDto> validator,
        ILogger<SaveAllowanceType> logger) : ISaveAllowanceType
    {
        public async Task<Guid> SaveAsync(SaveAllowanceTypeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var method = Enum.Parse<AllowanceCalcMethod>(dto.CalcMethod, true);
            if (method == AllowanceCalcMethod.PercentOfBase && dto.DefaultRate is > 100)
                throw new ValidationException(nameof(dto.DefaultRate), "A percent-of-base rate cannot exceed 100.");

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(AllowanceType), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(AllowanceType), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code, method, dto.DefaultRate, dto.IsTaxable, dto.IsActive, dto.SortOrder);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated AllowanceType {Id}", entity.Id);
                return entity.Id;
            }

            var created = AllowanceType.Create(dto.Name, dto.Code, method, dto.DefaultRate, dto.IsTaxable, dto.IsActive, dto.SortOrder);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created AllowanceType {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteAllowanceType(
        IRepository<AllowanceType> repository,
        IRepository<EmployeeAllowance> employeeAllowanceRepository,
        ILogger<DeleteAllowanceType> logger) : IDeleteAllowanceType
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AllowanceType), id.ToString());
            if (await employeeAllowanceRepository.GetAll().AnyAsync(e => e.AllowanceTypeId == id))
                throw new ValidationException(nameof(id), "Cannot delete an allowance type that is assigned to employees.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted AllowanceType {Id}", id);
        }
    }

    public class GetAllowanceTypeById(IRepository<AllowanceType> repository) : IGetAllowanceTypeById
    {
        public async Task<AllowanceTypeDto> GetAsync(Guid id) =>
            await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(AllowanceTypeMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(AllowanceType), id.ToString());
    }

    public class GetAllAllowanceTypes(IRepository<AllowanceType> repository) : IGetAllAllowanceTypes
    {
        public async Task<PaginatedResponse<AllowanceTypeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.Code != null && x.Code.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take).Select(AllowanceTypeMapper.Projection).ToListAsync();

            return new PaginatedResponse<AllowanceTypeDto> { Total = total, Data = data };
        }
    }
}
