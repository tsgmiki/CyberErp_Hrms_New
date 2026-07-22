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
    public class MentorshipDto
    {
        public Guid Id { get; set; }
        public Guid MentorEmployeeId { get; set; }
        public string? MentorName { get; set; }
        public Guid MenteeEmployeeId { get; set; }
        public string? MenteeName { get; set; }
        public string Context { get; set; } = nameof(MentorshipContext.General);
        public Guid? RefId { get; set; }
        public string Status { get; set; } = nameof(MentorshipStatus.Active);
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveMentorshipDto
    {
        public Guid? Id { get; set; }
        public Guid MentorEmployeeId { get; set; }
        public Guid MenteeEmployeeId { get; set; }
        public string Context { get; set; } = nameof(MentorshipContext.General);
        public Guid? RefId { get; set; }
        public string Status { get; set; } = nameof(MentorshipStatus.Active);
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveMentorshipDtoValidator : AbstractValidator<SaveMentorshipDto>
    {
        public SaveMentorshipDtoValidator()
        {
            RuleFor(x => x.MentorEmployeeId).NotEmpty();
            RuleFor(x => x.MenteeEmployeeId).NotEmpty()
                .NotEqual(x => x.MentorEmployeeId).WithMessage("A mentor and mentee must be different employees.");
            RuleFor(x => x.Context).NotEmpty()
                .Must(v => Enum.TryParse<MentorshipContext>(v, out _)).WithMessage("Invalid context.");
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<MentorshipStatus>(v, out _)).WithMessage("Invalid status.");
            RuleFor(x => x.Notes).MaximumLength(2000);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue).WithMessage("End date cannot be before start date.");
        }
    }

    internal static class MentorshipMapper
    {
        internal static readonly Expression<Func<Mentorship, MentorshipDto>> Projection = m => new MentorshipDto
        {
            Id = m.Id,
            MentorEmployeeId = m.MentorEmployeeId,
            MentorName = m.Mentor != null && m.Mentor.Person != null
                ? (m.Mentor.Person.FirstName + " " + m.Mentor.Person.GrandFatherName) : null,
            MenteeEmployeeId = m.MenteeEmployeeId,
            MenteeName = m.Mentee != null && m.Mentee.Person != null
                ? (m.Mentee.Person.FirstName + " " + m.Mentee.Person.GrandFatherName) : null,
            Context = m.Context.ToString(),
            RefId = m.RefId,
            Status = m.Status.ToString(),
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            Notes = m.Notes
        };
    }

    public interface ISaveMentorship { Task<Guid> SaveAsync(SaveMentorshipDto dto); }
    public interface IDeleteMentorship { Task DeleteAsync(Guid id); }
    public interface IGetMentorshipById { Task<MentorshipDto> GetAsync(Guid id); }
    public interface IGetAllMentorships { Task<PaginatedResponse<MentorshipDto>> GetAsync(GetAllRequest request); }

    public class SaveMentorship(
        IRepository<Mentorship> repository,
        IValidator<SaveMentorshipDto> validator,
        ILogger<SaveMentorship> logger) : ISaveMentorship
    {
        public async Task<Guid> SaveAsync(SaveMentorshipDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var context = Enum.Parse<MentorshipContext>(dto.Context);
            var status = Enum.Parse<MentorshipStatus>(dto.Status);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Mentorship), dto.Id.Value.ToString());
                entity.Update(dto.MentorEmployeeId, dto.MenteeEmployeeId, context, dto.RefId, status,
                    dto.StartDate, dto.EndDate, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = Mentorship.Create(dto.MentorEmployeeId, dto.MenteeEmployeeId, context, dto.RefId,
                status, dto.StartDate, dto.EndDate, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Mentorship {Id} (mentor {Mentor} → mentee {Mentee})",
                created.Id, dto.MentorEmployeeId, dto.MenteeEmployeeId);
            return created.Id;
        }
    }

    public class DeleteMentorship(IRepository<Mentorship> repository) : IDeleteMentorship
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Mentorship), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetMentorshipById(IRepository<Mentorship> repository) : IGetMentorshipById
    {
        public async Task<MentorshipDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id).Select(MentorshipMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(Mentorship), id.ToString());
    }

    public class GetAllMentorships(IRepository<Mentorship> repository) : IGetAllMentorships
    {
        public async Task<PaginatedResponse<MentorshipDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            // parentId = mentee (an employee's mentors); employeeId filter reuses the same axis.
            if (request.ParentId.HasValue)
                query = query.Where(x => x.MenteeEmployeeId == request.ParentId.Value);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.MenteeEmployeeId == request.EmployeeId.Value || x.MentorEmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<MentorshipStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x =>
                    (x.Mentor != null && x.Mentor.Person != null && x.Mentor.Person.FirstName.Contains(term)) ||
                    (x.Mentee != null && x.Mentee.Person != null && x.Mentee.Person.FirstName.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take)
                .Select(MentorshipMapper.Projection).ToListAsync();
            return new PaginatedResponse<MentorshipDto> { Total = total, Data = data };
        }
    }
}
