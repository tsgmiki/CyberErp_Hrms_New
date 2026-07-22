using System.Text;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingProviderPaymentDto
    {
        public Guid Id { get; set; }
        public Guid? TrainingSessionId { get; set; }
        public string? CourseName { get; set; }
        public DateTime? SessionStartDate { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }

    public class MarkProviderPaymentPaidDto
    {
        public string? Reference { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetAllProviderPayments { Task<PaginatedResponse<TrainingProviderPaymentDto>> GetAsync(GetAllRequest request); }
    public interface IMarkProviderPaymentPaid { Task PayAsync(Guid id, MarkProviderPaymentPaidDto dto); }
    /// <summary>HC202 — CSV hand-off for finance (no finance module exists).</summary>
    public interface IExportProviderPayments { Task<string> ExportCsvAsync(GetAllRequest request); }

    internal static class ProviderPaymentShared
    {
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR/finance administrators can manage provider payments.");
        }

        internal static IQueryable<TrainingProviderPaymentDto> Project(
            IQueryable<TrainingProviderPayment> query,
            IQueryable<TrainingSession> sessions,
            IQueryable<TrainingCourse> courses)
        {
            return query.Select(x => new TrainingProviderPaymentDto
            {
                Id = x.Id,
                TrainingSessionId = x.TrainingSessionId,
                CourseName = sessions.Where(s => s.Id == x.TrainingSessionId)
                    .Join(courses, s => s.TrainingCourseId, c => c.Id, (s, c) => c.Name).FirstOrDefault(),
                SessionStartDate = sessions.Where(s => s.Id == x.TrainingSessionId)
                    .Select(s => (DateTime?)s.StartDate).FirstOrDefault(),
                ProviderName = x.ProviderName,
                Amount = x.Amount,
                Status = x.Status.ToString(),
                PaidAt = x.PaidAt,
                Reference = x.Reference,
                Notes = x.Notes
            });
        }

        internal static IQueryable<TrainingProviderPayment> Filter(
            IQueryable<TrainingProviderPayment> query, GetAllRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ProviderPaymentStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.ProviderName.Contains(term));
            }
            return query;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class GetAllProviderPayments(
        IRepository<TrainingProviderPayment> repository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility) : IGetAllProviderPayments
    {
        public async Task<PaginatedResponse<TrainingProviderPaymentDto>> GetAsync(GetAllRequest request)
        {
            await ProviderPaymentShared.EnsureAdminAsync(visibility);

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = ProviderPaymentShared.Filter(repository.GetAll().AsNoTracking(), request);
            var total = await query.CountAsync();
            var data = await ProviderPaymentShared.Project(
                    query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt).Skip(skip).Take(take),
                    sessionRepository.GetAll(), courseRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<TrainingProviderPaymentDto> { Total = total, Data = data };
        }
    }

    public class MarkProviderPaymentPaid(
        IRepository<TrainingProviderPayment> repository,
        IPerformanceVisibilityService visibility,
        ILogger<MarkProviderPaymentPaid> logger) : IMarkProviderPaymentPaid
    {
        public async Task PayAsync(Guid id, MarkProviderPaymentPaidDto dto)
        {
            await ProviderPaymentShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingProviderPayment), id.ToString());
            if (entity.Status != ProviderPaymentStatus.Pending)
                throw new ValidationException(nameof(id), $"Only a pending payment can be paid (current: {entity.Status}).");
            entity.MarkPaid(DateTime.UtcNow.Date, string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim());
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Marked provider payment {Id} paid", id);
        }
    }

    public class ExportProviderPayments(
        IRepository<TrainingProviderPayment> repository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility) : IExportProviderPayments
    {
        public async Task<string> ExportCsvAsync(GetAllRequest request)
        {
            await ProviderPaymentShared.EnsureAdminAsync(visibility);

            var rows = await ProviderPaymentShared.Project(
                    ProviderPaymentShared.Filter(repository.GetAll().AsNoTracking(), request)
                        .OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt).Take(5000),
                    sessionRepository.GetAll(), courseRepository.GetAll())
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Provider,Course,SessionDate,Amount,Status,PaidAt,Reference");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(r.ProviderName), Csv(r.CourseName),
                    r.SessionStartDate?.ToString("yyyy-MM-dd") ?? "",
                    r.Amount.ToString("0.##"), r.Status,
                    r.PaidAt?.ToString("yyyy-MM-dd") ?? "", Csv(r.Reference)));
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
