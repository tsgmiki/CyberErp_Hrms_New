using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class CpdEntryDto
    {
        public Guid TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public DateTime? CompletedOn { get; set; }
        public decimal CpdHours { get; set; }
        public decimal? AssessmentScore { get; set; }
    }

    public class CpdSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public int? Year { get; set; }
        public decimal TotalCpdHours { get; set; }
        public int CompletedTrainings { get; set; }
        public int Certificates { get; set; }
        public List<CpdEntryDto> Entries { get; set; } = [];
    }

    // ---- Interface ----------------------------------------------------------
    /// <summary>
    /// HC200 — continuing professional development: CPD hours roll up from completed enrollments
    /// (each course carries its CPD hours), optionally windowed to one year.
    /// </summary>
    public interface IGetCpdSummary { Task<CpdSummaryDto> GetAsync(Guid? employeeId, int? year); }

    // ---- Handler ------------------------------------------------------------
    public class GetCpdSummary(
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<EmployeeTrainingCertificate> certificateRepository,
        IPerformanceVisibilityService visibility) : IGetCpdSummary
    {
        public async Task<CpdSummaryDto> GetAsync(Guid? employeeId, int? year)
        {
            var scope = await visibility.GetScopeAsync();
            var target = employeeId ?? scope.EmployeeId
                ?? throw new ValidationException(nameof(employeeId), "Your account is not linked to an employee record.");
            if (!await visibility.CanAccessEmployeeAsync(target))
                throw new ValidationException(nameof(employeeId), "The employee is outside your scope.");

            var query = enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == target && e.Status == TrainingEnrollmentStatus.Completed);
            if (year.HasValue)
            {
                var from = new DateTime(year.Value, 1, 1);
                var to = new DateTime(year.Value + 1, 1, 1);
                query = query.Where(e => e.CompletedOn >= from && e.CompletedOn < to);
            }

            var sessions = sessionRepository.GetAll();
            var courses = courseRepository.GetAll();
            var entries = await query
                .Join(sessions, e => e.TrainingSessionId, s => s.Id, (e, s) => new { e, s.TrainingCourseId })
                .Join(courses, x => x.TrainingCourseId, c => c.Id, (x, c) => new CpdEntryDto
                {
                    TrainingCourseId = c.Id,
                    CourseName = c.Name,
                    CompletedOn = x.e.CompletedOn,
                    CpdHours = c.CpdHours,
                    AssessmentScore = x.e.AssessmentScore
                })
                .OrderByDescending(x => x.CompletedOn)
                .ToListAsync();

            var certificates = await certificateRepository.GetAll().AsNoTracking()
                .CountAsync(c => c.EmployeeId == target);

            return new CpdSummaryDto
            {
                EmployeeId = target,
                Year = year,
                TotalCpdHours = entries.Sum(x => x.CpdHours),
                CompletedTrainings = entries.Count,
                Certificates = certificates,
                Entries = entries
            };
        }
    }
}
