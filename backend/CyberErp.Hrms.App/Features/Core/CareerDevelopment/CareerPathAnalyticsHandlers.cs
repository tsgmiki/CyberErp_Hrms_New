using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class VisualizeCompetencyDto
    {
        public Guid CompetencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        /// <summary>The employee's current position already carries this competency (only when an employee is supplied).</summary>
        public bool IsMet { get; set; }
    }

    public class CareerPathVisualizeStepDto
    {
        public Guid StepId { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? PositionClassId { get; set; }
        public string? PositionClassName { get; set; }
        public int? RequiredExperienceMonths { get; set; }
        public string? Certifications { get; set; }
        /// <summary>The assigned employee's milestone status at this step (null when no employee/assignment).</summary>
        public string? ProgressStatus { get; set; }
        public bool IsCurrentStep { get; set; }
        public int RequiredCount { get; set; }
        public int MetCount { get; set; }
        public List<VisualizeCompetencyDto> Competencies { get; set; } = [];
    }

    /// <summary>Career-path visualisation (HC166) with an optional employee overlay (HC164/HC165 gaps + progress).</summary>
    public class CareerPathVisualizeDto
    {
        public Guid CareerPathId { get; set; }
        public string CareerPathName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? ProgressPercent { get; set; }
        public List<CareerPathVisualizeStepDto> Steps { get; set; } = [];
    }

    public class CareerPathUtilizationDto
    {
        public Guid CareerPathId { get; set; }
        public string CareerPathName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int OnHoldCount { get; set; }
        public decimal AvgProgress { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IVisualizeCareerPath { Task<CareerPathVisualizeDto> GetAsync(Guid careerPathId, Guid? employeeId); }
    public interface IGetCareerPathUtilization { Task<List<CareerPathUtilizationDto>> GetAsync(); }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>
    /// Builds the ordered step ladder for a path and, when an employee is supplied, overlays their
    /// milestone progress (HC165) and per-step competency gap (HC164). A handful of set-based queries
    /// (steps, step-competencies, the employee's current-position competencies, the assignment) — no N+1.
    /// </summary>
    public class VisualizeCareerPath(
        IRepository<CareerPath> pathRepository,
        IRepository<CareerPathStep> stepRepository,
        IRepository<CareerPathStepCompetency> stepCompetencyRepository,
        IRepository<Competency> competencyRepository,
        IRepository<Employee> employeeRepository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        IRepository<EmployeeCareerPath> assignmentRepository) : IVisualizeCareerPath
    {
        public async Task<CareerPathVisualizeDto> GetAsync(Guid careerPathId, Guid? employeeId)
        {
            var path = await pathRepository.GetAll().Where(p => p.Id == careerPathId)
                    .Select(p => new { p.Id, p.Name }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(CareerPath), careerPathId.ToString());

            var steps = await stepRepository.GetAll().Where(s => s.CareerPathId == careerPathId)
                .OrderBy(s => s.StepOrder)
                .Select(s => new CareerPathVisualizeStepDto
                {
                    StepId = s.Id, StepOrder = s.StepOrder, Name = s.Name,
                    PositionClassId = s.PositionClassId,
                    PositionClassName = s.PositionClass != null ? s.PositionClass.Title : null,
                    RequiredExperienceMonths = s.RequiredExperienceMonths, Certifications = s.Certifications
                }).ToListAsync();
            var stepIds = steps.Select(s => s.StepId).ToList();

            // All required competencies across the path's steps (one set-based join).
            var stepComps = await stepCompetencyRepository.GetAll()
                .Where(sc => stepIds.Contains(sc.CareerPathStepId))
                .Join(competencyRepository.GetAll(), sc => sc.CompetencyId, c => c.Id,
                    (sc, c) => new { sc.CareerPathStepId, sc.CompetencyId, c.Name, sc.Weight })
                .ToListAsync();

            // The employee's current-position competency set (met) + their assignment (progress).
            HashSet<Guid> metSet = [];
            Dictionary<Guid, string> progressByStep = [];
            Guid? currentStepId = null;
            decimal? progressPercent = null;
            string? employeeName = null;
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                var emp = await employeeRepository.GetAll().Where(e => e.Id == employeeId.Value)
                    .Select(e => new
                    {
                        e.PositionId,
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : null
                    }).FirstOrDefaultAsync();
                employeeName = emp?.Name;
                if (emp?.PositionId is Guid pos)
                    metSet = (await positionCompetencyRepository.GetAll().Where(pc => pc.PositionId == pos)
                        .Select(pc => pc.CompetencyId).ToListAsync()).ToHashSet();

                var assignment = await assignmentRepository.GetAll()
                    .Where(a => a.EmployeeId == employeeId.Value && a.CareerPathId == careerPathId)
                    .Select(a => new { a.CurrentStepId, a.ProgressPercent, Progress = a.StepProgress.Select(p => new { p.CareerPathStepId, p.Status }) })
                    .FirstOrDefaultAsync();
                if (assignment != null)
                {
                    currentStepId = assignment.CurrentStepId;
                    progressPercent = assignment.ProgressPercent;
                    progressByStep = assignment.Progress.ToDictionary(p => p.CareerPathStepId, p => p.Status.ToString());
                }
            }

            foreach (var step in steps)
            {
                var comps = stepComps.Where(sc => sc.CareerPathStepId == step.StepId)
                    .Select(sc => new VisualizeCompetencyDto
                    {
                        CompetencyId = sc.CompetencyId, Name = sc.Name, Weight = sc.Weight,
                        IsMet = metSet.Contains(sc.CompetencyId)
                    }).ToList();
                step.Competencies = comps;
                step.RequiredCount = comps.Count;
                step.MetCount = comps.Count(c => c.IsMet);
                step.IsCurrentStep = currentStepId.HasValue && currentStepId.Value == step.StepId;
                if (progressByStep.TryGetValue(step.StepId, out var st)) step.ProgressStatus = st;
            }

            return new CareerPathVisualizeDto
            {
                CareerPathId = path.Id, CareerPathName = path.Name,
                EmployeeId = employeeId, EmployeeName = employeeName,
                ProgressPercent = progressPercent, Steps = steps
            };
        }
    }

    /// <summary>Career-path utilisation (HC166): assignment counts, status breakdown and average progress per
    /// path — a single GROUP BY over the assignments joined to their path.</summary>
    public class GetCareerPathUtilization(
        IRepository<CareerPath> pathRepository,
        IRepository<EmployeeCareerPath> assignmentRepository) : IGetCareerPathUtilization
    {
        public async Task<List<CareerPathUtilizationDto>> GetAsync()
        {
            var stats = await assignmentRepository.GetAll()
                .GroupBy(a => a.CareerPathId)
                .Select(g => new
                {
                    CareerPathId = g.Key,
                    AssignedCount = g.Count(),
                    ActiveCount = g.Count(x => x.Status == EmployeeCareerPathStatus.Active),
                    CompletedCount = g.Count(x => x.Status == EmployeeCareerPathStatus.Completed),
                    OnHoldCount = g.Count(x => x.Status == EmployeeCareerPathStatus.OnHold),
                    AvgProgress = g.Average(x => x.ProgressPercent)
                }).ToListAsync();
            var byId = stats.ToDictionary(s => s.CareerPathId);

            // Every active path appears, even those with no assignments yet.
            var paths = await pathRepository.GetAll().Where(p => p.IsActive)
                .Select(p => new { p.Id, p.Name, p.Code }).ToListAsync();

            return paths.Select(p =>
            {
                byId.TryGetValue(p.Id, out var s);
                return new CareerPathUtilizationDto
                {
                    CareerPathId = p.Id, CareerPathName = p.Name, Code = p.Code,
                    AssignedCount = s?.AssignedCount ?? 0,
                    ActiveCount = s?.ActiveCount ?? 0,
                    CompletedCount = s?.CompletedCount ?? 0,
                    OnHoldCount = s?.OnHoldCount ?? 0,
                    AvgProgress = s != null ? Math.Round(s.AvgProgress, 1) : 0m
                };
            }).OrderByDescending(x => x.AssignedCount).ToList();
        }
    }
}
