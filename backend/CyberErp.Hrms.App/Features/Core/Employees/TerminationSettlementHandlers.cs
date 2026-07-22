using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class SettlementLineDto
    {
        public Guid? Id { get; set; }
        /// <summary>Earning | Deduction.</summary>
        public string Kind { get; set; } = nameof(SettlementLineKind.Earning);
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsAutoSuggested { get; set; }
    }

    public class TerminationSettlementDto
    {
        public Guid Id { get; set; }
        public Guid TerminationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedOn { get; set; }
        public DateTime? PaidOn { get; set; }
        public string? PaidReference { get; set; }
        public string? Notes { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetAmount { get; set; }
        public List<SettlementLineDto> Lines { get; set; } = [];
    }

    public class UpdateSettlementLinesDto
    {
        public List<SettlementLineDto> Lines { get; set; } = [];
        public string? Notes { get; set; }
    }

    public class MarkSettlementPaidDto
    {
        public string? Reference { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    /// <summary>HC216 — creates the worksheet with auto-suggested lines (idempotent per case).</summary>
    public interface IBuildTerminationSettlement { Task<Guid> BuildAsync(Guid terminationId); }
    public interface IUpdateSettlementLines { Task UpdateAsync(Guid settlementId, UpdateSettlementLinesDto dto); }
    public interface IApproveTerminationSettlement { Task ApproveAsync(Guid settlementId); }
    public interface IMarkTerminationSettlementPaid { Task PayAsync(Guid settlementId, MarkSettlementPaidDto dto); }
    public interface IGetTerminationSettlement { Task<TerminationSettlementDto?> GetAsync(Guid terminationId); }

    internal static class SettlementShared
    {
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage settlements.");
        }

        internal static TerminationSettlementDto ToDto(TerminationSettlement s, List<SettlementLine> lines)
        {
            var earnings = lines.Where(l => l.Kind == SettlementLineKind.Earning).Sum(l => l.Amount);
            var deductions = lines.Where(l => l.Kind == SettlementLineKind.Deduction).Sum(l => l.Amount);
            return new TerminationSettlementDto
            {
                Id = s.Id,
                TerminationId = s.TerminationId,
                Status = s.Status.ToString(),
                ApprovedOn = s.ApprovedOn,
                PaidOn = s.PaidOn,
                PaidReference = s.PaidReference,
                Notes = s.Notes,
                TotalEarnings = earnings,
                TotalDeductions = deductions,
                NetAmount = earnings - deductions,
                Lines = lines.OrderBy(l => l.SortOrder)
                    .Select(l => new SettlementLineDto
                    {
                        Id = l.Id,
                        Kind = l.Kind.ToString(),
                        Label = l.Label,
                        Amount = l.Amount,
                        IsAutoSuggested = l.IsAutoSuggested
                    }).ToList()
            };
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class BuildTerminationSettlement(
        IRepository<TerminationSettlement> repository,
        IRepository<SettlementLine> lineRepository,
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<LeaveBalance> leaveBalanceRepository,
        IPerformanceVisibilityService visibility,
        ILogger<BuildTerminationSettlement> logger) : IBuildTerminationSettlement
    {
        public async Task<Guid> BuildAsync(Guid terminationId)
        {
            await SettlementShared.EnsureAdminAsync(visibility);

            var termination = await terminationRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == terminationId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());
            if (termination.Status == TerminationStatus.Cancelled)
                throw new ValidationException(nameof(terminationId), "A cancelled case has no settlement.");

            var existing = await repository.GetAll()
                .Where(x => x.TerminationId == terminationId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
            if (existing.HasValue) return existing.Value;

            var employee = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => e.Id == termination.EmployeeId)
                .Select(e => new { e.Salary, e.HireDate })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), termination.EmployeeId.ToString());

            var settlement = TerminationSettlement.Create(terminationId,
                "Auto-suggested amounts are ADVISORY — review and adjust before approval.");
            await repository.AddAsync(settlement);

            // ---- Auto-suggested lines (HC216) — advisory, HR edits while Draft -------------
            var order = 1;
            var salary = employee.Salary ?? 0m;

            // Accumulated leave payout: SUM of remaining balances (all leave types) × daily rate (÷30).
            var balances = await leaveBalanceRepository.GetAll().AsNoTracking()
                .Where(b => b.EmployeeId == termination.EmployeeId)
                .Select(b => new { b.Entitled, b.CarriedForward, b.Adjusted, b.Taken })
                .ToListAsync();
            var remainingDays = balances.Sum(b => b.Entitled + b.CarriedForward + b.Adjusted - b.Taken);
            if (remainingDays > 0 && salary > 0)
            {
                var payout = Math.Round(remainingDays * salary / 30m, 2);
                var line = SettlementLine.Create(settlement.Id, SettlementLineKind.Earning,
                    $"Accumulated leave payout ({remainingDays:0.#} days)", payout, isAutoSuggested: true, order++);
                if (string.IsNullOrEmpty(line.TenantId)) line.TenantId = settlement.TenantId;
                await lineRepository.AddAsync(line);
            }

            // Severance suggestion (involuntary exits): 30 days' salary for the first year of
            // service + 10 days per additional year — a starting point, not a legal calculation.
            if (termination.TerminationType == TerminationType.Involuntary && salary > 0 && employee.HireDate.HasValue)
            {
                var years = Math.Max(1, (int)((termination.LastWorkingDate - employee.HireDate.Value).TotalDays / 365.25));
                var severance = Math.Round(salary + salary / 3m * (years - 1), 2);
                var line = SettlementLine.Create(settlement.Id, SettlementLineKind.Earning,
                    $"Severance pay suggestion ({years} year(s) of service)", severance, isAutoSuggested: true, order++);
                if (string.IsNullOrEmpty(line.TenantId)) line.TenantId = settlement.TenantId;
                await lineRepository.AddAsync(line);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Settlement {Id} built for case {Case}", settlement.Id, terminationId);
            return settlement.Id;
        }
    }

    public class UpdateSettlementLines(
        IRepository<TerminationSettlement> repository,
        IRepository<SettlementLine> lineRepository,
        IPerformanceVisibilityService visibility,
        ILogger<UpdateSettlementLines> logger) : IUpdateSettlementLines
    {
        public async Task UpdateAsync(Guid settlementId, UpdateSettlementLinesDto dto)
        {
            await SettlementShared.EnsureAdminAsync(visibility);
            if (dto.Lines.Count == 0)
                throw new ValidationException(nameof(dto.Lines), "A settlement needs at least one line.");
            if (dto.Notes?.Length > 1000)
                throw new ValidationException(nameof(dto.Notes), "Notes are at most 1000 characters.");

            var settlement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == settlementId)
                ?? throw new NotFoundException(nameof(TerminationSettlement), settlementId.ToString());
            if (settlement.Status != SettlementStatus.Draft)
                throw new ValidationException(nameof(settlementId), $"The settlement is {settlement.Status} — the worksheet is locked.");

            // Replace the worksheet: children ride their OWN repository (aggregate-child rule).
            var current = await lineRepository.GetAll()
                .Where(l => l.TerminationSettlementId == settlementId).ToListAsync();
            foreach (var line in current) lineRepository.Delete(line);

            var order = 1;
            foreach (var dtoLine in dto.Lines)
            {
                if (!Enum.TryParse<SettlementLineKind>(dtoLine.Kind, true, out var kind))
                    throw new ValidationException(nameof(dto.Lines), $"Line \"{dtoLine.Label}\": kind must be Earning or Deduction.");
                var line = SettlementLine.Create(settlementId, kind, dtoLine.Label.Trim(), dtoLine.Amount,
                    dtoLine.IsAutoSuggested, order++);
                if (string.IsNullOrEmpty(line.TenantId)) line.TenantId = settlement.TenantId;
                await lineRepository.AddAsync(line);
            }

            settlement.UpdateNotes(dto.Notes);
            repository.UpdateAsync(settlement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Settlement {Id} worksheet updated ({Count} lines)", settlementId, dto.Lines.Count);
        }
    }

    public class ApproveTerminationSettlement(
        IRepository<TerminationSettlement> repository,
        IPerformanceVisibilityService visibility,
        ILogger<ApproveTerminationSettlement> logger) : IApproveTerminationSettlement
    {
        public async Task ApproveAsync(Guid settlementId)
        {
            await SettlementShared.EnsureAdminAsync(visibility);
            var settlement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == settlementId)
                ?? throw new NotFoundException(nameof(TerminationSettlement), settlementId.ToString());
            if (settlement.Status != SettlementStatus.Draft)
                throw new ValidationException(nameof(settlementId), $"Only a draft settlement can be approved (current: {settlement.Status}).");
            settlement.Approve(DateTime.UtcNow.Date);
            repository.UpdateAsync(settlement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Settlement {Id} approved (worksheet locked)", settlementId);
        }
    }

    public class MarkTerminationSettlementPaid(
        IRepository<TerminationSettlement> repository,
        IPerformanceVisibilityService visibility,
        ILogger<MarkTerminationSettlementPaid> logger) : IMarkTerminationSettlementPaid
    {
        public async Task PayAsync(Guid settlementId, MarkSettlementPaidDto dto)
        {
            await SettlementShared.EnsureAdminAsync(visibility);
            var settlement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == settlementId)
                ?? throw new NotFoundException(nameof(TerminationSettlement), settlementId.ToString());
            if (settlement.Status != SettlementStatus.Approved)
                throw new ValidationException(nameof(settlementId), $"Only an approved settlement can be paid (current: {settlement.Status}).");
            settlement.MarkPaid(DateTime.UtcNow.Date, string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim());
            repository.UpdateAsync(settlement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Settlement {Id} marked paid (payroll hand-off, HC217)", settlementId);
        }
    }

    public class GetTerminationSettlement(
        IRepository<TerminationSettlement> repository,
        IRepository<SettlementLine> lineRepository,
        IRepository<EmployeeTermination> terminationRepository,
        IPerformanceVisibilityService visibility) : IGetTerminationSettlement
    {
        public async Task<TerminationSettlementDto?> GetAsync(Guid terminationId)
        {
            var leaverId = await terminationRepository.GetAll().AsNoTracking()
                .Where(t => t.Id == terminationId).Select(t => (Guid?)t.EmployeeId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());

            var scope = await visibility.GetScopeAsync();
            var isLeaver = scope.EmployeeId.HasValue && scope.EmployeeId.Value == leaverId;
            if (!scope.IsAdmin && !isLeaver && !await visibility.CanAccessEmployeeAsync(leaverId))
                throw new ValidationException(nameof(terminationId), "You do not have access to this exit case.");

            var settlement = await repository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(x => x.TerminationId == terminationId);
            if (settlement is null) return null;

            var lines = await lineRepository.GetAll().AsNoTracking()
                .Where(l => l.TerminationSettlementId == settlement.Id).ToListAsync();
            return SettlementShared.ToDto(settlement, lines);
        }
    }
}
