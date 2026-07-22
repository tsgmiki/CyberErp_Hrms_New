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
    public class CareerPathDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveCareerPathDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveCareerPathDtoValidator : AbstractValidator<SaveCareerPathDto>
    {
        public SaveCareerPathDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(2000);
        }
    }

    internal static class CareerPathMapper
    {
        internal static readonly Expression<Func<CareerPath, CareerPathDto>> Projection = p => new CareerPathDto
        {
            Id = p.Id, Name = p.Name, Code = p.Code, Description = p.Description, IsActive = p.IsActive
        };
    }

    public interface ISaveCareerPath { Task<Guid> SaveAsync(SaveCareerPathDto dto); }
    public interface IDeleteCareerPath { Task DeleteAsync(Guid id); }
    public interface IGetCareerPathById { Task<CareerPathDto> GetAsync(Guid id); }
    public interface IGetAllCareerPaths { Task<PaginatedResponse<CareerPathDto>> GetAsync(GetAllRequest request); }

    public class SaveCareerPath(
        IRepository<CareerPath> repository,
        IValidator<SaveCareerPathDto> validator,
        ILogger<SaveCareerPath> logger) : ISaveCareerPath
    {
        public async Task<Guid> SaveAsync(SaveCareerPathDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(CareerPath), nameof(dto.Code), dto.Code);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(CareerPath), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code, dto.Description, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = CareerPath.Create(dto.Name, dto.Code, dto.Description, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CareerPath {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteCareerPath(
        IRepository<CareerPath> repository,
        IRepository<EmployeeCareerPath> assignmentRepository,
        ILogger<DeleteCareerPath> logger) : IDeleteCareerPath
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CareerPath), id.ToString());
            if (await assignmentRepository.GetAll().AnyAsync(a => a.CareerPathId == id))
                throw new ValidationException(nameof(id), "Cannot delete a career path that has employee assignments.");
            repository.Delete(entity); // cascade removes its steps + step-competencies
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CareerPath {Id}", id);
        }
    }

    public class GetCareerPathById(IRepository<CareerPath> repository) : IGetCareerPathById
    {
        public async Task<CareerPathDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id).Select(CareerPathMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(CareerPath), id.ToString());
    }

    public class GetAllCareerPaths(IRepository<CareerPath> repository) : IGetAllCareerPaths
    {
        public async Task<PaginatedResponse<CareerPathDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(CareerPathMapper.Projection).ToListAsync();
            return new PaginatedResponse<CareerPathDto> { Total = total, Data = data };
        }
    }
}
