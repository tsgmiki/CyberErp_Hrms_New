using System.Linq.Expressions;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    public class CareerPathChangeRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid? CurrentCareerPathId { get; set; }
        public Guid RequestedCareerPathId { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = nameof(CareerPathChangeStatus.Draft);
        public string? DecisionNotes { get; set; }
        public DateTime? DecidedAt { get; set; }
    }

    public class SaveCareerPathChangeRequestDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? CurrentCareerPathId { get; set; }
        public Guid RequestedCareerPathId { get; set; }
        public string? Reason { get; set; }
    }

    public class DecideCareerPathChangeRequestDto
    {
        public string? DecisionNotes { get; set; }
    }

    public class SaveCareerPathChangeRequestDtoValidator : AbstractValidator<SaveCareerPathChangeRequestDto>
    {
        public SaveCareerPathChangeRequestDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.RequestedCareerPathId).NotEmpty();
            RuleFor(x => x.Reason).MaximumLength(2000);
        }
    }

    internal static class CareerPathChangeRequestMapper
    {
        internal static readonly Expression<Func<CareerPathChangeRequest, CareerPathChangeRequestDto>> Projection = r => new CareerPathChangeRequestDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null && r.Employee.Person != null
                ? (r.Employee.Person.FirstName + " " + r.Employee.Person.GrandFatherName) : null,
            EmployeeNumber = r.Employee != null ? r.Employee.EmployeeNumber : null,
            CurrentCareerPathId = r.CurrentCareerPathId,
            RequestedCareerPathId = r.RequestedCareerPathId,
            Reason = r.Reason,
            Status = r.Status.ToString(),
            DecisionNotes = r.DecisionNotes,
            DecidedAt = r.DecidedAt
        };
    }

    public interface ISaveCareerPathChangeRequest { Task<Guid> SaveAsync(SaveCareerPathChangeRequestDto dto); }
    public interface IDeleteCareerPathChangeRequest { Task DeleteAsync(Guid id); }
    public interface IGetCareerPathChangeRequestById { Task<CareerPathChangeRequestDto> GetAsync(Guid id); }
    public interface IGetAllCareerPathChangeRequests { Task<PaginatedResponse<CareerPathChangeRequestDto>> GetAsync(GetAllRequest request); }
    public interface ISubmitCareerPathChangeRequest { Task SubmitAsync(Guid id); }
    public interface IApproveCareerPathChangeRequest { Task ApproveAsync(Guid id, DecideCareerPathChangeRequestDto dto); }
    public interface IRejectCareerPathChangeRequest { Task RejectAsync(Guid id, DecideCareerPathChangeRequestDto dto); }

    public class SaveCareerPathChangeRequest(
        IRepository<CareerPathChangeRequest> repository,
        IValidator<SaveCareerPathChangeRequestDto> validator,
        ILogger<SaveCareerPathChangeRequest> logger) : ISaveCareerPathChangeRequest
    {
        public async Task<Guid> SaveAsync(SaveCareerPathChangeRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(CareerPathChangeRequest), dto.Id.Value.ToString());
                entity.Update(dto.CurrentCareerPathId, dto.RequestedCareerPathId, dto.Reason);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = CareerPathChangeRequest.Create(dto.EmployeeId, dto.CurrentCareerPathId,
                dto.RequestedCareerPathId, dto.Reason);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CareerPathChangeRequest {Id} for employee {EmpId}", created.Id, dto.EmployeeId);
            return created.Id;
        }
    }

    public class SubmitCareerPathChangeRequest(
        IRepository<CareerPathChangeRequest> repository,
        IWorkflowService workflowService) : ISubmitCareerPathChangeRequest
    {
        public async Task SubmitAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), id.ToString());
            if (entity.Status is CareerPathChangeStatus.Approved or CareerPathChangeStatus.Rejected)
                throw new ValidationException(nameof(id), "A decided request can no longer be submitted.");

            // If an approval chain is configured (HC169), verify it can run before committing the submission.
            await workflowService.EnsureStartableAsync(WorkflowEntityTypes.CareerPathChangeRequest, entity.EmployeeId);
            entity.Submit();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();

            // Starts the workflow only when a definition exists; otherwise the manual approve/reject stands.
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.CareerPathChangeRequest, entity.Id,
                entity.EmployeeId, $"Career path change request for employee {entity.EmployeeId}");
        }
    }

    public class ApproveCareerPathChangeRequest(
        IRepository<CareerPathChangeRequest> repository,
        IRepository<EmployeeCareerPath> assignmentRepository,
        IWorkflowGate workflowGate,
        ILogger<ApproveCareerPathChangeRequest> logger) : IApproveCareerPathChangeRequest
    {
        public async Task ApproveAsync(Guid id, DecideCareerPathChangeRequestDto dto)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), id.ToString());
            // A configured workflow (HC169) owns the decision — approve it from the workflow screen instead.
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.CareerPathChangeRequest, id);
            if (entity.Status != CareerPathChangeStatus.Submitted)
                throw new ValidationException(nameof(id), "Only a submitted request can be approved.");
            entity.Approve(dto.DecisionNotes);
            repository.UpdateAsync(entity);

            // Approval assigns the employee to the requested path if not already assigned (HC169).
            var alreadyAssigned = await assignmentRepository.GetAll()
                .AnyAsync(a => a.EmployeeId == entity.EmployeeId && a.CareerPathId == entity.RequestedCareerPathId);
            if (!alreadyAssigned)
            {
                var assignment = EmployeeCareerPath.Create(entity.EmployeeId, entity.RequestedCareerPathId,
                    null, "Change request approval", null, EmployeeCareerPathStatus.Active, entity.Reason);
                await assignmentRepository.AddAsync(assignment);
            }
            await repository.SaveChangesAsync();
            logger.LogInformation("Approved CareerPathChangeRequest {Id}", id);
        }
    }

    public class RejectCareerPathChangeRequest(
        IRepository<CareerPathChangeRequest> repository,
        IWorkflowGate workflowGate) : IRejectCareerPathChangeRequest
    {
        public async Task RejectAsync(Guid id, DecideCareerPathChangeRequestDto dto)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), id.ToString());
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.CareerPathChangeRequest, id);
            if (entity.Status != CareerPathChangeStatus.Submitted)
                throw new ValidationException(nameof(id), "Only a submitted request can be rejected.");
            entity.Reject(dto.DecisionNotes);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class DeleteCareerPathChangeRequest(IRepository<CareerPathChangeRequest> repository) : IDeleteCareerPathChangeRequest
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetCareerPathChangeRequestById(IRepository<CareerPathChangeRequest> repository) : IGetCareerPathChangeRequestById
    {
        public async Task<CareerPathChangeRequestDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(CareerPathChangeRequestMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(CareerPathChangeRequest), id.ToString());
    }

    public class GetAllCareerPathChangeRequests(IRepository<CareerPathChangeRequest> repository) : IGetAllCareerPathChangeRequests
    {
        public async Task<PaginatedResponse<CareerPathChangeRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CareerPathChangeStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Employee != null && x.Employee.Person != null
                    && (x.Employee.Person.FirstName.Contains(term) || x.Employee.EmployeeNumber.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(CareerPathChangeRequestMapper.Projection).ToListAsync();
            return new PaginatedResponse<CareerPathChangeRequestDto> { Total = total, Data = data };
        }
    }
}
