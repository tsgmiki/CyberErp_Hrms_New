using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Loans
{
    // ---- DTOs ---------------------------------------------------------------
    public class LoanGuarantorDto
    {
        public Guid Id { get; set; }
        public Guid? GuarantorEmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? IdentificationNumber { get; set; }
        public string? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? GuaranteedAmount { get; set; }
    }

    public class LoanScheduleLineDto
    {
        public Guid Id { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalPortion { get; set; }
        public decimal InterestPortion { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class LoanDto
    {
        public Guid Id { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid LoanTypeId { get; set; }
        public string? LoanTypeName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal InterestRatePct { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalRepayable { get; set; }
        public string? Purpose { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public int ServiceCommitmentMonths { get; set; }
        public DateTime? DisbursedAt { get; set; }
        public string? DisbursementReference { get; set; }
        public DateTime? SettledAt { get; set; }
        public DateTime? ServiceCommitmentConsentAt { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int PaidInstallmentCount { get; set; }
        public int TotalInstallmentCount { get; set; }
        public List<LoanGuarantorDto> Guarantors { get; set; } = [];
        public List<LoanScheduleLineDto> Schedule { get; set; } = [];
    }

    public class LoanGuarantorInputDto
    {
        public Guid? GuarantorEmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? IdentificationNumber { get; set; }
        public string? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? GuaranteedAmount { get; set; }
    }

    public class RequestLoanDto
    {
        /// <summary>HR may request on behalf of an employee; employees request for themselves.</summary>
        public Guid? EmployeeId { get; set; }
        public Guid LoanTypeId { get; set; }
        public decimal PrincipalAmount { get; set; }
        public int TermMonths { get; set; }
        public string? Purpose { get; set; }
        public List<LoanGuarantorInputDto> Guarantors { get; set; } = [];
    }

    public class RequestLoanDtoValidator : AbstractValidator<RequestLoanDto>
    {
        public RequestLoanDtoValidator()
        {
            RuleFor(x => x.LoanTypeId).NotEmpty();
            RuleFor(x => x.PrincipalAmount).GreaterThan(0);
            RuleFor(x => x.TermMonths).GreaterThan(0);
            RuleForEach(x => x.Guarantors).ChildRules(g => g.RuleFor(y => y.FullName).NotEmpty().WithMessage("Guarantor name is required."));
        }
    }

    public class ApproveLoanDto { public string? Note { get; set; } }
    public class RejectLoanDto { public string Reason { get; set; } = string.Empty; }

    // ---- Interfaces ---------------------------------------------------------
    public interface IRequestLoan { Task<Guid> RequestAsync(RequestLoanDto dto); }
    public interface IGetLoans { Task<PaginatedResponse<LoanDto>> GetAsync(GetAllRequest request); }
    public interface IGetLoanById { Task<LoanDto> GetAsync(Guid id); }
    public interface IApproveLoan { Task ApproveAsync(Guid id, string? note); }
    public interface IRejectLoan { Task RejectAsync(Guid id, string reason); }
    public interface ICancelLoan { Task CancelAsync(Guid id); }

    // ---- Schedule generation ------------------------------------------------
    internal static class LoanSchedule
    {
        /// <summary>Generates the amortization rows for a loan (flat interest split evenly; last row absorbs rounding).</summary>
        internal static List<LoanRepaymentScheduleLine> Build(Loan loan)
        {
            var rows = new List<LoanRepaymentScheduleLine>();
            var perPrincipal = Math.Round(loan.PrincipalAmount / loan.TermMonths, 2);
            var perInterest = Math.Round(loan.TotalInterest / loan.TermMonths, 2);
            decimal allocP = 0, allocI = 0, allocA = 0;
            for (var i = 1; i <= loan.TermMonths; i++)
            {
                var last = i == loan.TermMonths;
                var p = last ? loan.PrincipalAmount - allocP : perPrincipal;
                var interest = last ? loan.TotalInterest - allocI : perInterest;
                var amt = last ? loan.TotalRepayable - allocA : loan.MonthlyInstallment;
                allocP += p; allocI += interest; allocA += amt;
                rows.Add(LoanRepaymentScheduleLine.Create(loan.Id, i, loan.RequestDate.AddMonths(i), p, interest, amt));
            }
            return rows;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class RequestLoan(
        IRepository<Loan> repository,
        IRepository<LoanGuarantor> guarantorRepository,
        IRepository<LoanRepaymentScheduleLine> scheduleRepository,
        IRepository<LoanType> typeRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IGetCompensationSummary compensation,
        IWorkflowService workflowService,
        INumberSequenceService numberSequence,
        IValidator<RequestLoanDto> validator,
        ILogger<RequestLoan> logger) : IRequestLoan
    {
        public async Task<Guid> RequestAsync(RequestLoanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            Guid employeeId;
            if (scope.IsAdmin)
                employeeId = dto.EmployeeId ?? scope.EmployeeId ?? throw new ValidationException(nameof(dto.EmployeeId), "An employee is required.");
            else
            {
                employeeId = scope.EmployeeId ?? throw new ValidationException("scope", "Your account is not linked to an employee record.");
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != employeeId)
                    throw new ValidationException(nameof(dto.EmployeeId), "You can only request your own loans.");
            }
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var type = await typeRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.LoanTypeId)
                ?? throw new NotFoundException(nameof(LoanType), dto.LoanTypeId.ToString());
            if (!type.IsActive) throw new ValidationException(nameof(dto.LoanTypeId), "The selected loan type is inactive.");

            // ---- Loan-limit enforcement (HC251) ----
            if (dto.TermMonths > type.MaxTermMonths)
                throw new ValidationException(nameof(dto.TermMonths), $"Term exceeds the maximum of {type.MaxTermMonths} months for this loan type.");
            if (type.MaxAmount is decimal cap && dto.PrincipalAmount > cap)
                throw new ValidationException(nameof(dto.PrincipalAmount), $"Amount exceeds the maximum of {cap:N2} for this loan type.");
            if (type.MaxSalaryMultiple is decimal mult && mult > 0)
            {
                var baseSalary = (await compensation.GetAsync(employeeId)).BaseSalary;
                if (baseSalary > 0 && dto.PrincipalAmount > mult * baseSalary)
                    throw new ValidationException(nameof(dto.PrincipalAmount), $"Amount exceeds {mult:N2}× the monthly base salary ({mult * baseSalary:N2}).");
            }
            var minGuarantors = type.RequiresGuarantor ? Math.Max(1, type.MinGuarantors) : type.MinGuarantors;
            if (dto.Guarantors.Count < minGuarantors)
                throw new ValidationException(nameof(dto.Guarantors), $"This loan type requires at least {minGuarantors} guarantor(s).");

            var loanNumber = $"LN-{await numberSequence.NextAsync("Loan"):D5}";
            var created = Loan.Create(loanNumber, employeeId, type.Id, dto.PrincipalAmount, dto.TermMonths,
                type.InterestRatePct, type.ServiceCommitmentMonths, dto.Purpose, DateTime.UtcNow.Date);
            await repository.AddAsync(created);

            foreach (var g in dto.Guarantors)
                await guarantorRepository.AddAsync(LoanGuarantor.Create(created.Id, g.GuarantorEmployeeId, g.FullName,
                    g.IdentificationNumber, g.Relationship, g.PhoneNumber, g.GuaranteedAmount));
            foreach (var line in LoanSchedule.Build(created))
                await scheduleRepository.AddAsync(line);

            await repository.SaveChangesAsync();

            var name = await employeeRepository.GetAll().Where(e => e.Id == employeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefaultAsync();
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.EmployeeLoan, created.Id, employeeId,
                $"Loan {loanNumber} — {name} — {dto.PrincipalAmount:N2}");

            logger.LogInformation("Requested Loan {LoanNumber} for Employee {EmployeeId}", loanNumber, employeeId);
            return created.Id;
        }
    }

    public class GetLoans(
        IRepository<Loan> repository,
        IRepository<Employee> employeeRepository,
        IRepository<LoanType> typeRepository,
        IRepository<LoanRepaymentScheduleLine> scheduleRepository,
        IPerformanceVisibilityService visibility) : IGetLoans
    {
        public async Task<PaginatedResponse<LoanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                query = query.Where(l => l.EmployeeId == (scope.EmployeeId ?? Guid.Empty));  // own only

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<LoanStatus>(request.Status, true, out var st))
                query = query.Where(l => l.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(l => l.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(l => l.LoanNumber.Contains(request.SearchText.Trim()));

            var total = await query.CountAsync();
            var employees = employeeRepository.GetAll();
            var types = typeRepository.GetAll();
            var schedule = scheduleRepository.GetAll();
            var data = await query.OrderByDescending(l => l.RequestDate).ThenByDescending(l => l.CreatedAt)
                .Skip(skip).Take(take)
                .Select(l => new LoanDto
                {
                    Id = l.Id, LoanNumber = l.LoanNumber, EmployeeId = l.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == l.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    LoanTypeId = l.LoanTypeId, LoanTypeName = types.Where(x => x.Id == l.LoanTypeId).Select(x => x.Name).FirstOrDefault(),
                    PrincipalAmount = l.PrincipalAmount, TermMonths = l.TermMonths, InterestRatePct = l.InterestRatePct,
                    MonthlyInstallment = l.MonthlyInstallment, TotalInterest = l.TotalInterest, TotalRepayable = l.TotalRepayable,
                    Purpose = l.Purpose, RequestDate = l.RequestDate, Status = l.Status.ToString(), Resolution = l.Resolution,
                    ServiceCommitmentMonths = l.ServiceCommitmentMonths, DisbursedAt = l.DisbursedAt,
                    DisbursementReference = l.DisbursementReference, SettledAt = l.SettledAt, ServiceCommitmentConsentAt = l.ServiceCommitmentConsentAt,
                    OutstandingBalance = schedule.Where(sl => sl.LoanId == l.Id && sl.Status == LoanInstallmentStatus.Pending).Sum(sl => (decimal?)sl.Amount) ?? 0m,
                    PaidInstallmentCount = schedule.Count(sl => sl.LoanId == l.Id && sl.Status == LoanInstallmentStatus.Paid),
                    TotalInstallmentCount = schedule.Count(sl => sl.LoanId == l.Id)
                }).ToListAsync();
            return new PaginatedResponse<LoanDto> { Total = total, Data = data };
        }
    }

    public class GetLoanById(
        IRepository<Loan> repository,
        IRepository<LoanGuarantor> guarantorRepository,
        IRepository<LoanRepaymentScheduleLine> scheduleRepository,
        IRepository<Employee> employeeRepository,
        IRepository<LoanType> typeRepository,
        IPerformanceVisibilityService visibility) : IGetLoanById
    {
        public async Task<LoanDto> GetAsync(Guid id)
        {
            var employees = employeeRepository.GetAll();
            var types = typeRepository.GetAll();
            var dto = await repository.GetAll().AsNoTracking().Where(l => l.Id == id)
                .Select(l => new LoanDto
                {
                    Id = l.Id, LoanNumber = l.LoanNumber, EmployeeId = l.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == l.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    LoanTypeId = l.LoanTypeId, LoanTypeName = types.Where(x => x.Id == l.LoanTypeId).Select(x => x.Name).FirstOrDefault(),
                    PrincipalAmount = l.PrincipalAmount, TermMonths = l.TermMonths, InterestRatePct = l.InterestRatePct,
                    MonthlyInstallment = l.MonthlyInstallment, TotalInterest = l.TotalInterest, TotalRepayable = l.TotalRepayable,
                    Purpose = l.Purpose, RequestDate = l.RequestDate, Status = l.Status.ToString(), Resolution = l.Resolution,
                    ServiceCommitmentMonths = l.ServiceCommitmentMonths, DisbursedAt = l.DisbursedAt,
                    DisbursementReference = l.DisbursementReference, SettledAt = l.SettledAt, ServiceCommitmentConsentAt = l.ServiceCommitmentConsentAt
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Loan), id.ToString());

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this loan.");

            dto.Guarantors = await guarantorRepository.GetAll().AsNoTracking().Where(g => g.LoanId == id)
                .Select(g => new LoanGuarantorDto { Id = g.Id, GuarantorEmployeeId = g.GuarantorEmployeeId, FullName = g.FullName, IdentificationNumber = g.IdentificationNumber, Relationship = g.Relationship, PhoneNumber = g.PhoneNumber, GuaranteedAmount = g.GuaranteedAmount })
                .ToListAsync();
            dto.Schedule = await scheduleRepository.GetAll().AsNoTracking().Where(sl => sl.LoanId == id).OrderBy(sl => sl.InstallmentNo)
                .Select(sl => new LoanScheduleLineDto { Id = sl.Id, InstallmentNo = sl.InstallmentNo, DueDate = sl.DueDate, PrincipalPortion = sl.PrincipalPortion, InterestPortion = sl.InterestPortion, Amount = sl.Amount, Status = sl.Status.ToString(), PaidAt = sl.PaidAt })
                .ToListAsync();
            dto.TotalInstallmentCount = dto.Schedule.Count;
            dto.PaidInstallmentCount = dto.Schedule.Count(sl => sl.Status == nameof(LoanInstallmentStatus.Paid));
            dto.OutstandingBalance = dto.Schedule.Where(sl => sl.Status == nameof(LoanInstallmentStatus.Pending)).Sum(sl => sl.Amount);
            return dto;
        }
    }

    public class ApproveLoan(
        IRepository<Loan> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IApproveLoan
    {
        public async Task ApproveAsync(Guid id, string? note)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can approve loans.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeLoan, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            if (entity.Status != LoanStatus.Requested) throw new ValidationException(nameof(id), "Only a requested loan can be approved.");
            entity.Approve(note);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class RejectLoan(
        IRepository<Loan> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IRejectLoan
    {
        public async Task RejectAsync(Guid id, string reason)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can reject loans.");
            if (string.IsNullOrWhiteSpace(reason)) throw new ValidationException(nameof(reason), "A rejection reason is required.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeLoan, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            if (entity.Status != LoanStatus.Requested) throw new ValidationException(nameof(id), "Only a requested loan can be rejected.");
            entity.Reject(reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class CancelLoan(
        IRepository<Loan> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : ICancelLoan
    {
        public async Task CancelAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            // Owner may cancel their own request; HR may cancel any.
            if (!scope.IsAdmin && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(id), "You can only cancel your own loan requests.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeLoan, id);
            entity.Cancel();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }
}
