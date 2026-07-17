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
    public class EmployeeCareerStepProgressDto
    {
        public Guid Id { get; set; }
        public Guid CareerPathStepId { get; set; }
        public string Status { get; set; } = nameof(CareerStepProgressStatus.NotStarted);
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class EmployeeCareerPathDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid CareerPathId { get; set; }
        public string? CareerPathName { get; set; }
        public Guid? CurrentStepId { get; set; }
        public string? AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
        public decimal ProgressPercent { get; set; }
        public string Status { get; set; } = nameof(EmployeeCareerPathStatus.Active);
        public string? Notes { get; set; }
        public List<EmployeeCareerStepProgressDto> StepProgress { get; set; } = [];
    }

    public class SaveEmployeeCareerStepProgressDto
    {
        public Guid CareerPathStepId { get; set; }
        public string Status { get; set; } = nameof(CareerStepProgressStatus.NotStarted);
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveEmployeeCareerPathDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid CareerPathId { get; set; }
        public Guid? CurrentStepId { get; set; }
        public string? AssignedBy { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string Status { get; set; } = nameof(EmployeeCareerPathStatus.Active);
        public string? Notes { get; set; }
        public List<SaveEmployeeCareerStepProgressDto> StepProgress { get; set; } = [];
    }

    public class SaveEmployeeCareerPathDtoValidator : AbstractValidator<SaveEmployeeCareerPathDto>
    {
        public SaveEmployeeCareerPathDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.CareerPathId).NotEmpty();
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeCareerPathStatus>(v, out _)).WithMessage("Invalid status.");
            RuleFor(x => x.Notes).MaximumLength(2000);
            RuleForEach(x => x.StepProgress).ChildRules(p =>
            {
                p.RuleFor(x => x.CareerPathStepId).NotEmpty();
                p.RuleFor(x => x.Status).NotEmpty()
                    .Must(v => Enum.TryParse<CareerStepProgressStatus>(v, out _)).WithMessage("Invalid step status.");
                p.RuleFor(x => x.Notes).MaximumLength(1000);
            });
        }
    }

    internal static class EmployeeCareerPathMapper
    {
        internal static readonly Expression<Func<EmployeeCareerPath, EmployeeCareerPathDto>> Projection = e => new EmployeeCareerPathDto
        {
            Id = e.Id,
            EmployeeId = e.EmployeeId,
            EmployeeName = e.Employee != null && e.Employee.Person != null
                ? (e.Employee.Person.FirstName + " " + e.Employee.Person.GrandFatherName) : null,
            EmployeeNumber = e.Employee != null ? e.Employee.EmployeeNumber : null,
            CareerPathId = e.CareerPathId,
            CareerPathName = e.CareerPath != null ? e.CareerPath.Name : null,
            CurrentStepId = e.CurrentStepId,
            AssignedBy = e.AssignedBy,
            AssignedDate = e.AssignedDate,
            ProgressPercent = e.ProgressPercent,
            Status = e.Status.ToString(),
            Notes = e.Notes,
            StepProgress = e.StepProgress.Select(p => new EmployeeCareerStepProgressDto
            {
                Id = p.Id, CareerPathStepId = p.CareerPathStepId, Status = p.Status.ToString(),
                CompletedDate = p.CompletedDate, Notes = p.Notes
            }).ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted progress rows copy it here.</summary>
        internal static void StampProgressTenant(EmployeeCareerPath e)
        {
            foreach (var p in e.StepProgress)
                if (string.IsNullOrEmpty(p.TenantId)) p.TenantId = e.TenantId;
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveEmployeeCareerPath { Task<Guid> SaveAsync(SaveEmployeeCareerPathDto dto); }
    public interface IDeleteEmployeeCareerPath { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeCareerPathById { Task<EmployeeCareerPathDto> GetAsync(Guid id); }
    public interface IGetAllEmployeeCareerPaths { Task<PaginatedResponse<EmployeeCareerPathDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveEmployeeCareerPath(
        IRepository<EmployeeCareerPath> repository,
        IRepository<EmployeeCareerPathStepProgress> progressRepository,
        IValidator<SaveEmployeeCareerPathDto> validator,
        ILogger<SaveEmployeeCareerPath> logger) : ISaveEmployeeCareerPath
    {
        public async Task<Guid> SaveAsync(SaveEmployeeCareerPathDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // One assignment per employee per path.
            if (await repository.GetAll().AnyAsync(x => x.EmployeeId == dto.EmployeeId
                    && x.CareerPathId == dto.CareerPathId && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(EmployeeCareerPath), nameof(dto.CareerPathId), dto.CareerPathId.ToString());

            var status = Enum.Parse<EmployeeCareerPathStatus>(dto.Status);
            var specs = dto.StepProgress.Select(p => new CareerStepProgressSpec(
                p.CareerPathStepId, Enum.Parse<CareerStepProgressStatus>(p.Status), p.CompletedDate, p.Notes)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.StepProgress)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(EmployeeCareerPath), dto.Id.Value.ToString());

                foreach (var old in entity.StepProgress.ToList()) progressRepository.Delete(old);
                entity.Update(dto.EmployeeId, dto.CareerPathId, dto.CurrentStepId, dto.AssignedBy, status, dto.Notes);
                entity.SetStepProgress(specs); // recomputes ProgressPercent
                EmployeeCareerPathMapper.StampProgressTenant(entity);
                foreach (var p in entity.StepProgress) await progressRepository.AddAsync(p);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = EmployeeCareerPath.Create(dto.EmployeeId, dto.CareerPathId, dto.CurrentStepId,
                dto.AssignedBy, dto.AssignedDate, status, dto.Notes);
            created.SetStepProgress(specs); // recomputes ProgressPercent
            await repository.AddAsync(created);
            EmployeeCareerPathMapper.StampProgressTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Assigned employee {EmpId} to career path {PathId}", dto.EmployeeId, dto.CareerPathId);
            return created.Id;
        }
    }

    public class DeleteEmployeeCareerPath(IRepository<EmployeeCareerPath> repository) : IDeleteEmployeeCareerPath
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeCareerPath), id.ToString());
            repository.Delete(entity); // cascade removes step-progress
            await repository.SaveChangesAsync();
        }
    }

    public class GetEmployeeCareerPathById(IRepository<EmployeeCareerPath> repository) : IGetEmployeeCareerPathById
    {
        public async Task<EmployeeCareerPathDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(EmployeeCareerPathMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(EmployeeCareerPath), id.ToString());
    }

    public class GetAllEmployeeCareerPaths(IRepository<EmployeeCareerPath> repository) : IGetAllEmployeeCareerPaths
    {
        public async Task<PaginatedResponse<EmployeeCareerPathDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            // parentId = career path (utilization drilldown); employeeId filter reuses SearchText-less path.
            if (request.ParentId.HasValue)
                query = query.Where(x => x.CareerPathId == request.ParentId.Value);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EmployeeCareerPathStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Employee != null && x.Employee.Person != null
                    && (x.Employee.Person.FirstName.Contains(term) || x.Employee.EmployeeNumber.Contains(term)));
            }

            var total = await query.CountAsync();
            // List is lightweight — progress rows load only on GetById.
            var data = await query.OrderByDescending(x => x.AssignedDate).Skip(skip).Take(take)
                .Select(e => new EmployeeCareerPathDto
                {
                    Id = e.Id, EmployeeId = e.EmployeeId,
                    EmployeeName = e.Employee != null && e.Employee.Person != null
                        ? (e.Employee.Person.FirstName + " " + e.Employee.Person.GrandFatherName) : null,
                    EmployeeNumber = e.Employee != null ? e.Employee.EmployeeNumber : null,
                    CareerPathId = e.CareerPathId, CareerPathName = e.CareerPath != null ? e.CareerPath.Name : null,
                    CurrentStepId = e.CurrentStepId, AssignedBy = e.AssignedBy, AssignedDate = e.AssignedDate,
                    ProgressPercent = e.ProgressPercent, Status = e.Status.ToString(), Notes = e.Notes
                }).ToListAsync();

            return new PaginatedResponse<EmployeeCareerPathDto> { Total = total, Data = data };
        }
    }
}
