using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs ---------------------------------------------------------------
    /// <summary>An open internal vacancy as seen by an employee browsing the internal job market.</summary>
    public class OpenVacancyDto
    {
        public Guid Id { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? RoleName { get; set; }
        public string? LocationName { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public int NumberOfPositions { get; set; }
        public string? MinQualifications { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? Skills { get; set; }
        public string? PostingText { get; set; }
        public string PostingChannel { get; set; } = string.Empty;
        public DateTime? OpenFrom { get; set; }
        public DateTime? OpenUntil { get; set; }
        /// <summary>True when the caller has already applied to this vacancy.</summary>
        public bool AlreadyApplied { get; set; }
        /// <summary>The caller's application stage on this vacancy (null when not applied).</summary>
        public string? MyApplicationStage { get; set; }
    }

    /// <summary>One of the caller's own applications (self-service tracking).</summary>
    public class MyApplicationDto
    {
        public Guid ApplicationId { get; set; }
        public Guid RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public string? RequisitionTitle { get; set; }
        public string? DepartmentName { get; set; }
        public string Stage { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }

    public class ApplyToVacancyDto
    {
        /// <summary>Mandatory data-processing consent (HC097) — the internal candidate cannot be created without it.</summary>
        public bool ConsentGiven { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetOpenVacancies { Task<List<OpenVacancyDto>> GetAsync(); }
    public interface IApplyToVacancy { Task<Guid> ApplyAsync(Guid requisitionId, ApplyToVacancyDto dto); }
    public interface IGetMyApplications { Task<List<MyApplicationDto>> GetAsync(); }

    internal static class InternalVacancyShared
    {
        /// <summary>A requisition currently open to the INTERNAL job market (posted, right channel, in window).</summary>
        internal static bool IsOpenToInternal(RequisitionStatus status, PostingChannel channel,
            DateTime? openFrom, DateTime? openUntil, DateTime today) =>
            status == RequisitionStatus.Posted
            && channel is PostingChannel.Internal or PostingChannel.Both
            && (openFrom is null || openFrom.Value.Date <= today)
            && (openUntil is null || openUntil.Value.Date >= today);
    }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>
    /// The internal job market for the signed-in employee: the currently-open requisitions posted to
    /// the Internal or Both channel, within their open window. Gated to a linked employee; no
    /// recruitment permission required (this is self-service, not the HR console).
    /// </summary>
    public class GetOpenVacancies(
        IRepository<JobRequisition> requisitions,
        IRepository<Candidate> candidates,
        IRepository<JobApplication> applications,
        IRepository<OrganizationUnit> units,
        IRepository<PositionClass> positionClasses,
        IRepository<WorkLocation> workLocations,
        IPerformanceVisibilityService visibility) : IGetOpenVacancies
    {
        public async Task<List<OpenVacancyDto>> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");
            var empId = scope.EmployeeId.Value;
            var today = DateTime.UtcNow.Date;

            // The caller's own applications (via their internal candidate record) → applied set.
            var myCandidateIds = await candidates.GetAll().AsNoTracking()
                .Where(c => c.InternalEmployeeId == empId).Select(c => c.Id).ToListAsync();
            var myApps = myCandidateIds.Count == 0
                ? []
                : await applications.GetAll().AsNoTracking()
                    .Where(a => myCandidateIds.Contains(a.CandidateId))
                    .Select(a => new { a.RequisitionId, a.Stage })
                    .ToListAsync();
            var appliedStage = myApps
                .GroupBy(a => a.RequisitionId)
                .ToDictionary(g => g.Key, g => g.First().Stage.ToString());

            var rows = await requisitions.GetAll().AsNoTracking()
                .Where(r => r.Status == RequisitionStatus.Posted
                    && (r.PostingChannel == PostingChannel.Internal || r.PostingChannel == PostingChannel.Both)
                    && (r.OpenFrom == null || r.OpenFrom <= today)
                    && (r.OpenUntil == null || r.OpenUntil >= today))
                .OrderByDescending(r => r.PostedAt)
                .Select(r => new
                {
                    Requisition = r,
                    UnitName = units.GetAll().Where(u => u.Id == r.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    RoleName = positionClasses.GetAll().Where(c => c.Id == r.PositionClassId).Select(c => c.Title).FirstOrDefault(),
                    LocationName = r.WorkLocationId == null
                        ? null
                        : workLocations.GetAll().Where(w => w.Id == r.WorkLocationId).Select(w => w.Name).FirstOrDefault()
                })
                .ToListAsync();

            return rows.Select(x => new OpenVacancyDto
            {
                Id = x.Requisition.Id,
                RequisitionNumber = x.Requisition.RequisitionNumber,
                Title = x.Requisition.Title,
                DepartmentName = x.UnitName,
                RoleName = x.RoleName,
                LocationName = x.LocationName,
                EmploymentType = x.Requisition.EmploymentType.ToString(),
                NumberOfPositions = x.Requisition.NumberOfPositions,
                MinQualifications = x.Requisition.MinQualifications,
                MinExperienceYears = x.Requisition.MinExperienceYears,
                Skills = x.Requisition.Skills,
                PostingText = x.Requisition.PostingText,
                PostingChannel = x.Requisition.PostingChannel.ToString(),
                OpenFrom = x.Requisition.OpenFrom,
                OpenUntil = x.Requisition.OpenUntil,
                AlreadyApplied = appliedStage.ContainsKey(x.Requisition.Id),
                MyApplicationStage = appliedStage.GetValueOrDefault(x.Requisition.Id)
            }).ToList();
        }
    }

    /// <summary>
    /// Applies the signed-in employee to an open internal vacancy: finds-or-creates their internal
    /// Candidate record (Source=Internal, linked to their employee + person, HC090) and creates the
    /// JobApplication — respecting the module's one-application-per-candidate-per-vacancy rule.
    /// </summary>
    public class ApplyToVacancy(
        IRepository<JobRequisition> requisitions,
        IRepository<Candidate> candidates,
        IRepository<JobApplication> applications,
        IRepository<Employee> employees,
        INumberSequenceService numberSequence,
        ICurrentUserService currentUser,
        IPerformanceVisibilityService visibility,
        ILogger<ApplyToVacancy> logger) : IApplyToVacancy
    {
        public async Task<Guid> ApplyAsync(Guid requisitionId, ApplyToVacancyDto dto)
        {
            if (!dto.ConsentGiven)
                throw new ValidationException("consentGiven",
                    "You must consent to your details being used for this application.");

            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");
            var empId = scope.EmployeeId.Value;
            var today = DateTime.UtcNow.Date;

            var requisition = await requisitions.GetAll().FirstOrDefaultAsync(r => r.Id == requisitionId)
                ?? throw new NotFoundException(nameof(JobRequisition), requisitionId.ToString());
            if (!InternalVacancyShared.IsOpenToInternal(requisition.Status, requisition.PostingChannel,
                    requisition.OpenFrom, requisition.OpenUntil, today))
                throw new ValidationException("requisitionId",
                    "This vacancy is not open to internal applicants.");

            // Find-or-create the caller's internal candidate record (reuses their employee person).
            var candidate = await candidates.GetAll()
                .FirstOrDefaultAsync(c => c.InternalEmployeeId == empId && !c.IsArchived);
            if (candidate is null)
            {
                var e = await employees.GetAll().AsNoTracking().Include(x => x.Person)
                    .FirstOrDefaultAsync(x => x.Id == empId)
                    ?? throw new NotFoundException(nameof(Employee), empId.ToString());
                var person = e.Person;
                var number = await RecruitmentShared.NextNumberAsync(numberSequence, "Candidate", "CND");
                candidate = Candidate.Create(number, person?.FirstName ?? "Employee",
                    CandidateSource.Internal, consentGiven: true,
                    fatherName: person?.FatherName, grandFatherName: person?.GrandFatherName,
                    email: e.Email, phoneNumber: person?.PhoneNumber, gender: person?.Gender,
                    internalEmployeeId: empId);
                candidate.SetPerson(e.PersonId);
                await candidates.AddAsync(candidate);
            }

            if (await applications.GetAll().AnyAsync(a =>
                    a.CandidateId == candidate.Id && a.RequisitionId == requisitionId))
                throw new DuplicateException(nameof(JobApplication), "vacancy", requisition.RequisitionNumber);

            var created = JobApplication.Create(candidate.Id, requisitionId, DateTime.UtcNow,
                currentUser.GetCurrentUserName());
            await applications.AddAsync(created);
            foreach (var log in created.StageLog)
                if (string.IsNullOrEmpty(log.TenantId))
                    log.TenantId = created.TenantId;
            await applications.SaveChangesAsync();
            logger.LogInformation("Employee {EmployeeId} applied to vacancy {Requisition} (candidate {Candidate})",
                empId, requisition.RequisitionNumber, candidate.CandidateNumber);
            return created.Id;
        }
    }

    /// <summary>The signed-in employee's own applications, newest first (self-service tracking).</summary>
    public class GetMyApplications(
        IRepository<Candidate> candidates,
        IRepository<JobApplication> applications,
        IRepository<JobRequisition> requisitions,
        IRepository<OrganizationUnit> units,
        IPerformanceVisibilityService visibility) : IGetMyApplications
    {
        public async Task<List<MyApplicationDto>> GetAsync()
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.EmployeeId.HasValue)
                throw new ValidationException("employee", "Your account is not linked to an employee record.");
            var empId = scope.EmployeeId.Value;

            var myCandidateIds = await candidates.GetAll().AsNoTracking()
                .Where(c => c.InternalEmployeeId == empId).Select(c => c.Id).ToListAsync();
            if (myCandidateIds.Count == 0) return [];

            var rows = await applications.GetAll().AsNoTracking()
                .Where(a => myCandidateIds.Contains(a.CandidateId))
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    a.Id,
                    a.RequisitionId,
                    a.Stage,
                    a.AppliedAt,
                    ReqNumber = requisitions.GetAll().Where(r => r.Id == a.RequisitionId).Select(r => r.RequisitionNumber).FirstOrDefault(),
                    ReqTitle = requisitions.GetAll().Where(r => r.Id == a.RequisitionId).Select(r => r.Title).FirstOrDefault(),
                    UnitName = requisitions.GetAll().Where(r => r.Id == a.RequisitionId)
                        .Select(r => units.GetAll().Where(u => u.Id == r.OrganizationUnitId).Select(u => u.Name).FirstOrDefault())
                        .FirstOrDefault()
                })
                .ToListAsync();

            return rows.Select(x => new MyApplicationDto
            {
                ApplicationId = x.Id,
                RequisitionId = x.RequisitionId,
                RequisitionNumber = x.ReqNumber,
                RequisitionTitle = x.ReqTitle,
                DepartmentName = x.UnitName,
                Stage = x.Stage.ToString(),
                AppliedAt = x.AppliedAt
            }).ToList();
        }
    }
}
