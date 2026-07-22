using System.Text;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class RewardDisbursementDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? BadgeName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? GrantedOn { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Reference { get; set; }
    }

    public class MarkDisbursementPaidDto
    {
        public string? Reference { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetAllRewardDisbursements { Task<PaginatedResponse<RewardDisbursementDto>> GetAsync(GetAllRequest request); }
    public interface IMarkRewardDisbursementPaid { Task PayAsync(Guid id, MarkDisbursementPaidDto dto); }
    /// <summary>HC185 — CSV hand-off for payroll/finance (no payroll module exists).</summary>
    public interface IExportRewardDisbursements { Task<string> ExportCsvAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    internal static class RewardDisbursementShared
    {
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR/finance administrators can manage disbursements.");
        }

        internal static IQueryable<RewardDisbursementDto> Project(
            IQueryable<RewardDisbursement> query,
            IQueryable<Employee> employees,
            IQueryable<RecognitionBadge> badges,
            IQueryable<EmployeeRecognition> recognitions)
        {
            return query.Select(d => new RewardDisbursementDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                EmployeeName = employees.Where(e => e.Id == d.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                EmployeeNumber = employees.Where(e => e.Id == d.EmployeeId)
                    .Select(e => e.EmployeeNumber).FirstOrDefault(),
                BadgeName = badges.Where(b => b.Id == d.RecognitionBadgeId).Select(b => b.Name).FirstOrDefault(),
                Amount = d.Amount,
                Status = d.Status.ToString(),
                GrantedOn = recognitions.Where(r => r.Id == d.EmployeeRecognitionId)
                    .Select(r => (DateTime?)r.RecognizedOn).FirstOrDefault(),
                PaidAt = d.PaidAt,
                Reference = d.Reference
            });
        }

        internal static IQueryable<RewardDisbursement> Filter(
            IQueryable<RewardDisbursement> query, GetAllRequest request, IQueryable<Employee> employees)
        {
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<DisbursementStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => employees.Any(e => e.Id == x.EmployeeId && (e.EmployeeNumber.Contains(term)
                    || (e.Person != null && (e.Person.FirstName.Contains(term) || e.Person.GrandFatherName.Contains(term))))));
            }
            return query;
        }
    }

    public class GetAllRewardDisbursements(
        IRepository<RewardDisbursement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<EmployeeRecognition> recognitionRepository,
        IPerformanceVisibilityService visibility) : IGetAllRewardDisbursements
    {
        public async Task<PaginatedResponse<RewardDisbursementDto>> GetAsync(GetAllRequest request)
        {
            await RewardDisbursementShared.EnsureAdminAsync(visibility);

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = RewardDisbursementShared.Filter(
                repository.GetAll().AsNoTracking(), request, employeeRepository.GetAll());

            var total = await query.CountAsync();
            var data = await RewardDisbursementShared.Project(
                    query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt).Skip(skip).Take(take),
                    employeeRepository.GetAll(), badgeRepository.GetAll(), recognitionRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<RewardDisbursementDto> { Total = total, Data = data };
        }
    }

    public class MarkRewardDisbursementPaid(
        IRepository<RewardDisbursement> repository,
        IPerformanceVisibilityService visibility,
        ILogger<MarkRewardDisbursementPaid> logger) : IMarkRewardDisbursementPaid
    {
        public async Task PayAsync(Guid id, MarkDisbursementPaidDto dto)
        {
            await RewardDisbursementShared.EnsureAdminAsync(visibility);

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(RewardDisbursement), id.ToString());
            if (entity.Status != DisbursementStatus.Pending)
                throw new ValidationException(nameof(id), $"Only a pending disbursement can be paid (current: {entity.Status}).");

            entity.MarkPaid(DateTime.UtcNow.Date, string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim());
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Marked RewardDisbursement {Id} paid", id);
        }
    }

    public class ExportRewardDisbursements(
        IRepository<RewardDisbursement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<EmployeeRecognition> recognitionRepository,
        IPerformanceVisibilityService visibility) : IExportRewardDisbursements
    {
        public async Task<string> ExportCsvAsync(GetAllRequest request)
        {
            await RewardDisbursementShared.EnsureAdminAsync(visibility);

            var query = RewardDisbursementShared.Filter(
                repository.GetAll().AsNoTracking(), request, employeeRepository.GetAll());
            var rows = await RewardDisbursementShared.Project(
                    query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt).Take(5000),
                    employeeRepository.GetAll(), badgeRepository.GetAll(), recognitionRepository.GetAll())
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("EmployeeNumber,EmployeeName,Award,Amount,Status,GrantedOn,PaidAt,Reference");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(r.EmployeeNumber), Csv(r.EmployeeName), Csv(r.BadgeName),
                    r.Amount.ToString("0.##"), r.Status,
                    r.GrantedOn?.ToString("yyyy-MM-dd") ?? "", r.PaidAt?.ToString("yyyy-MM-dd") ?? "",
                    Csv(r.Reference)));
            }
            return sb.ToString();
        }

        private static string Csv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Contains(',') || value.Contains('"')
                ? "\"" + value.Replace("\"", "\"\"") + "\""
                : value;
        }
    }
}
