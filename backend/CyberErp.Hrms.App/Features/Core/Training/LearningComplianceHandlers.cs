using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    public class LearningAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public Guid? AudienceId { get; set; }
        /// <summary>The unit / position class / branch by name, so the grid reads without a lookup.</summary>
        public string? AudienceName { get; set; }
        public bool IncludeSubUnits { get; set; }
        public int DueWithinDays { get; set; }
        public DateTime? FixedDueOn { get; set; }
        public int? RecurrenceMonths { get; set; }
        /// <summary>Whether a completion also needs a verifier's signature (logic §12.90).</summary>
        public bool RequiresVerification { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        /// <summary>How many people it currently covers, and how many are compliant.</summary>
        public int ObligationCount { get; set; }
        public int CompliantCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class SaveLearningAssignmentDto
    {
        public Guid? Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Audience { get; set; } = nameof(AssignmentAudience.Everyone);
        public Guid? AudienceId { get; set; }
        public bool IncludeSubUnits { get; set; }
        public int DueWithinDays { get; set; } = 30;
        public DateTime? FixedDueOn { get; set; }
        public int? RecurrenceMonths { get; set; }
        public bool RequiresVerification { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    /// <summary>One person's outstanding or historic obligation, as HR and the learner both see it.</summary>
    public class ObligationDto
    {
        public Guid Id { get; set; }
        public Guid LearningAssignmentId { get; set; }
        public string AssignmentName { get; set; } = string.Empty;
        public Guid TrainingCourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? UnitName { get; set; }
        public int CycleNumber { get; set; }
        public DateTime AssignedOn { get; set; }
        public DateTime DueOn { get; set; }
        public string Status { get; set; } = string.Empty;
        /// <summary>Derived, never stored — see <see cref="ObligationStatus"/>.</summary>
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
        public DateTime? CompletedOn { get; set; }
        /// <summary>The enrolment that satisfied it — the record a signature attaches to.</summary>
        public Guid? TrainingEnrollmentId { get; set; }
        public string? WaivedReason { get; set; }
    }

    /// <summary>One row of the compliance dashboard — by course, or by organizational unit.</summary>
    public class ComplianceRowDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }
        public int Waived { get; set; }
        /// <summary>
        /// Completed as a share of everything that still counts. Waived rows leave the denominator —
        /// an excused obligation is neither a pass nor a failure, and counting it either way would
        /// make the figure argue with itself.
        /// </summary>
        public decimal CompliancePercent { get; set; }
    }

    public class ComplianceOverviewDto
    {
        public int TotalObligations { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public int DueSoon { get; set; }
        public decimal CompliancePercent { get; set; }
        public List<ComplianceRowDto> ByCourse { get; set; } = [];
        public List<ComplianceRowDto> ByUnit { get; set; } = [];
    }

    /// <summary>
    /// Training effectiveness, as far as the data honestly reaches.
    ///
    /// <para>Kirkpatrick levels 1 and 2 are measured; level 3 is a signal, not a measurement. Each
    /// figure carries the sample it came from so a number built on three people cannot be read as if
    /// it were built on three hundred.</para>
    /// </summary>
    public class EffectivenessRowDto
    {
        public Guid TrainingCourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Completions { get; set; }
        /// <summary>Level 1 — the participant's own 1–5 rating.</summary>
        public decimal? AverageFeedback { get; set; }
        public int FeedbackResponses { get; set; }
        /// <summary>Level 2 — the measured quiz score phase 4 made real.</summary>
        public decimal? AverageAssessmentScore { get; set; }
        public int AssessmentResults { get; set; }
        public decimal? FirstAttemptPassRate { get; set; }
        /// <summary>
        /// Level 3 signal — the average change in appraisal score on the competencies this course
        /// develops, comparing the last appraisal before completion with the first one after.
        /// </summary>
        public decimal? CompetencyMovement { get; set; }
        public int CompetencyPairs { get; set; }
        /// <summary>Says plainly why a figure is missing, instead of showing a confident zero.</summary>
        public string? Caveat { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetLearningAssignments { Task<PaginatedResponse<LearningAssignmentDto>> GetAsync(GetAllRequest request); }
    public interface IGetLearningAssignment { Task<LearningAssignmentDto> GetAsync(Guid id); }
    public interface ISaveLearningAssignment { Task<Guid> SaveAsync(SaveLearningAssignmentDto dto); }
    public interface IDeleteLearningAssignment { Task DeleteAsync(Guid id); }
    public interface IGetObligations { Task<PaginatedResponse<ObligationDto>> GetAsync(GetAllRequest request); }
    public interface IWaiveObligation { Task WaiveAsync(Guid id, string reason); }
    public interface IGetComplianceOverview { Task<ComplianceOverviewDto> GetAsync(Guid? trainingCourseId); }
    public interface IGetMyObligations { Task<List<ObligationDto>> GetAsync(); }
    public interface IGetTrainingEffectiveness { Task<List<EffectivenessRowDto>> GetAsync(Guid? trainingCourseId); }

    // ---- Shared -------------------------------------------------------------

    internal static class ComplianceShared
    {
        internal static AssignmentAudience ParseAudience(string audience) =>
            Enum.TryParse<AssignmentAudience>(audience, true, out var a)
                ? a
                : throw new ValidationException("audience", $"'{audience}' is not an audience type.");

        internal static decimal Percent(int completed, int denominator) =>
            denominator <= 0 ? 0m : Math.Round(completed * 100m / denominator, 1);

        internal static void Guard(Action action, string field)
        {
            try { action(); }
            catch (ArgumentException ex) { throw new ValidationException(field, ex.Message); }
            catch (InvalidOperationException ex) { throw new ValidationException(field, ex.Message); }
        }
    }

    // ---- Assignments --------------------------------------------------------

    public class GetLearningAssignments(
        IRepository<LearningAssignment> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<AssignmentObligation> obligationRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<Branch> branchRepository) : IGetLearningAssignments
    {
        public async Task<PaginatedResponse<LearningAssignmentDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(a => a.Name.Contains(request.SearchText.Trim()));
            if (request.CourseId.HasValue)
                query = query.Where(a => a.TrainingCourseId == request.CourseId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(a => a.IsActive == active);

            var total = await query.CountAsync();
            var rows = await query.OrderBy(a => a.Name).Skip(skip).Take(take).ToListAsync();

            var dtos = rows.Select(a => new LearningAssignmentDto
            {
                Id = a.Id,
                TrainingCourseId = a.TrainingCourseId,
                Name = a.Name,
                Audience = a.Audience.ToString(),
                AudienceId = a.AudienceId,
                IncludeSubUnits = a.IncludeSubUnits,
                DueWithinDays = a.DueWithinDays,
                FixedDueOn = a.FixedDueOn,
                RecurrenceMonths = a.RecurrenceMonths,
                RequiresVerification = a.RequiresVerification,
                IsActive = a.IsActive,
                Notes = a.Notes
            }).ToList();

            await AudienceNaming.FillAsync(dtos, courseRepository, unitRepository,
                positionClassRepository, branchRepository);

            // Counted in one grouped query rather than per row — the grid's whole job is to show
            // which rules are being met.
            var ids = dtos.Select(d => d.Id).ToList();
            var today = DateTime.UtcNow.Date;
            var counts = await obligationRepository.GetAll().AsNoTracking()
                .Where(o => ids.Contains(o.LearningAssignmentId))
                .GroupBy(o => o.LearningAssignmentId)
                .Select(g => new
                {
                    g.Key,
                    Total = g.Count(),
                    Completed = g.Count(x => x.Status == ObligationStatus.Completed),
                    Overdue = g.Count(x => x.Status == ObligationStatus.Pending && x.DueOn < today)
                })
                .ToListAsync();

            foreach (var dto in dtos)
            {
                var c = counts.FirstOrDefault(x => x.Key == dto.Id);
                dto.ObligationCount = c?.Total ?? 0;
                dto.CompliantCount = c?.Completed ?? 0;
                dto.OverdueCount = c?.Overdue ?? 0;
            }

            return new PaginatedResponse<LearningAssignmentDto> { Total = total, Data = dtos };
        }
    }

    /// <summary>
    /// Resolves the audience id to a name.
    ///
    /// <para>One query per audience KIND rather than per row: the id points at a different table
    /// depending on the kind, which is exactly why it cannot be a foreign key or a join.</para>
    /// </summary>
    internal static class AudienceNaming
    {
        internal static async Task FillAsync(
            List<LearningAssignmentDto> dtos,
            IRepository<TrainingCourse> courses,
            IRepository<OrganizationUnit> units,
            IRepository<PositionClass> positionClasses,
            IRepository<Branch> branches)
        {
            if (dtos.Count == 0) return;

            var courseIds = dtos.Select(d => d.TrainingCourseId).Distinct().ToList();
            var courseNames = await courses.GetAll().AsNoTracking()
                .Where(c => courseIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
            foreach (var d in dtos)
                d.CourseName = courseNames.TryGetValue(d.TrainingCourseId, out var n) ? n : null;

            var unitIds = Ids(dtos, nameof(AssignmentAudience.OrganizationUnit));
            if (unitIds.Count > 0)
            {
                var names = await units.GetAll().AsNoTracking()
                    .Where(u => unitIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Name);
                Apply(dtos, nameof(AssignmentAudience.OrganizationUnit), names);
            }

            var classIds = Ids(dtos, nameof(AssignmentAudience.PositionClassAudience));
            if (classIds.Count > 0)
            {
                var names = await positionClasses.GetAll().AsNoTracking()
                    .Where(p => classIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Title);
                Apply(dtos, nameof(AssignmentAudience.PositionClassAudience), names);
            }

            var branchIds = Ids(dtos, nameof(AssignmentAudience.Branch));
            if (branchIds.Count > 0)
            {
                var names = await branches.GetAll().AsNoTracking()
                    .Where(b => branchIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.Name);
                Apply(dtos, nameof(AssignmentAudience.Branch), names);
            }

            foreach (var d in dtos.Where(x => x.Audience == nameof(AssignmentAudience.Everyone)))
                d.AudienceName = "Everyone";
        }

        private static List<Guid> Ids(List<LearningAssignmentDto> dtos, string audience) =>
            [.. dtos.Where(d => d.Audience == audience && d.AudienceId.HasValue)
                    .Select(d => d.AudienceId!.Value).Distinct()];

        private static void Apply(List<LearningAssignmentDto> dtos, string audience, Dictionary<Guid, string> names)
        {
            foreach (var d in dtos.Where(x => x.Audience == audience && x.AudienceId.HasValue))
                d.AudienceName = names.TryGetValue(d.AudienceId!.Value, out var n) ? n : null;
        }
    }

    public class GetLearningAssignment(
        IRepository<LearningAssignment> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<Branch> branchRepository) : IGetLearningAssignment
    {
        public async Task<LearningAssignmentDto> GetAsync(Guid id)
        {
            var a = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(LearningAssignment), id.ToString());

            var dto = new LearningAssignmentDto
            {
                Id = a.Id,
                TrainingCourseId = a.TrainingCourseId,
                Name = a.Name,
                Audience = a.Audience.ToString(),
                AudienceId = a.AudienceId,
                IncludeSubUnits = a.IncludeSubUnits,
                DueWithinDays = a.DueWithinDays,
                FixedDueOn = a.FixedDueOn,
                RecurrenceMonths = a.RecurrenceMonths,
                RequiresVerification = a.RequiresVerification,
                IsActive = a.IsActive,
                Notes = a.Notes
            };
            await AudienceNaming.FillAsync([dto], courseRepository, unitRepository,
                positionClassRepository, branchRepository);
            return dto;
        }
    }

    public class SaveLearningAssignment(
        IRepository<LearningAssignment> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<Branch> branchRepository,
        ILogger<SaveLearningAssignment> logger) : ISaveLearningAssignment
    {
        public async Task<Guid> SaveAsync(SaveLearningAssignmentDto dto)
        {
            if (!await courseRepository.GetAll().AnyAsync(c => c.Id == dto.TrainingCourseId))
                throw new NotFoundException(nameof(TrainingCourse), dto.TrainingCourseId.ToString());

            var audience = ComplianceShared.ParseAudience(dto.Audience);
            await EnsureAudienceExistsAsync(audience, dto.AudienceId);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var existing = await repository.GetAll().FirstOrDefaultAsync(a => a.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LearningAssignment), dto.Id.Value.ToString());

                ComplianceShared.Guard(() => existing.Apply(dto.Name, audience, dto.AudienceId,
                    dto.IncludeSubUnits, dto.DueWithinDays, dto.FixedDueOn, dto.RecurrenceMonths,
                    dto.RequiresVerification, dto.IsActive, dto.Notes), "name");
                repository.UpdateAsync(existing);
                await repository.SaveChangesAsync();
                return existing.Id;
            }

            LearningAssignment created = null!;
            ComplianceShared.Guard(() => created = LearningAssignment.Create(dto.TrainingCourseId,
                dto.Name, audience, dto.AudienceId, dto.IncludeSubUnits, dto.DueWithinDays,
                dto.FixedDueOn, dto.RecurrenceMonths, dto.RequiresVerification, dto.IsActive,
                dto.Notes), "name");
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Learning assignment '{Name}' created for course {CourseId} ({Audience})",
                created.Name, created.TrainingCourseId, created.Audience);
            return created.Id;
        }

        /// <summary>
        /// AudienceId has no foreign key — it points at a different table per audience kind — so the
        /// reference is checked here. A rule aimed at a deleted unit would silently cover nobody, and
        /// the screen would look correctly configured.
        /// </summary>
        private async Task EnsureAudienceExistsAsync(AssignmentAudience audience, Guid? audienceId)
        {
            if (audience == AssignmentAudience.Everyone || audienceId is null) return;

            var exists = audience switch
            {
                AssignmentAudience.OrganizationUnit =>
                    await unitRepository.GetAll().AnyAsync(u => u.Id == audienceId.Value),
                AssignmentAudience.PositionClassAudience =>
                    await positionClassRepository.GetAll().AnyAsync(p => p.Id == audienceId.Value),
                AssignmentAudience.Branch =>
                    await branchRepository.GetAll().AnyAsync(b => b.Id == audienceId.Value),
                _ => true
            };

            if (!exists)
                throw new ValidationException("audienceId", $"That {audience} no longer exists.");
        }
    }

    public class DeleteLearningAssignment(
        IRepository<LearningAssignment> repository,
        IRepository<AssignmentObligation> obligationRepository,
        ILogger<DeleteLearningAssignment> logger) : IDeleteLearningAssignment
    {
        public async Task DeleteAsync(Guid id)
        {
            var assignment = await repository.GetAll().FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException(nameof(LearningAssignment), id.ToString());

            // ⚠️ Deleting a rule with a compliance history destroys the record of who was required to
            // do what — the thing an audit asks for. Deactivating stops it chasing anyone new while
            // keeping the evidence, so that is what the user is pointed at.
            var completed = await obligationRepository.GetAll().AsNoTracking()
                .CountAsync(o => o.LearningAssignmentId == id && o.Status != ObligationStatus.Pending);
            if (completed > 0)
                throw new ValidationException("id",
                    $"This assignment has {completed} completed or waived record(s) and cannot be deleted. Deactivate it instead — that stops it applying to anyone new and keeps the history.");

            var pending = await obligationRepository.GetAll()
                .Where(o => o.LearningAssignmentId == id).ToListAsync();
            foreach (var o in pending) obligationRepository.Delete(o);

            repository.Delete(assignment);
            await repository.SaveChangesAsync();
            logger.LogInformation("Learning assignment {Id} deleted with {Count} outstanding obligation(s)",
                id, pending.Count);
        }
    }

    // ---- Obligations --------------------------------------------------------

    internal static class ObligationMapping
    {
        internal static async Task<List<ObligationDto>> MapAsync(
            List<AssignmentObligation> rows,
            IRepository<LearningAssignment> assignments,
            IRepository<TrainingCourse> courses,
            IRepository<Employee> employees,
            IRepository<Position> positions,
            IRepository<OrganizationUnit> units,
            bool includeEmployee)
        {
            if (rows.Count == 0) return [];
            var today = DateTime.UtcNow.Date;

            var assignmentIds = rows.Select(r => r.LearningAssignmentId).Distinct().ToList();
            var assignmentInfo = await assignments.GetAll().AsNoTracking()
                .Where(a => assignmentIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Name, a.TrainingCourseId })
                .ToListAsync();
            var courseIds = assignmentInfo.Select(a => a.TrainingCourseId).Distinct().ToList();
            var courseNames = await courses.GetAll().AsNoTracking()
                .Where(c => courseIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

            var dtos = rows.Select(o =>
            {
                var a = assignmentInfo.FirstOrDefault(x => x.Id == o.LearningAssignmentId);
                return new ObligationDto
                {
                    Id = o.Id,
                    LearningAssignmentId = o.LearningAssignmentId,
                    AssignmentName = a?.Name ?? "",
                    TrainingCourseId = a?.TrainingCourseId ?? Guid.Empty,
                    CourseName = a is not null && courseNames.TryGetValue(a.TrainingCourseId, out var n) ? n : "",
                    EmployeeId = o.EmployeeId,
                    CycleNumber = o.CycleNumber,
                    AssignedOn = o.AssignedOn,
                    DueOn = o.DueOn,
                    Status = o.Status.ToString(),
                    IsOverdue = o.IsOverdueOn(today),
                    DaysRemaining = (o.DueOn.Date - today).Days,
                    CompletedOn = o.CompletedOn,
                    TrainingEnrollmentId = o.TrainingEnrollmentId,
                    WaivedReason = o.WaivedReason
                };
            }).ToList();

            if (!includeEmployee) return dtos;

            // Names and units in two batched reads — the HR grid is the only caller that needs them,
            // and the learner's own list would gain nothing from either.
            var employeeIds = dtos.Select(d => d.EmployeeId).Distinct().ToList();
            var people = await employees.GetAll().AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeNumber,
                    e.PositionId,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .ToListAsync();

            var positionIds = people.Where(p => p.PositionId.HasValue).Select(p => p.PositionId!.Value).Distinct().ToList();
            var unitOfPosition = await positions.GetAll().AsNoTracking()
                .Where(p => positionIds.Contains(p.Id))
                .Select(p => new { p.Id, p.OrganizationUnitId })
                .ToListAsync();
            var unitIds = unitOfPosition.Select(u => u.OrganizationUnitId).Distinct().ToList();
            var unitNames = await units.GetAll().AsNoTracking()
                .Where(u => unitIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Name);

            foreach (var d in dtos)
            {
                var p = people.FirstOrDefault(x => x.Id == d.EmployeeId);
                d.EmployeeName = p?.Name;
                d.EmployeeNumber = p?.EmployeeNumber;
                var unitId = p?.PositionId is null
                    ? (Guid?)null
                    : unitOfPosition.FirstOrDefault(u => u.Id == p.PositionId.Value)?.OrganizationUnitId;
                d.UnitName = unitId.HasValue && unitNames.TryGetValue(unitId.Value, out var un) ? un : null;
            }
            return dtos;
        }
    }

    public class GetObligations(
        IRepository<AssignmentObligation> repository,
        IRepository<LearningAssignment> assignmentRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> unitRepository) : IGetObligations
    {
        public async Task<PaginatedResponse<ObligationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var today = DateTime.UtcNow.Date;

            var query = repository.GetAll().AsNoTracking();
            if (request.ItemId.HasValue)
                query = query.Where(o => o.LearningAssignmentId == request.ItemId.Value);
            if (request.EmployeeId.HasValue)
                query = query.Where(o => o.EmployeeId == request.EmployeeId.Value);

            // "Overdue" is a status the user asks for but not one the column holds, so it is
            // translated here rather than stored — see ObligationStatus.
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (request.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.Status == ObligationStatus.Pending && o.DueOn < today);
                else if (Enum.TryParse<ObligationStatus>(request.Status, true, out var status))
                    query = query.Where(o => o.Status == status);
            }

            var total = await query.CountAsync();
            var rows = await query.OrderBy(o => o.DueOn).Skip(skip).Take(take).ToListAsync();

            var dtos = await ObligationMapping.MapAsync(rows, assignmentRepository, courseRepository,
                employeeRepository, positionRepository, unitRepository, includeEmployee: true);

            return new PaginatedResponse<ObligationDto> { Total = total, Data = dtos };
        }
    }

    public class WaiveObligation(
        IRepository<AssignmentObligation> repository,
        IPerformanceVisibilityService visibility,
        ILogger<WaiveObligation> logger) : IWaiveObligation
    {
        public async Task WaiveAsync(Guid id, string reason)
        {
            // Waiving is how a compliance record shows someone as excused, so it is an HR act and
            // never a self-service one.
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                throw new ValidationException("access", "Only HR can waive a training obligation.");

            var obligation = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(AssignmentObligation), id.ToString());

            ComplianceShared.Guard(() => obligation.Waive(reason), "reason");
            repository.UpdateAsync(obligation);
            await repository.SaveChangesAsync();
            logger.LogInformation("Obligation {Id} waived: {Reason}", id, reason);
        }
    }

    /// <summary>The signed-in learner's own obligations — what is required of them, and by when.</summary>
    public class GetMyObligations(
        IRepository<AssignmentObligation> repository,
        IRepository<LearningAssignment> assignmentRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetMyObligations
    {
        public async Task<List<ObligationDto>> GetAsync()
        {
            var myEmployeeId = await PlayerAccess.MyEmployeeIdAsync(userRepository, currentUser.GetCurrentUserId());
            // An unlinked account has no obligations rather than an error: plenty of admin logins
            // are not employees, and a red banner on their dashboard would be wrong.
            if (myEmployeeId is null) return [];

            var rows = await repository.GetAll().AsNoTracking()
                .Where(o => o.EmployeeId == myEmployeeId.Value)
                .OrderBy(o => o.Status == ObligationStatus.Pending ? 0 : 1)
                .ThenBy(o => o.DueOn)
                .Take(200)
                .ToListAsync();

            return await ObligationMapping.MapAsync(rows, assignmentRepository, courseRepository,
                employeeRepository, positionRepository, unitRepository, includeEmployee: false);
        }
    }
}
