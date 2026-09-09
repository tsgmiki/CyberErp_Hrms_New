using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    /// <summary>One session a learner could join, with the state the enrol button depends on.</summary>
    public class CatalogSessionDto
    {
        public Guid TrainingSessionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Venue { get; set; }
        public string? MeetingUrl { get; set; }
        public string? TrainerName { get; set; }
        /// <summary>Null when the session is uncapped.</summary>
        public int? MaxParticipants { get; set; }
        public int SeatsTaken { get; set; }
        /// <summary>Null when uncapped — the UI shows "open" rather than a number.</summary>
        public int? SeatsLeft { get; set; }
        public bool IsFull { get; set; }
        /// <summary>True when the caller already holds a live seat on this session.</summary>
        public bool IsEnrolled { get; set; }
        /// <summary>False with a reason when the enrol button must be disabled.</summary>
        public bool CanEnroll { get; set; }
        public string? BlockedReason { get; set; }
    }

    /// <summary>A course as a learner browsing the catalogue sees it.</summary>
    public class CatalogCourseDto
    {
        public Guid TrainingCourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? Objectives { get; set; }
        public string? TargetAudience { get; set; }
        public string? Prerequisites { get; set; }
        public decimal? DurationHours { get; set; }
        public decimal CpdHours { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public bool IsExternal { get; set; }
        public string? ProviderName { get; set; }
        public string? ExternalUrl { get; set; }
        /// <summary>What the course develops — from the phase-1 mapping. Primary first.</summary>
        public List<string> Competencies { get; set; } = [];
        /// <summary>True when one of these competencies is a gap on the caller's latest appraisal.</summary>
        public bool IsRecommended { get; set; }
        public string? RecommendedBecause { get; set; }
        public List<CatalogSessionDto> Sessions { get; set; } = [];
    }

    public interface IGetTrainingCatalog { Task<List<CatalogCourseDto>> GetAsync(); }

    // ---- Handler ------------------------------------------------------------

    /// <summary>
    /// The course catalogue as a LEARNER sees it: active courses, the sessions they could still join,
    /// what each course develops, and whether it addresses one of their own competency gaps.
    ///
    /// <para>⚠️ A separate read from the admin <c>TrainingCourse</c> endpoints on purpose. Those are
    /// gated on <c>trainingCourse</c>, which ordinary staff do not hold — and adding <c>myTraining</c>
    /// to that controller would hand every employee the catalogue's CREATE and DELETE as well, since
    /// UserRole carries Add on <c>myTraining</c>. This controller is read-only by construction, so the
    /// permission it needs cannot be turned into write access (logic §12.85).</para>
    ///
    /// <para>⚠️ The per-session state mirrors <c>EnrollTraining</c> rule for rule — scheduled only,
    /// no double seat, capacity counted with withdrawn seats freed. A catalogue that offers what the
    /// enrol endpoint then refuses is worse than one that offers nothing.</para>
    /// </summary>
    public class GetTrainingCatalog(
        IRepository<TrainingCourse> courseRepository,
        IRepository<TrainingCategory> categoryRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<CourseCompetency> courseCompetencyRepository,
        IRepository<Competency> competencyRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<AppraisalCompetency> appraisalCompetencyRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetTrainingCatalog
    {
        /// <summary>Same development threshold the suggestion engine uses — one definition of "a gap".</summary>
        private const decimal LowScorePercent = 60m;

        public async Task<List<CatalogCourseDto>> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            var myEmployeeId = userId is null ? null : await userRepository.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();

            var courses = await courseRepository.GetAll().AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id, c.Name, c.Code, c.TrainingCategoryId, c.Description, c.Objectives,
                    c.TargetAudience, c.Prerequisites, c.DurationHours, c.CpdHours,
                    c.DeliveryMode, c.IsExternal, c.ProviderName, c.ExternalUrl
                })
                .ToListAsync();
            if (courses.Count == 0) return [];

            var courseIds = courses.Select(c => c.Id).ToList();

            var categoryIds = courses.Where(c => c.TrainingCategoryId.HasValue)
                .Select(c => c.TrainingCategoryId!.Value).Distinct().ToList();
            var categories = categoryIds.Count == 0
                ? []
                : await categoryRepository.GetAll().AsNoTracking()
                    .Where(x => categoryIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name);

            // Only sessions still worth showing: scheduled, and not already started.
            var today = DateTime.UtcNow.Date;
            var sessions = await sessionRepository.GetAll().AsNoTracking()
                .Where(s => courseIds.Contains(s.TrainingCourseId)
                    && s.Status == TrainingSessionStatus.Scheduled
                    && s.StartDate >= today)
                .Select(s => new
                {
                    s.Id, s.TrainingCourseId, s.StartDate, s.EndDate,
                    s.Venue, s.MeetingUrl, s.TrainerName, s.MaxParticipants
                })
                .ToListAsync();

            var sessionIds = sessions.Select(s => s.Id).ToList();
            // Seat counts and my own seats in one pass each — withdrawn rows free their seat, which
            // is exactly how EnrollTraining counts them.
            var liveSeats = sessionIds.Count == 0
                ? []
                : await enrollmentRepository.GetAll().AsNoTracking()
                    .Where(e => sessionIds.Contains(e.TrainingSessionId)
                        && e.Status != TrainingEnrollmentStatus.Withdrawn)
                    .Select(e => new { e.TrainingSessionId, e.EmployeeId })
                    .ToListAsync();

            var takenBySession = liveSeats.GroupBy(x => x.TrainingSessionId)
                .ToDictionary(g => g.Key, g => g.Count());
            var mySessionIds = myEmployeeId is null
                ? []
                : liveSeats.Where(x => x.EmployeeId == myEmployeeId.Value)
                    .Select(x => x.TrainingSessionId).ToHashSet();

            // What each course develops (phase 1), primary first.
            var mappings = await courseCompetencyRepository.GetAll().AsNoTracking()
                .Where(m => courseIds.Contains(m.TrainingCourseId))
                .Select(m => new { m.TrainingCourseId, m.CompetencyId, m.IsPrimary })
                .ToListAsync();
            var competencyIds = mappings.Select(m => m.CompetencyId).Distinct().ToList();
            var competencyNames = competencyIds.Count == 0
                ? []
                : await competencyRepository.GetAll().AsNoTracking()
                    .Where(c => competencyIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, c => c.Name);

            var myGaps = await MyGapCompetencyIdsAsync(myEmployeeId);

            var result = new List<CatalogCourseDto>();
            foreach (var c in courses)
            {
                var mine = mappings.Where(m => m.TrainingCourseId == c.Id)
                    .OrderByDescending(m => m.IsPrimary)
                    .ToList();

                var gapHit = mine.FirstOrDefault(m => myGaps.Contains(m.CompetencyId));

                var dto = new CatalogCourseDto
                {
                    TrainingCourseId = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CategoryName = c.TrainingCategoryId.HasValue
                        ? categories.GetValueOrDefault(c.TrainingCategoryId.Value) : null,
                    Description = c.Description,
                    Objectives = c.Objectives,
                    TargetAudience = c.TargetAudience,
                    Prerequisites = c.Prerequisites,
                    DurationHours = c.DurationHours,
                    CpdHours = c.CpdHours,
                    DeliveryMode = c.DeliveryMode.ToString(),
                    IsExternal = c.IsExternal,
                    ProviderName = c.ProviderName,
                    ExternalUrl = c.ExternalUrl,
                    Competencies = [.. mine
                        .Select(m => competencyNames.GetValueOrDefault(m.CompetencyId))
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Select(n => n!)],
                    IsRecommended = gapHit is not null,
                    RecommendedBecause = gapHit is null
                        ? null
                        : $"Develops {competencyNames.GetValueOrDefault(gapHit.CompetencyId)}, "
                          + "rated below the development threshold on your latest appraisal.",
                    Sessions = [.. sessions
                        .Where(s => s.TrainingCourseId == c.Id)
                        .OrderBy(s => s.StartDate)
                        .Select(s =>
                        {
                            var taken = takenBySession.GetValueOrDefault(s.Id);
                            var full = s.MaxParticipants.HasValue && taken >= s.MaxParticipants.Value;
                            var enrolled = mySessionIds.Contains(s.Id);
                            return new CatalogSessionDto
                            {
                                TrainingSessionId = s.Id,
                                StartDate = s.StartDate,
                                EndDate = s.EndDate,
                                Venue = s.Venue,
                                MeetingUrl = s.MeetingUrl,
                                TrainerName = s.TrainerName,
                                MaxParticipants = s.MaxParticipants,
                                SeatsTaken = taken,
                                SeatsLeft = s.MaxParticipants.HasValue
                                    ? Math.Max(0, s.MaxParticipants.Value - taken) : null,
                                IsFull = full,
                                IsEnrolled = enrolled,
                                // The reason is shown INSTEAD of a dead button, so a learner is never
                                // left guessing why they cannot join.
                                CanEnroll = myEmployeeId is not null && !enrolled && !full,
                                BlockedReason =
                                    myEmployeeId is null ? "Your account is not linked to an employee record."
                                    : enrolled ? "You are already enrolled."
                                    : full ? $"Full ({taken}/{s.MaxParticipants})."
                                    : null
                            };
                        })]
                };
                result.Add(dto);
            }

            // Courses that close one of the learner's own gaps come first, then those they can act on
            // now, then the rest alphabetically.
            return [.. result
                .OrderByDescending(x => x.IsRecommended)
                .ThenByDescending(x => x.Sessions.Any(s => s.CanEnroll))
                .ThenBy(x => x.Name)];
        }

        /// <summary>
        /// The competencies the caller was rated below the development threshold on, from their most
        /// recent scored appraisal. Empty when they have none — the catalogue then simply shows no
        /// recommendations rather than failing.
        /// </summary>
        private async Task<HashSet<Guid>> MyGapCompetencyIdsAsync(Guid? employeeId)
        {
            if (employeeId is null) return [];

            var latest = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value && a.OverallScore != null)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.Id, a.ReviewCycleId })
                .FirstOrDefaultAsync();
            if (latest is null) return [];

            var cycle = await reviewCycleRepository.GetAll().AsNoTracking()
                .Where(c => c.Id == latest.ReviewCycleId)
                .Select(c => new { c.RatingScaleId })
                .FirstOrDefaultAsync();
            if (cycle is null) return [];

            var max = await ratingLevelRepository.GetAll().AsNoTracking()
                .Where(l => l.RatingScaleId == cycle.RatingScaleId)
                .Select(l => (decimal?)l.Value).MaxAsync() ?? 0m;
            if (max <= 0) return [];

            var threshold = max * LowScorePercent / 100m;
            return (await appraisalCompetencyRepository.GetAll().AsNoTracking()
                .Where(x => x.AppraisalId == latest.Id && x.ManagerScore != null && x.ManagerScore < threshold)
                .Select(x => x.CompetencyId)
                .ToListAsync()).ToHashSet();
        }
    }
}
