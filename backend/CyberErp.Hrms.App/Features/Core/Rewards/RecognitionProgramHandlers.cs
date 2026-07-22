using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class RecognitionProgramDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Period { get; set; } = string.Empty;
        public Guid? RecognitionBadgeId { get; set; }
        public string? BadgeName { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveRecognitionProgramDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Monthly | Quarterly | Annual | AdHoc.</summary>
        public string Period { get; set; } = nameof(RecognitionProgramPeriod.AdHoc);
        public Guid? RecognitionBadgeId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveRecognitionProgramDtoValidator : AbstractValidator<SaveRecognitionProgramDto>
    {
        public SaveRecognitionProgramDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Period)
                .Must(p => Enum.TryParse<RecognitionProgramPeriod>(p, true, out _))
                .WithMessage("Period must be Monthly, Quarterly, Annual or AdHoc.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveRecognitionProgram { Task<Guid> SaveAsync(SaveRecognitionProgramDto dto); }
    public interface IDeleteRecognitionProgram { Task DeleteAsync(Guid id); }
    public interface IGetRecognitionProgramById { Task<RecognitionProgramDto> GetAsync(Guid id); }
    public interface IGetAllRecognitionPrograms { Task<PaginatedResponse<RecognitionProgramDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveRecognitionProgram(
        IRepository<RecognitionProgram> repository,
        IRepository<RecognitionBadge> badgeRepository,
        IValidator<SaveRecognitionProgramDto> validator,
        ILogger<SaveRecognitionProgram> logger) : ISaveRecognitionProgram
    {
        public async Task<Guid> SaveAsync(SaveRecognitionProgramDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(RecognitionProgram), nameof(dto.Name), dto.Name);
            if (dto.RecognitionBadgeId.HasValue &&
                !await badgeRepository.GetAll().AnyAsync(b => b.Id == dto.RecognitionBadgeId.Value))
                throw new NotFoundException(nameof(RecognitionBadge), dto.RecognitionBadgeId.Value.ToString());

            var period = Enum.Parse<RecognitionProgramPeriod>(dto.Period, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(RecognitionProgram), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, period, dto.RecognitionBadgeId, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated RecognitionProgram {Id}", entity.Id);
                return entity.Id;
            }

            var created = RecognitionProgram.Create(dto.Name, dto.Description, period, dto.RecognitionBadgeId, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created RecognitionProgram {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteRecognitionProgram(
        IRepository<RecognitionProgram> repository,
        IRepository<RewardNomination> nominationRepository,
        ILogger<DeleteRecognitionProgram> logger) : IDeleteRecognitionProgram
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(RecognitionProgram), id.ToString());
            if (await nominationRepository.GetAll().AnyAsync(n => n.RecognitionProgramId == id))
                throw new ValidationException(nameof(id), "Cannot delete a program that nominations reference.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted RecognitionProgram {Id}", id);
        }
    }

    public class GetRecognitionProgramById(
        IRepository<RecognitionProgram> repository,
        IRepository<RecognitionBadge> badgeRepository) : IGetRecognitionProgramById
    {
        public async Task<RecognitionProgramDto> GetAsync(Guid id)
        {
            var badges = badgeRepository.GetAll();
            return await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new RecognitionProgramDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Period = x.Period.ToString(),
                    RecognitionBadgeId = x.RecognitionBadgeId,
                    BadgeName = badges.Where(b => b.Id == x.RecognitionBadgeId).Select(b => b.Name).FirstOrDefault(),
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(RecognitionProgram), id.ToString());
        }
    }

    public class GetAllRecognitionPrograms(
        IRepository<RecognitionProgram> repository,
        IRepository<RecognitionBadge> badgeRepository) : IGetAllRecognitionPrograms
    {
        public async Task<PaginatedResponse<RecognitionProgramDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var badges = badgeRepository.GetAll();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(x => new RecognitionProgramDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Period = x.Period.ToString(),
                    RecognitionBadgeId = x.RecognitionBadgeId,
                    BadgeName = badges.Where(b => b.Id == x.RecognitionBadgeId).Select(b => b.Name).FirstOrDefault(),
                    IsActive = x.IsActive
                }).ToListAsync();

            return new PaginatedResponse<RecognitionProgramDto> { Total = total, Data = data };
        }
    }
}
