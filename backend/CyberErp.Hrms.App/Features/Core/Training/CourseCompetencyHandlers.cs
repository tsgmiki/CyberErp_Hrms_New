using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    public class CourseCompetencyDto
    {
        public Guid Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public Guid CompetencyId { get; set; }
        public string CompetencyName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        /// <summary>The course's main subject, rather than something it touches in passing.</summary>
        public bool IsPrimary { get; set; }
    }

    /// <summary>One competency the course develops, as posted from the mapping editor.</summary>
    public class CourseCompetencySpecDto
    {
        public Guid CompetencyId { get; set; }
        public bool IsPrimary { get; set; } = true;
    }

    public class SetCourseCompetenciesDto
    {
        public Guid TrainingCourseId { get; set; }
        public List<CourseCompetencySpecDto> Competencies { get; set; } = [];
    }

    /// <summary>A course offered against a competency gap, with why it was offered.</summary>
    public class RecommendedCourseDto
    {
        public Guid TrainingCourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public decimal? DurationHours { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        /// <summary>Sessions still open to enrol on — a recommendation nobody can act on is noise.</summary>
        public int UpcomingSessions { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetCourseCompetencies { Task<List<CourseCompetencyDto>> GetAsync(Guid trainingCourseId); }
    public interface ISetCourseCompetencies { Task SetAsync(SetCourseCompetenciesDto dto); }

    // ---- Handlers -----------------------------------------------------------

    public class GetCourseCompetencies(
        IRepository<CourseCompetency> repository,
        IRepository<Competency> competencyRepository,
        IRepository<CompetencyCategory> categoryRepository) : IGetCourseCompetencies
    {
        public async Task<List<CourseCompetencyDto>> GetAsync(Guid trainingCourseId)
        {
            var rows = await repository.GetAll().AsNoTracking()
                .Where(x => x.TrainingCourseId == trainingCourseId)
                .Select(x => new { x.Id, x.TrainingCourseId, x.CompetencyId, x.IsPrimary })
                .ToListAsync();
            if (rows.Count == 0) return [];

            var ids = rows.Select(r => r.CompetencyId).ToList();
            var competencies = await competencyRepository.GetAll().AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.Name, c.CompetencyCategoryId })
                .ToListAsync();
            var categoryIds = competencies.Select(c => c.CompetencyCategoryId).Distinct().ToList();
            var categories = await categoryRepository.GetAll().AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            return [.. rows
                .Select(r =>
                {
                    var c = competencies.FirstOrDefault(x => x.Id == r.CompetencyId);
                    return new CourseCompetencyDto
                    {
                        Id = r.Id,
                        TrainingCourseId = r.TrainingCourseId,
                        CompetencyId = r.CompetencyId,
                        CompetencyName = c?.Name ?? "(removed competency)",
                        CategoryName = c is null ? null : categories.GetValueOrDefault(c.CompetencyCategoryId),
                        IsPrimary = r.IsPrimary
                    };
                })
                // Primary first, then alphabetical — the same order the recommendation uses, so the
                // editor shows what a learner would be offered.
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.CompetencyName)];
        }
    }

    /// <summary>
    /// Replaces a course's competency mapping in one call — the set-semantics the rest of this
    /// codebase uses for ordered/keyed child collections (see <c>LearningPath.SetSteps</c>).
    /// </summary>
    public class SetCourseCompetencies(
        IRepository<CourseCompetency> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Competency> competencyRepository,
        ILogger<SetCourseCompetencies> logger) : ISetCourseCompetencies
    {
        public async Task SetAsync(SetCourseCompetenciesDto dto)
        {
            if (dto.TrainingCourseId == Guid.Empty)
                throw new ValidationException(nameof(dto.TrainingCourseId), "A course is required.");
            if (!await courseRepository.GetAll().AnyAsync(c => c.Id == dto.TrainingCourseId))
                throw new NotFoundException(nameof(TrainingCourse), dto.TrainingCourseId.ToString());

            var specs = (dto.Competencies ?? [])
                .Where(x => x.CompetencyId != Guid.Empty)
                // The unique index would reject a duplicate pair with a 2601; collapsing here gives
                // the user "you listed it twice" behaviour instead of a database error.
                .GroupBy(x => x.CompetencyId)
                .Select(g => new CourseCompetencySpecDto { CompetencyId = g.Key, IsPrimary = g.Any(x => x.IsPrimary) })
                .ToList();

            if (specs.Count > 0)
            {
                var ids = specs.Select(s => s.CompetencyId).ToList();
                var known = await competencyRepository.GetAll()
                    .Where(c => ids.Contains(c.Id)).Select(c => c.Id).ToListAsync();
                if (known.Count != ids.Count)
                    throw new ValidationException("competencies", "One of those competencies no longer exists.");
            }

            var existing = await repository.GetAll()
                .Where(x => x.TrainingCourseId == dto.TrainingCourseId)
                .ToListAsync();

            // Rows are matched by competency rather than deleted and re-inserted, so an unchanged
            // mapping keeps its id and audit trail — re-saving the form must not look like the whole
            // mapping was replaced.
            foreach (var row in existing)
            {
                var spec = specs.FirstOrDefault(s => s.CompetencyId == row.CompetencyId);
                if (spec is null)
                {
                    repository.Delete(row);
                }
                else if (row.IsPrimary != spec.IsPrimary)
                {
                    row.SetPrimary(spec.IsPrimary);
                    repository.UpdateAsync(row);
                }
            }

            foreach (var spec in specs.Where(s => existing.All(e => e.CompetencyId != s.CompetencyId)))
                await repository.AddAsync(CourseCompetency.Create(dto.TrainingCourseId, spec.CompetencyId, spec.IsPrimary));

            await repository.SaveChangesAsync();
            logger.LogInformation("Course {CourseId} now develops {Count} competency(ies)",
                dto.TrainingCourseId, specs.Count);
        }
    }

    /// <summary>
    /// Shared lookup: the courses that develop a given set of competencies.
    ///
    /// <para>Used by the suggestion engine so a competency gap can name real courses. Kept here
    /// rather than inline so any future surface — a learner's "recommended for you", a learning-path
    /// generator — resolves recommendations the same way.</para>
    /// </summary>
    internal static class CourseRecommendation
    {
        internal static async Task<Dictionary<Guid, List<RecommendedCourseDto>>> ForCompetenciesAsync(
            IRepository<CourseCompetency> mappings,
            IRepository<TrainingCourse> courses,
            IRepository<TrainingSession> sessions,
            IReadOnlyCollection<Guid> competencyIds,
            int perCompetency = 3)
        {
            var result = new Dictionary<Guid, List<RecommendedCourseDto>>();
            if (competencyIds.Count == 0) return result;

            var pairs = await mappings.GetAll().AsNoTracking()
                .Where(m => competencyIds.Contains(m.CompetencyId))
                .Select(m => new { m.CompetencyId, m.TrainingCourseId, m.IsPrimary })
                .ToListAsync();
            if (pairs.Count == 0) return result;

            var courseIds = pairs.Select(p => p.TrainingCourseId).Distinct().ToList();
            // Only ACTIVE courses are recommended — a retired course is still mapped (history) but
            // must never be offered.
            var catalogue = await courses.GetAll().AsNoTracking()
                .Where(c => courseIds.Contains(c.Id) && c.IsActive)
                .Select(c => new { c.Id, c.Name, c.Code, c.DurationHours, c.DeliveryMode })
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var openSessions = (await sessions.GetAll().AsNoTracking()
                .Where(s => courseIds.Contains(s.TrainingCourseId)
                    && s.Status == TrainingSessionStatus.Scheduled
                    && s.StartDate >= today)
                .Select(s => s.TrainingCourseId)
                .ToListAsync())
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var competencyId in competencyIds.Distinct())
            {
                var offered = pairs
                    .Where(p => p.CompetencyId == competencyId)
                    .Select(p => new { p.IsPrimary, Course = catalogue.FirstOrDefault(c => c.Id == p.TrainingCourseId) })
                    .Where(x => x.Course is not null)
                    .Select(x => new RecommendedCourseDto
                    {
                        TrainingCourseId = x.Course!.Id,
                        CourseName = x.Course.Name,
                        CourseCode = x.Course.Code,
                        DurationHours = x.Course.DurationHours,
                        DeliveryMode = x.Course.DeliveryMode.ToString(),
                        IsPrimary = x.IsPrimary,
                        UpcomingSessions = openSessions.GetValueOrDefault(x.Course.Id)
                    })
                    // Primary mappings first, then whatever the learner can actually join soonest.
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenByDescending(x => x.UpcomingSessions)
                    .ThenBy(x => x.CourseName)
                    .Take(perCompetency)
                    .ToList();

                if (offered.Count > 0) result[competencyId] = offered;
            }

            return result;
        }
    }
}
