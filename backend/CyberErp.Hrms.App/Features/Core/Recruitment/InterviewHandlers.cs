using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs -----------------------------------------------------------------

    public class InterviewPanelistDto
    {
        public Guid? Id { get; set; }
        public Guid? EmployeeId { get; set; }
        public string PanelistName { get; set; } = string.Empty;
        public bool IsLead { get; set; }
        public string Attendance { get; set; } = nameof(PanelistAttendance.Pending);
        public List<InterviewFeedbackDto> Feedback { get; set; } = [];
        public decimal? AverageScore { get; set; }
    }

    public class InterviewFeedbackDto
    {
        public Guid? CriterionId { get; set; }
        public string? CriterionName { get; set; }
        public decimal Score { get; set; }
        public string? Comments { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class InterviewDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public int Round { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Format { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
        public List<InterviewPanelistDto> Panelists { get; set; } = [];
        public decimal? AverageScore { get; set; }
    }

    public class SaveInterviewDto
    {
        public Guid? Id { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Format { get; set; } = nameof(InterviewFormat.InPerson);
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
        public List<InterviewPanelistDto> Panelists { get; set; } = [];
    }

    public class SaveInterviewDtoValidator : AbstractValidator<SaveInterviewDto>
    {
        public SaveInterviewDtoValidator()
        {
            RuleFor(x => x.ApplicationId).NotEmpty();
            RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart)
                .WithMessage("The interview must end after it starts.");
            RuleFor(x => x.Panelists).NotEmpty().WithMessage("At least one panelist is required (HC104).");
            RuleFor(x => x.Panelists.Count(p => p.IsLead)).LessThanOrEqualTo(1)
                .WithMessage("Only one panelist can lead the interview.");
        }
    }

    public class SetInterviewStatusDto
    {
        public Guid Id { get; set; }
        /// <summary>Complete | Cancel | NoShow</summary>
        public string Action { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class SubmitInterviewFeedbackDto
    {
        public Guid PanelistId { get; set; }
        public List<InterviewFeedbackDto> Entries { get; set; } = [];
    }

    /// <summary>HC109 — the consolidated evaluation of one application across all rounds.</summary>
    public class InterviewConsolidatedDto
    {
        public Guid ApplicationId { get; set; }
        public int Rounds { get; set; }
        public int PanelistCount { get; set; }
        public int ScoredPanelists { get; set; }
        /// <summary>Plain average over every feedback entry (incl. overall impressions).</summary>
        public decimal? OverallAverage { get; set; }
        /// <summary>
        /// Σ(criterion average × criterion weight) / Σ(weight) — the weights defined on the
        /// requisition's criteria are INHERITED here, not re-entered (requirement #1).
        /// </summary>
        public decimal? WeightedAverage { get; set; }
        public List<InterviewCriterionSummaryDto> Criteria { get; set; } = [];
        public List<InterviewDto> Interviews { get; set; } = [];
    }

    public class InterviewCriterionSummaryDto
    {
        public Guid? CriterionId { get; set; }
        public string CriterionName { get; set; } = string.Empty;
        /// <summary>The weight (%) inherited from the requisition criterion (0 for overall entries).</summary>
        public int Weight { get; set; }
        public decimal Average { get; set; }
        public int Scores { get; set; }
    }

    // ---- Interfaces -------------------------------------------------------------

    public interface ISaveInterview { Task<Guid> SaveAsync(SaveInterviewDto dto); }
    public interface IGetInterviews { Task<List<InterviewDto>> GetAsync(Guid applicationId); }
    public interface ISetInterviewStatus { Task SetAsync(SetInterviewStatusDto dto); }
    public interface ISubmitInterviewFeedback { Task SubmitAsync(SubmitInterviewFeedbackDto dto); }
    public interface IGetInterviewConsolidated { Task<InterviewConsolidatedDto> GetAsync(Guid applicationId); }
    public interface IDeleteInterview { Task DeleteAsync(Guid id); }

    internal static class InterviewShared
    {
        /// <summary>The repository stamps only aggregate roots — cascade-inserted children copy it here.</summary>
        internal static void StampChildrenTenant(Interview interview)
        {
            foreach (var p in interview.Panelists)
            {
                if (string.IsNullOrEmpty(p.TenantId)) p.TenantId = interview.TenantId;
                foreach (var f in p.Feedback)
                    if (string.IsNullOrEmpty(f.TenantId)) f.TenantId = p.TenantId;
            }
        }

        internal static InterviewDto ToDto(Interview i)
        {
            var panelists = i.Panelists.Select(p => new InterviewPanelistDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                PanelistName = p.PanelistName,
                IsLead = p.IsLead,
                Attendance = p.Attendance.ToString(),
                Feedback = p.Feedback.Select(f => new InterviewFeedbackDto
                {
                    CriterionId = f.CriterionId,
                    CriterionName = f.CriterionName,
                    Score = f.Score,
                    Comments = f.Comments,
                    SubmittedAt = f.SubmittedAt
                }).ToList(),
                AverageScore = p.Feedback.Count > 0 ? Math.Round(p.Feedback.Average(f => f.Score), 2) : null
            }).OrderByDescending(p => p.IsLead).ThenBy(p => p.PanelistName).ToList();

            var scored = panelists.Where(p => p.AverageScore.HasValue).ToList();
            return new InterviewDto
            {
                Id = i.Id,
                ApplicationId = i.ApplicationId,
                Round = i.Round,
                ScheduledStart = i.ScheduledStart,
                ScheduledEnd = i.ScheduledEnd,
                Format = i.Format.ToString(),
                Status = i.Status.ToString(),
                Location = i.Location,
                MeetingLink = i.MeetingLink,
                Notes = i.Notes,
                Panelists = panelists,
                AverageScore = scored.Count > 0 ? Math.Round(scored.Average(p => p.AverageScore!.Value), 2) : null
            };
        }

        /// <summary>Resolves panelist display names from the employee master (snapshot pattern).</summary>
        internal static async Task<List<InterviewPanelist>> BuildPanelAsync(
            IRepository<Employee> employees, Guid interviewId, List<InterviewPanelistDto> panelists)
        {
            var employeeIds = panelists.Where(p => p.EmployeeId.HasValue).Select(p => p.EmployeeId!.Value).ToList();
            var names = await employees.GetAll()
                .Where(e => employeeIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .ToDictionaryAsync(e => e.Id, e => e.Name);

            var result = new List<InterviewPanelist>();
            foreach (var p in panelists)
            {
                var name = p.EmployeeId.HasValue && names.TryGetValue(p.EmployeeId.Value, out var resolved)
                    ? resolved
                    : p.PanelistName;
                if (string.IsNullOrWhiteSpace(name))
                    throw new ValidationException("panelists", "Each panelist needs an employee or a name.");
                result.Add(InterviewPanelist.Create(interviewId, p.EmployeeId, name, p.IsLead));
            }
            return result;
        }
    }

    // ---- Handlers -----------------------------------------------------------------

    public class SaveInterview(
        IRepository<Interview> repository,
        IRepository<InterviewPanelist> panelistRepository,
        IRepository<JobApplication> applicationRepository,
        IRepository<Employee> employeeRepository,
        IValidator<SaveInterviewDto> validator,
        ILogger<SaveInterview> logger) : ISaveInterview
    {
        public async Task<Guid> SaveAsync(SaveInterviewDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!Enum.TryParse<InterviewFormat>(dto.Format, true, out var format))
                throw new ValidationException("format", "Unknown interview format.");

            var application = await applicationRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == dto.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), dto.ApplicationId.ToString());
            // LEVEL RULE: interviewing is the Interview step's activity — scheduling (and
            // rescheduling) happens only while the application sits AT that level. Moving the
            // candidate there is a deliberate pipeline decision, never a side effect.
            if (application.Stage != ApplicationStage.Interview)
                throw new ValidationException("applicationId",
                    $"Interviews are scheduled at the INTERVIEW level — move the application there first (current: {application.Stage}).");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(i => i.Panelists).ThenInclude(p => p.Feedback)
                        .FirstOrDefaultAsync(i => i.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Interview), dto.Id.Value.ToString());
                entity.Reschedule(dto.ScheduledStart, dto.ScheduledEnd, format, dto.Location, dto.MeetingLink, dto.Notes);
                entity.SetPanel(await InterviewShared.BuildPanelAsync(employeeRepository, entity.Id, dto.Panelists));
                InterviewShared.StampChildrenTenant(entity);
                // Replacement panelists are new rows — mark Added explicitly (app-generated keys).
                foreach (var p in entity.Panelists)
                    await panelistRepository.AddAsync(p);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Rescheduled Interview {Id}", entity.Id);
                return entity.Id;
            }

            var round = await repository.GetAll()
                .Where(i => i.ApplicationId == dto.ApplicationId)
                .Select(i => (int?)i.Round).MaxAsync() ?? 0;
            var created = Interview.Create(dto.ApplicationId, round + 1, dto.ScheduledStart, dto.ScheduledEnd,
                format, dto.Location, dto.MeetingLink, dto.Notes);
            created.SetPanel(await InterviewShared.BuildPanelAsync(employeeRepository, created.Id, dto.Panelists));
            await repository.AddAsync(created);   // stamps the root's TenantId
            InterviewShared.StampChildrenTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Scheduled Interview {Id} (round {Round}) for Application {ApplicationId}",
                created.Id, created.Round, dto.ApplicationId);
            return created.Id;
        }
    }

    public class GetInterviews(IRepository<Interview> repository) : IGetInterviews
    {
        public async Task<List<InterviewDto>> GetAsync(Guid applicationId)
        {
            var rows = await repository.GetAll()
                .Include(i => i.Panelists).ThenInclude(p => p.Feedback)
                .Where(i => i.ApplicationId == applicationId)
                .OrderBy(i => i.Round)
                .ToListAsync();
            return rows.Select(InterviewShared.ToDto).ToList();
        }
    }

    public class SetInterviewStatus(
        IRepository<Interview> repository,
        ILogger<SetInterviewStatus> logger) : ISetInterviewStatus
    {
        public async Task SetAsync(SetInterviewStatusDto dto)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(i => i.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Interview), dto.Id.ToString());

            switch (dto.Action?.Trim().ToLowerInvariant())
            {
                case "complete": entity.Complete(dto.Note); break;
                case "cancel": entity.Cancel(dto.Note); break;
                case "noshow": entity.MarkNoShow(); break;
                default: throw new ValidationException("action", "Action must be Complete, Cancel or NoShow.");
            }

            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Interview {Id} → {Status}", dto.Id, entity.Status);
        }
    }

    public class SubmitInterviewFeedback(
        IRepository<InterviewPanelist> repository,
        IRepository<InterviewFeedback> feedbackRepository,
        IRepository<Interview> interviewRepository,
        ILogger<SubmitInterviewFeedback> logger) : ISubmitInterviewFeedback
    {
        public async Task SubmitAsync(SubmitInterviewFeedbackDto dto)
        {
            if (dto.Entries.Count == 0)
                throw new ValidationException("entries", "At least one feedback entry is required.");

            var panelist = await repository.GetAll().Include(p => p.Feedback)
                    .FirstOrDefaultAsync(p => p.Id == dto.PanelistId)
                ?? throw new NotFoundException(nameof(InterviewPanelist), dto.PanelistId.ToString());
            var interview = await interviewRepository.GetAll()
                    .FirstOrDefaultAsync(i => i.Id == panelist.InterviewId)
                ?? throw new NotFoundException(nameof(Interview), panelist.InterviewId.ToString());
            if (interview.Status is InterviewStatus.Cancelled)
                throw new ValidationException("panelistId", "A cancelled interview cannot be scored.");

            var existing = panelist.Feedback.Select(f => f.Id).ToHashSet();
            foreach (var entry in dto.Entries)
                panelist.RecordFeedback(entry.CriterionId, entry.CriterionName, entry.Score, entry.Comments);
            panelist.SetAttendance(PanelistAttendance.Attended);

            foreach (var f in panelist.Feedback.Where(f => !existing.Contains(f.Id)))
            {
                if (string.IsNullOrEmpty(f.TenantId)) f.TenantId = panelist.TenantId;
                await feedbackRepository.AddAsync(f);
            }
            repository.UpdateAsync(panelist);
            await repository.SaveChangesAsync();
            logger.LogInformation("Recorded {Count} feedback entrie(s) for Panelist {Id}", dto.Entries.Count, dto.PanelistId);
        }
    }

    public class GetInterviewConsolidated(
        IRepository<Interview> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobRequisition> requisitionRepository) : IGetInterviewConsolidated
    {
        public async Task<InterviewConsolidatedDto> GetAsync(Guid applicationId)
        {
            var interviews = await repository.GetAll()
                .Include(i => i.Panelists).ThenInclude(p => p.Feedback)
                .Where(i => i.ApplicationId == applicationId)
                .OrderBy(i => i.Round)
                .ToListAsync();

            // Weights are INHERITED from the requisition's criteria (requirement #1) — the
            // panelists never re-enter them.
            var requisitionId = await applicationRepository.GetAll()
                .Where(a => a.Id == applicationId).Select(a => (Guid?)a.RequisitionId).FirstOrDefaultAsync();
            var weightsByCriterion = requisitionId.HasValue
                ? await requisitionRepository.GetAll()
                    .Where(q => q.Id == requisitionId.Value)
                    .SelectMany(q => q.ScreeningCriteria)
                    .ToDictionaryAsync(c => c.Id, c => c.Weight)
                : [];

            var dtos = interviews.Select(InterviewShared.ToDto).ToList();
            var allFeedback = interviews
                .Where(i => i.Status != InterviewStatus.Cancelled)
                .SelectMany(i => i.Panelists)
                .SelectMany(p => p.Feedback)
                .ToList();

            var criteria = allFeedback
                .GroupBy(f => new { f.CriterionId, Name = f.CriterionName ?? "Overall" })
                .Select(g => new InterviewCriterionSummaryDto
                {
                    CriterionId = g.Key.CriterionId,
                    CriterionName = g.Key.Name,
                    Weight = g.Key.CriterionId.HasValue ? weightsByCriterion.GetValueOrDefault(g.Key.CriterionId.Value) : 0,
                    Average = Math.Round(g.Average(f => f.Score), 2),
                    Scores = g.Count()
                })
                .OrderByDescending(c => c.Weight)
                .ThenByDescending(c => c.Average)
                .ToList();

            // Weighted total over the criteria that carry a weight (overall entries excluded).
            var weighted = criteria.Where(c => c.Weight > 0).ToList();
            var totalWeight = weighted.Sum(c => c.Weight);

            var panelists = interviews.SelectMany(i => i.Panelists).ToList();
            return new InterviewConsolidatedDto
            {
                ApplicationId = applicationId,
                Rounds = interviews.Count,
                PanelistCount = panelists.Count,
                ScoredPanelists = panelists.Count(p => p.Feedback.Count > 0),
                OverallAverage = allFeedback.Count > 0 ? Math.Round(allFeedback.Average(f => f.Score), 2) : null,
                WeightedAverage = totalWeight > 0
                    ? Math.Round(weighted.Sum(c => c.Average * c.Weight) / totalWeight, 2)
                    : null,
                Criteria = criteria,
                Interviews = dtos
            };
        }
    }

    public class DeleteInterview(
        IRepository<Interview> repository,
        ILogger<DeleteInterview> logger) : IDeleteInterview
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException(nameof(Interview), id.ToString());
            if (entity.Status != InterviewStatus.Scheduled)
                throw new ValidationException("id", $"A {entity.Status} interview is part of the record — cancel instead of deleting.");

            repository.Delete(entity);   // panel + feedback cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Interview {Id}", id);
        }
    }
}
