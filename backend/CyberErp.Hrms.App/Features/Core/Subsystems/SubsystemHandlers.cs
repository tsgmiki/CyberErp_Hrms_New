using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Subsystems
{
    // ---- DTOs ---------------------------------------------------------------

    public class SubsystemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class SubsystemDtoValidator : AbstractValidator<SubsystemDto>
    {
        public SubsystemDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }

    // ---- Save (create or update) -------------------------------------------

    public interface ISaveSubsystem { Task<Guid> SaveAsync(SubsystemDto dto); }

    public class SaveSubsystem(
        IRepository<Subsystem> repository,
        IUnitOfWork unitOfWork,
        IValidator<SubsystemDto> validator,
        ILogger<SaveSubsystem> logger) : ISaveSubsystem
    {
        public async Task<Guid> SaveAsync(SubsystemDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new FluentValidation.ValidationException(validation.Errors);

            var duplicate = await repository.GetAll()
                .AnyAsync(s => s.Id != dto.Id && s.Name == dto.Name.Trim());
            if (duplicate)
                throw new ValidationException(nameof(dto.Name), $"A subsystem named '{dto.Name}' already exists.");

            if (dto.Id != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(s => s.Id == dto.Id)
                    ?? throw new ValidationException(nameof(dto.Id), "Subsystem not found.");
                entity.Update(dto.Name, dto.Code, dto.SortOrder);
                repository.UpdateAsync(entity);
                await unitOfWork.SaveChangesAsync();
                return entity.Id;
            }

            var subsystem = Subsystem.Create(dto.Name, dto.Code, dto.SortOrder);
            await repository.AddAsync(subsystem);
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Subsystem {Name} created", subsystem.Name);
            return subsystem.Id;
        }
    }

    // ---- GetAll (paged) -----------------------------------------------------

    public interface IGetAllSubsystems { Task<PaginatedResponse<SubsystemDto>> GetAsync(GetAllRequest request); }

    public class GetAllSubsystems(IRepository<Subsystem> repository) : IGetAllSubsystems
    {
        public async Task<PaginatedResponse<SubsystemDto>> GetAsync(GetAllRequest request)
        {
            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(s => s.Name.Contains(request.SearchText) || s.Code.Contains(request.SearchText));

            var total = await query.CountAsync();

            int skip = int.TryParse(request.Skip, out var s) ? s : 0;
            int take = int.TryParse(request.Take, out var t) ? t : 15;

            var data = await query
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(x => new SubsystemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    SortOrder = x.SortOrder
                })
                .ToListAsync();

            return new PaginatedResponse<SubsystemDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------

    public interface IDeleteSubsystem { Task DeleteAsync(Guid id); }

    public class DeleteSubsystem(
        IRepository<Subsystem> repository,
        IRepository<Module> moduleRepository,
        IUnitOfWork unitOfWork) : IDeleteSubsystem
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new ValidationException(nameof(id), "Subsystem not found.");

            // Block deleting a subsystem that still owns menu modules.
            var inUse = await moduleRepository.GetAll().AnyAsync(m => m.SubsystemId == entity.Id);
            if (inUse)
                throw new ValidationException(nameof(id), "This subsystem still has modules — move or delete them first.");

            repository.Delete(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
