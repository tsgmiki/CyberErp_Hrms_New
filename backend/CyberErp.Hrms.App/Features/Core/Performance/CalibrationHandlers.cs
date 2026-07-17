using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class CalibrationItemDto
    {
        public Guid Id { get; set; }
        public Guid AppraisalId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? OriginalScore { get; set; }
        public decimal? CalibratedScore { get; set; }
        public string? Justification { get; set; }
        public bool IsAdjusted { get; set; }
    }

    public class CalibrationSessionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ReviewCycleId { get; set; }
        public string? ReviewCycleName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public List<CalibrationItemDto> Items { get; set; } = [];
    }

    public class CreateCalibrationSessionDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid ReviewCycleId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveCalibrationItemDto
    {
        public Guid ItemId { get; set; }
        public decimal? CalibratedScore { get; set; }
        public string? Justification { get; set; }
    }

    public class CreateCalibrationSessionDtoValidator : AbstractValidator<CreateCalibrationSessionDto>
    {
        public CreateCalibrationSessionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ReviewCycleId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateCalibrationSession { Task<Guid> CreateAsync(CreateCalibrationSessionDto dto); }
    public interface ISaveCalibrationItem { Task SaveAsync(SaveCalibrationItemDto dto); }
    public interface IFinalizeCalibrationSession { Task FinalizeAsync(Guid id); }
    public interface IDeleteCalibrationSession { Task DeleteAsync(Guid id); }
    public interface IGetCalibrationSessionById { Task<CalibrationSessionDto> GetAsync(Guid id); }
    public interface IGetAllCalibrationSessions { Task<PaginatedResponse<CalibrationSessionDto>> GetAsync(GetAllRequest request); }

    // ---- Create (auto-pull the cohort's appraisals) -------------------------
    public class CreateCalibrationSession(
        IRepository<CalibrationSession> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IPerformanceHistoryWriter history,
        IValidator<CreateCalibrationSessionDto> validator,
        ILogger<CreateCalibrationSession> logger) : ICreateCalibrationSession
    {
        public async Task<Guid> CreateAsync(CreateCalibrationSessionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await reviewCycleRepository.GetAll().AnyAsync(c => c.Id == dto.ReviewCycleId))
                throw new NotFoundException(nameof(ReviewCycle), dto.ReviewCycleId.ToString());
            if (dto.OrganizationUnitId.HasValue &&
                !await organizationUnitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.Value.ToString());

            var session = CalibrationSession.Create(dto.Name, dto.ReviewCycleId, dto.OrganizationUnitId, dto.Notes);

            // Cohort = appraisals in the cycle, optionally narrowed to employees on positions in the unit.
            var appraisals = appraisalRepository.GetAll().Where(a => a.ReviewCycleId == dto.ReviewCycleId);
            if (dto.OrganizationUnitId.HasValue)
            {
                var employeeIds = employeeRepository.GetAll()
                    .Where(e => e.PositionId != null &&
                        positionRepository.GetAll().Any(p => p.Id == e.PositionId && p.OrganizationUnitId == dto.OrganizationUnitId.Value))
                    .Select(e => e.Id);
                appraisals = appraisals.Where(a => employeeIds.Contains(a.EmployeeId));
            }
            var cohort = await appraisals.Select(a => new { a.Id, a.EmployeeId, a.OverallScore }).ToListAsync();
            foreach (var a in cohort)
                session.AddItem(a.Id, a.EmployeeId, a.OverallScore);

            await repository.AddAsync(session);
            CalibrationMapper.StampItemTenant(session);
            await history.WriteAsync("Calibration", session.Id, "Created",
                $"Calibration session created with {cohort.Count} appraisal(s).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CalibrationSession {Id} ({Count} items)", session.Id, cohort.Count);
            return session.Id;
        }
    }

    // ---- Adjust an item (documented, HC129) ---------------------------------
    public class SaveCalibrationItem(
        IRepository<CalibrationItem> itemRepository,
        IRepository<CalibrationSession> sessionRepository,
        ILogger<SaveCalibrationItem> logger) : ISaveCalibrationItem
    {
        public async Task SaveAsync(SaveCalibrationItemDto dto)
        {
            var item = await itemRepository.GetAll().FirstOrDefaultAsync(i => i.Id == dto.ItemId)
                ?? throw new NotFoundException(nameof(CalibrationItem), dto.ItemId.ToString());
            var status = await sessionRepository.GetAll().Where(s => s.Id == item.CalibrationSessionId)
                .Select(s => s.Status).FirstOrDefaultAsync();
            if (status != CalibrationStatus.Draft)
                throw new ValidationException(nameof(dto.ItemId), "A finalized calibration session can no longer be modified.");

            item.Calibrate(dto.CalibratedScore, dto.Justification);
            await itemRepository.SaveChangesAsync();
            logger.LogInformation("Calibrated item {Id} → {Score}", dto.ItemId, dto.CalibratedScore);
        }
    }

    // ---- Finalize (apply adjustments back to the appraisals) ----------------
    public class FinalizeCalibrationSession(
        IRepository<CalibrationSession> repository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IPerformanceHistoryWriter history,
        ILogger<FinalizeCalibrationSession> logger) : IFinalizeCalibrationSession
    {
        public async Task FinalizeAsync(Guid id)
        {
            var session = await repository.GetAll().Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException(nameof(CalibrationSession), id.ToString());
            if (session.Status != CalibrationStatus.Draft)
                throw new ValidationException(nameof(id), "This calibration session is already finalized.");

            var applied = 0;
            foreach (var item in session.Items.Where(i => i.IsAdjusted && i.CalibratedScore.HasValue))
            {
                var appraisal = await appraisalRepository.GetAll().FirstOrDefaultAsync(a => a.Id == item.AppraisalId);
                if (appraisal is null) continue;
                var levelId = await ResolveRatingLevelAsync(appraisal.ReviewCycleId, item.CalibratedScore!.Value);
                appraisal.ApplyCalibration(item.CalibratedScore.Value, levelId);
                await history.WriteAsync("Appraisal", appraisal.Id, "Calibrated",
                    $"Score calibrated to {item.CalibratedScore.Value} ({item.Justification ?? "no justification"}).");
                applied++;
            }

            session.Finalize();
            await history.WriteAsync("Calibration", session.Id, "Finalized", $"Finalized; {applied} appraisal(s) adjusted.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Finalized CalibrationSession {Id}; applied {Applied} adjustments", id, applied);
        }

        private async Task<Guid?> ResolveRatingLevelAsync(Guid reviewCycleId, decimal overall)
        {
            var scaleId = await reviewCycleRepository.GetAll().Where(c => c.Id == reviewCycleId)
                .Select(c => c.RatingScaleId).FirstOrDefaultAsync();
            if (scaleId == Guid.Empty) return null;
            var levels = await ratingLevelRepository.GetAll().Where(l => l.RatingScaleId == scaleId).ToListAsync();
            if (levels.Count == 0) return null;
            var band = levels.FirstOrDefault(l => l.MinScore.HasValue && l.MaxScore.HasValue
                && overall >= l.MinScore.Value && overall <= l.MaxScore.Value);
            return band?.Id ?? levels.OrderBy(l => Math.Abs(l.Value - overall)).First().Id;
        }
    }

    // ---- Delete / reads -----------------------------------------------------
    public class DeleteCalibrationSession(
        IRepository<CalibrationSession> repository,
        ILogger<DeleteCalibrationSession> logger) : IDeleteCalibrationSession
    {
        public async Task DeleteAsync(Guid id)
        {
            var session = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CalibrationSession), id.ToString());
            repository.Delete(session);   // items cascade; applied scores stay on the appraisals
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CalibrationSession {Id}", id);
        }
    }

    public class GetCalibrationSessionById(
        IRepository<CalibrationSession> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationUnit> organizationUnitRepository) : IGetCalibrationSessionById
    {
        public async Task<CalibrationSessionDto> GetAsync(Guid id)
        {
            var session = await repository.GetAll().Include(s => s.Items).AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException(nameof(CalibrationSession), id.ToString());
            var cycleName = await reviewCycleRepository.GetAll().Where(c => c.Id == session.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
            var unitName = session.OrganizationUnitId.HasValue
                ? await organizationUnitRepository.GetAll().Where(u => u.Id == session.OrganizationUnitId.Value).Select(u => u.Name).FirstOrDefaultAsync()
                : null;

            var dto = CalibrationMapper.MapHeader(session, cycleName, unitName);
            var employees = employeeRepository.GetAll();
            foreach (var item in session.Items.OrderBy(i => i.EmployeeId))
            {
                var name = await employees.Where(e => e.Id == item.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
                dto.Items.Add(new CalibrationItemDto
                {
                    Id = item.Id,
                    AppraisalId = item.AppraisalId,
                    EmployeeId = item.EmployeeId,
                    EmployeeName = name,
                    OriginalScore = item.OriginalScore,
                    CalibratedScore = item.CalibratedScore,
                    Justification = item.Justification,
                    IsAdjusted = item.IsAdjusted
                });
            }
            return dto;
        }
    }

    public class GetAllCalibrationSessions(
        IRepository<CalibrationSession> repository,
        IRepository<ReviewCycle> reviewCycleRepository) : IGetAllCalibrationSessions
    {
        public async Task<PaginatedResponse<CalibrationSessionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (request.ReviewCycleId.HasValue)
                query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CalibrationStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync();
            var cycles = reviewCycleRepository.GetAll();
            var data = new List<CalibrationSessionDto>(rows.Count);
            foreach (var r in rows)
            {
                var cycleName = await cycles.Where(c => c.Id == r.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
                data.Add(CalibrationMapper.MapHeader(r, cycleName, null));
            }
            return new PaginatedResponse<CalibrationSessionDto> { Total = total, Data = data };
        }
    }

    internal static class CalibrationMapper
    {
        internal static CalibrationSessionDto MapHeader(CalibrationSession s, string? cycleName, string? unitName) => new()
        {
            Id = s.Id,
            Name = s.Name,
            ReviewCycleId = s.ReviewCycleId,
            ReviewCycleName = cycleName,
            OrganizationUnitId = s.OrganizationUnitId,
            OrganizationUnitName = unitName,
            Status = s.Status.ToString(),
            Notes = s.Notes,
            FinalizedAt = s.FinalizedAt
        };

        internal static void StampItemTenant(CalibrationSession s)
        {
            foreach (var item in s.Items)
                if (string.IsNullOrEmpty(item.TenantId)) item.TenantId = s.TenantId;
        }
    }
}
