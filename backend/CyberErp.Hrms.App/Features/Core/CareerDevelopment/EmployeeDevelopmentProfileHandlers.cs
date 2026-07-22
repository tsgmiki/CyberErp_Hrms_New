using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class DevEmployeeCareerPathDto
    {
        public Guid Id { get; set; }
        public Guid CareerPathId { get; set; }
        public string? CareerPathName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
    }

    public class DevSuccessionCandidacyDto
    {
        public Guid Id { get; set; }
        public Guid SuccessionPlanId { get; set; }
        public string? PlanName { get; set; }
        public string? RoleTitle { get; set; }
        public int Rank { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
    }

    public class DevMentorshipDto
    {
        public Guid Id { get; set; }
        /// <summary>"Mentor" (the employee mentors someone) or "Mentee".</summary>
        public string Role { get; set; } = string.Empty;
        public string? CounterpartName { get; set; }
        public string Context { get; set; } = nameof(MentorshipContext.General);
        public string Status { get; set; } = nameof(MentorshipStatus.Active);
    }

    /// <summary>
    /// A single holistic "development" view of one employee (HC158) — the bridge between the Performance
    /// and Career Development modules: performance snapshot + career-path progress + succession candidacy
    /// + the next-step competency gap + mentorships. Composes the already-optimised per-area handlers;
    /// invoked on demand for one employee (never in a list render).
    /// </summary>
    public class EmployeeDevelopmentProfileDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public EmployeePerformanceSummaryDto? Performance { get; set; }
        public List<DevEmployeeCareerPathDto> CareerPaths { get; set; } = [];
        public List<DevSuccessionCandidacyDto> SuccessionCandidacies { get; set; } = [];
        public List<DevMentorshipDto> Mentorships { get; set; } = [];
        /// <summary>Competency gap for the next step of the employee's primary (active) career path (HC164).</summary>
        public DevelopmentRecommendationDto? NextStepGap { get; set; }
    }

    // ---- Interface ----------------------------------------------------------
    public interface IGetEmployeeDevelopmentProfile { Task<EmployeeDevelopmentProfileDto> GetAsync(Guid employeeId); }

    // ---- Handler ------------------------------------------------------------
    public class GetEmployeeDevelopmentProfile(
        IRepository<Employee> employeeRepository,
        IGetEmployeePerformanceSummary performanceHandler,
        IGetCareerPathRecommendations recommendationsHandler,
        IRepository<EmployeeCareerPath> careerPathRepository,
        IRepository<SuccessionCandidate> candidateRepository,
        IRepository<SuccessionPlan> planRepository,
        IRepository<CriticalPosition> criticalPositionRepository,
        IRepository<Position> positionRepository,
        IRepository<Mentorship> mentorshipRepository) : IGetEmployeeDevelopmentProfile
    {
        public async Task<EmployeeDevelopmentProfileDto> GetAsync(Guid employeeId)
        {
            var name = await employeeRepository.GetAll().Where(e => e.Id == employeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : null)
                    .FirstOrDefaultAsync();
            if (name is null && !await employeeRepository.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());

            // Career-path assignments + progress (HC163/HC165).
            var careerPaths = await careerPathRepository.GetAll().Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AssignedDate)
                .Select(a => new DevEmployeeCareerPathDto
                {
                    Id = a.Id, CareerPathId = a.CareerPathId,
                    CareerPathName = a.CareerPath != null ? a.CareerPath.Name : null,
                    Status = a.Status.ToString(), ProgressPercent = a.ProgressPercent
                }).ToListAsync();

            // Succession candidacies + readiness (HC153/HC154) — with the target role title.
            var candidacies = await candidateRepository.GetAll().Where(c => c.EmployeeId == employeeId)
                .Join(planRepository.GetAll(), c => c.SuccessionPlanId, p => p.Id, (c, p) => new { c, p })
                .Select(x => new DevSuccessionCandidacyDto
                {
                    Id = x.c.Id, SuccessionPlanId = x.p.Id, PlanName = x.p.Name,
                    RoleTitle = criticalPositionRepository.GetAll()
                        .Where(cp => cp.Id == x.p.CriticalPositionId)
                        .Join(positionRepository.GetAll(), cp => cp.PositionId, pos => pos.Id,
                            (cp, pos) => pos.PositionClass != null ? pos.PositionClass.Title : pos.Code)
                        .FirstOrDefault(),
                    Rank = x.c.Rank, Readiness = x.c.Readiness.ToString(), ReadinessScore = x.c.ReadinessScore
                }).ToListAsync();

            // Mentorships the employee is part of (HC168) — as mentor or mentee.
            var mentorships = await mentorshipRepository.GetAll()
                .Where(m => m.MentorEmployeeId == employeeId || m.MenteeEmployeeId == employeeId)
                .Select(m => new DevMentorshipDto
                {
                    Id = m.Id,
                    Role = m.MentorEmployeeId == employeeId ? "Mentor" : "Mentee",
                    CounterpartName = m.MentorEmployeeId == employeeId
                        ? (m.Mentee != null && m.Mentee.Person != null ? m.Mentee.Person.FirstName + " " + m.Mentee.Person.GrandFatherName : null)
                        : (m.Mentor != null && m.Mentor.Person != null ? m.Mentor.Person.FirstName + " " + m.Mentor.Person.GrandFatherName : null),
                    Context = m.Context.ToString(), Status = m.Status.ToString()
                }).ToListAsync();

            // Next-step competency gap for the primary (most recent Active) career-path assignment (HC164).
            DevelopmentRecommendationDto? nextStepGap = null;
            var primary = careerPaths.FirstOrDefault(a => a.Status == nameof(EmployeeCareerPathStatus.Active))
                ?? careerPaths.FirstOrDefault();
            if (primary != null)
                nextStepGap = await recommendationsHandler.GetAsync(primary.Id);

            return new EmployeeDevelopmentProfileDto
            {
                EmployeeId = employeeId,
                EmployeeName = name,
                Performance = await performanceHandler.GetAsync(employeeId),
                CareerPaths = careerPaths,
                SuccessionCandidacies = candidacies,
                Mentorships = mentorships,
                NextStepGap = nextStepGap,
            };
        }
    }
}
