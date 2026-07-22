using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Lookups
{
    // ---- DTOs -------------------------------------------------------------------------------------
    public class LookupItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class LookupCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<LookupItemDto> Items { get; set; } = [];
    }

    public class SaveLookupItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SaveLookupCategoryDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<SaveLookupItemDto> Items { get; set; } = [];
    }

    public class SaveLookupCategoryDtoValidator : AbstractValidator<SaveLookupCategoryDto>
    {
        public SaveLookupCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleForEach(x => x.Items).ChildRules(i =>
            {
                i.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
                i.RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            });
        }
    }

    // ---- Interfaces (one per operation) -----------------------------------------------------------
    /// <summary>Read the value list of a lookup category by its CODE — the endpoint comboboxes call.</summary>
    public interface IGetLookupItems { Task<List<LookupItemDto>> GetAsync(string categoryCode); }
    public interface IGetAllLookupCategories { Task<PaginatedResponse<LookupCategoryDto>> GetAsync(GetAllRequest request); }
    public interface ISaveLookupCategory { Task<Guid> SaveAsync(SaveLookupCategoryDto dto); }
    public interface IDeleteLookupCategory { Task DeleteAsync(Guid id); }

    // ---- Handlers ---------------------------------------------------------------------------------
    public class GetLookupItems(IRepository<LookupCategory> repository) : IGetLookupItems
    {
        public async Task<List<LookupItemDto>> GetAsync(string categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode)) return [];
            var code = categoryCode.Trim();
            // Lookups are global (skip-listed from the tenant filter), so GetAll() returns them for any tenant.
            return await repository.GetAll()
                .Where(c => c.Code == code)
                .SelectMany(c => c.Items)
                .OrderBy(i => i.Name)
                .Select(i => new LookupItemDto { Id = i.Id, Name = i.Name, Code = i.Code })
                .ToListAsync();
        }
    }

    public class GetAllLookupCategories(IRepository<LookupCategory> repository) : IGetAllLookupCategories
    {
        public async Task<PaginatedResponse<LookupCategoryDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 100;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(c => new LookupCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Items = c.Items.OrderBy(i => i.Name)
                        .Select(i => new LookupItemDto { Id = i.Id, Name = i.Name, Code = i.Code }).ToList()
                })
                .ToListAsync();
            return new PaginatedResponse<LookupCategoryDto> { Total = total, Data = data };
        }
    }

    /// <summary>Upsert a category and its value list in one call (master-detail — mirrors ClearanceDepartment).</summary>
    public class SaveLookupCategory(
        IRepository<LookupCategory> repository,
        IRepository<LookupCategoryList> itemRepository,
        IValidator<SaveLookupCategoryDto> validator) : ISaveLookupCategory
    {
        public async Task<Guid> SaveAsync(SaveLookupCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(LookupCategory), nameof(dto.Code), dto.Code);

            var items = dto.Items.Select(i => (i.Name, i.Code));

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(c => c.Items)
                    .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LookupCategory), dto.Id.Value.ToString());

                entity.Update(dto.Name, dto.Code);
                entity.SetItems(items);
                // Explicitly Add the re-created child rows (context.Update(root) would otherwise treat the
                // app-generated keys as existing/Modified — same reason as the ClearanceDepartment save).
                foreach (var item in entity.Items)
                    await itemRepository.AddAsync(item);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = LookupCategory.Create(dto.Name, dto.Code);
            created.SetItems(items);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteLookupCategory(IRepository<LookupCategory> repository) : IDeleteLookupCategory
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(LookupCategory), id.ToString());
            repository.Delete(entity); // FK cascade removes the value rows
            await repository.SaveChangesAsync();
        }
    }
}
