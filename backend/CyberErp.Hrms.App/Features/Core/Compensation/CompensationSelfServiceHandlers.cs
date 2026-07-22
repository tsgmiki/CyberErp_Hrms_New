using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    // ---- My Compensation (HC233) — one consolidated self-service read --------
    public class MyCompensationDto
    {
        public Guid EmployeeId { get; set; }
        public CompensationSummaryDto Summary { get; set; } = new();
        public List<BenefitEnrollmentDto> Benefits { get; set; } = [];
        public PayrollDeductionsDto Deductions { get; set; } = new();
    }

    public interface IGetMyCompensation { Task<MyCompensationDto> GetAsync(); }

    public class GetMyCompensation(
        IPerformanceVisibilityService visibility,
        IGetCompensationSummary summaryHandler,
        IGetEmployeeBenefits benefitsHandler,
        IGetPayrollDeductions deductionsHandler) : IGetMyCompensation
    {
        public async Task<MyCompensationDto> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");

            var empId = scope.EmployeeId.Value;
            return new MyCompensationDto
            {
                EmployeeId = empId,
                Summary = await summaryHandler.GetAsync(empId),
                Benefits = await benefitsHandler.GetAsync(empId),
                Deductions = await deductionsHandler.GetAsync(empId)
            };
        }
    }

    // ---- Compensation requests (HC234) --------------------------------------
    public class CompensationRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Guid? BenefitPlanId { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? ReferencePeriod { get; set; }
        public decimal? DisputedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }
    }

    public class SubmitCompensationRequestDto
    {
        /// <summary>HR may raise on behalf of an employee; when null the caller's own record is used.</summary>
        public Guid? EmployeeId { get; set; }
        public string RequestType { get; set; } = nameof(CompensationRequestType.BenefitChange);
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Guid? BenefitPlanId { get; set; }
        public string? ReferencePeriod { get; set; }
        public decimal? DisputedAmount { get; set; }
    }

    public class SubmitCompensationRequestDtoValidator : AbstractValidator<SubmitCompensationRequestDto>
    {
        public SubmitCompensationRequestDtoValidator()
        {
            RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Details).NotEmpty().MaximumLength(4000);
            RuleFor(x => x.RequestType).Must(v => Enum.TryParse<CompensationRequestType>(v, true, out _))
                .WithMessage("Request type must be BenefitChange or PayrollDiscrepancy.");
        }
    }

    public class ResolveCompensationRequestDto
    {
        /// <summary>UnderReview | Resolved | Rejected.</summary>
        public string Status { get; set; } = nameof(CompensationRequestStatus.Resolved);
        public string? Resolution { get; set; }
    }

    public interface ISubmitCompensationRequest { Task<Guid> SubmitAsync(SubmitCompensationRequestDto dto); }
    public interface IGetCompensationRequests { Task<PaginatedResponse<CompensationRequestDto>> GetAsync(GetAllRequest request); }
    public interface IResolveCompensationRequest { Task ResolveAsync(Guid id, ResolveCompensationRequestDto dto); }

    public class SubmitCompensationRequest(
        IRepository<CompensationRequest> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SubmitCompensationRequestDto> validator,
        ILogger<SubmitCompensationRequest> logger) : ISubmitCompensationRequest
    {
        public async Task<Guid> SubmitAsync(SubmitCompensationRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Employees raise requests about themselves; HR may raise on behalf of anyone.
            var scope = await visibility.GetScopeAsync();
            Guid employeeId;
            if (scope.IsAdmin)
            {
                employeeId = dto.EmployeeId ?? throw new ValidationException(nameof(dto.EmployeeId), "Select the employee.");
            }
            else
            {
                if (!scope.EmployeeId.HasValue)
                    throw new ValidationException("employee", "Your account is not linked to an employee record.");
                employeeId = scope.EmployeeId.Value;
            }
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var type = Enum.Parse<CompensationRequestType>(dto.RequestType, true);
            var created = CompensationRequest.Create(employeeId, type, dto.Subject, dto.Details,
                dto.BenefitPlanId, dto.ReferencePeriod, dto.DisputedAmount, DateTime.UtcNow.Date);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Submitted CompensationRequest {Id} ({Type}) for {EmployeeId}", created.Id, type, employeeId);
            return created.Id;
        }
    }

    public class GetCompensationRequests(
        IRepository<CompensationRequest> repository,
        IRepository<Employee> employeeRepository,
        IRepository<BenefitPlan> planRepository,
        IPerformanceVisibilityService visibility) : IGetCompensationRequests
    {
        public async Task<PaginatedResponse<CompensationRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                // Managers get their subtree; everyone else only their own requests.
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    var emps = employeeRepository.GetAll();
                    query = query.Where(r => r.EmployeeId == myEmp ||
                        emps.Any(e => e.Id == r.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(r => r.EmployeeId == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CompensationRequestStatus>(request.Status, true, out var st))
                query = query.Where(r => r.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

            var total = await query.CountAsync();
            var employees = employeeRepository.GetAll();
            var plans = planRepository.GetAll();
            var data = await query.OrderByDescending(r => r.SubmittedOn).ThenByDescending(r => r.CreatedAt)
                .Skip(skip).Take(take)
                .Select(r => new CompensationRequestDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == r.EmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == r.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    RequestType = r.RequestType.ToString(),
                    Subject = r.Subject,
                    Details = r.Details,
                    BenefitPlanId = r.BenefitPlanId,
                    BenefitPlanName = plans.Where(p => p.Id == r.BenefitPlanId).Select(p => p.Name).FirstOrDefault(),
                    ReferencePeriod = r.ReferencePeriod,
                    DisputedAmount = r.DisputedAmount,
                    Status = r.Status.ToString(),
                    Resolution = r.Resolution,
                    SubmittedOn = r.SubmittedOn,
                    ResolvedOn = r.ResolvedOn
                }).ToListAsync();

            return new PaginatedResponse<CompensationRequestDto> { Total = total, Data = data };
        }
    }

    public class ResolveCompensationRequest(
        IRepository<CompensationRequest> repository,
        IPerformanceVisibilityService visibility,
        ILogger<ResolveCompensationRequest> logger) : IResolveCompensationRequest
    {
        public async Task ResolveAsync(Guid id, ResolveCompensationRequestDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException(nameof(id), "Only HR can action compensation requests.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(CompensationRequest), id.ToString());

            var status = Enum.Parse<CompensationRequestStatus>(dto.Status, true);
            switch (status)
            {
                case CompensationRequestStatus.UnderReview:
                    entity.StartReview();
                    break;
                case CompensationRequestStatus.Resolved:
                    if (string.IsNullOrWhiteSpace(dto.Resolution))
                        throw new ValidationException(nameof(dto.Resolution), "A resolution is required.");
                    entity.Resolve(dto.Resolution!, DateTime.UtcNow.Date);
                    break;
                case CompensationRequestStatus.Rejected:
                    if (string.IsNullOrWhiteSpace(dto.Resolution))
                        throw new ValidationException(nameof(dto.Resolution), "A reason is required.");
                    entity.Reject(dto.Resolution!, DateTime.UtcNow.Date);
                    break;
                default:
                    throw new ValidationException(nameof(dto.Status), "Status must be UnderReview, Resolved or Rejected.");
            }

            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("CompensationRequest {Id} → {Status}", id, status);
        }
    }
}
