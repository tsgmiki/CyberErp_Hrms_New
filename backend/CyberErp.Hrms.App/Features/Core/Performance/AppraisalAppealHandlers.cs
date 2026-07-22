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
    public class SignAppraisalDto
    {
        public Guid Id { get; set; }
        public string Signature { get; set; } = string.Empty;
    }

    public class SubmitAppraisalAppealDto
    {
        public Guid AppraisalId { get; set; }
        public string Comments { get; set; } = string.Empty;
        public bool RequestFollowUp { get; set; }
    }

    public class ResolveAppraisalAppealDto
    {
        public Guid Id { get; set; }
        public bool Upheld { get; set; }
        public string Resolution { get; set; } = string.Empty;
    }

    public class AppraisalAppealDto
    {
        public Guid Id { get; set; }
        public Guid AppraisalId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Comments { get; set; } = string.Empty;
        public bool RequestFollowUp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class SignAppraisalDtoValidator : AbstractValidator<SignAppraisalDto>
    {
        public SignAppraisalDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Signature).NotEmpty().MaximumLength(200);
        }
    }

    public class SubmitAppraisalAppealDtoValidator : AbstractValidator<SubmitAppraisalAppealDto>
    {
        public SubmitAppraisalAppealDtoValidator()
        {
            RuleFor(x => x.AppraisalId).NotEmpty();
            RuleFor(x => x.Comments).NotEmpty().MaximumLength(2000);
        }
    }

    public class ResolveAppraisalAppealDtoValidator : AbstractValidator<ResolveAppraisalAppealDto>
    {
        public ResolveAppraisalAppealDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Resolution).NotEmpty().MaximumLength(2000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IAcknowledgeAppraisal { Task AcknowledgeAsync(SignAppraisalDto dto); }
    public interface IManagerSignAppraisal { Task SignAsync(SignAppraisalDto dto); }
    public interface ISubmitAppraisalAppeal { Task<Guid> SubmitAsync(SubmitAppraisalAppealDto dto); }
    public interface IStartAppraisalAppealReview { Task StartAsync(Guid id); }
    public interface IResolveAppraisalAppeal { Task ResolveAsync(ResolveAppraisalAppealDto dto); }
    public interface IGetAppraisalAppealById { Task<AppraisalAppealDto> GetAsync(Guid id); }
    public interface IGetAllAppraisalAppeals { Task<PaginatedResponse<AppraisalAppealDto>> GetAsync(GetAllRequest request); }

    // ---- Acknowledge / sign (HC142–HC143, HC146) ----------------------------
    public class AcknowledgeAppraisal(
        IRepository<Appraisal> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IPerformanceHistoryWriter history,
        IEnumerable<IAppraisalCompletedHandler> completedHandlers,
        IAppraisalWorkflowService workflowService,
        IValidator<SignAppraisalDto> validator,
        ILogger<AcknowledgeAppraisal> logger) : IAcknowledgeAppraisal
    {
        public async Task AcknowledgeAsync(SignAppraisalDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Appraisal), dto.Id.ToString());
            if (appraisal.Stage != AppraisalStage.EmployeeAcknowledgment)
                throw new ValidationException(nameof(dto.Id), "This appraisal is not awaiting the employee's acknowledgment.");
            if (appraisal.AcknowledgmentStatus == Dom.Entities.Core.AcknowledgmentStatus.Appealed)
                throw new ValidationException(nameof(dto.Id), "This appraisal is under appeal and cannot be acknowledged.");
            await workflowService.EnsureCanActAsync(appraisal);

            // The employee's signature routes to HR sign-off if the cycle enables it, otherwise closes/locks.
            var enableHr = await reviewCycleRepository.GetAll()
                .Where(c => c.Id == appraisal.ReviewCycleId).Select(c => c.EnableHrSignOff).FirstOrDefaultAsync();
            var nextStage = enableHr ? AppraisalStage.HrSignOff : AppraisalStage.Completed;

            appraisal.Acknowledge(dto.Signature, nextStage);
            repository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", dto.Id, "Acknowledged",
                $"Employee acknowledged and signed ({dto.Signature}); routed to {nextStage}.");
            await repository.SaveChangesAsync();
            await workflowService.SyncInstanceAsync(appraisal, "Employee acknowledged & signed");   // route to HR / complete
            logger.LogInformation("Appraisal {Id} acknowledged (next {Stage})", dto.Id, nextStage);

            // If there is no HR sign-off step, the employee's signature is terminal — notify downstream modules.
            if (nextStage == AppraisalStage.Completed)
                await AppraisalCompletion.NotifyAsync(completedHandlers, logger, appraisal.Id, appraisal.EmployeeId);
        }
    }

    public class ManagerSignAppraisal(
        IRepository<Appraisal> repository,
        IPerformanceHistoryWriter history,
        IAppraisalWorkflowService workflowService,
        IValidator<SignAppraisalDto> validator,
        ILogger<ManagerSignAppraisal> logger) : IManagerSignAppraisal
    {
        public async Task SignAsync(SignAppraisalDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Appraisal), dto.Id.ToString());
            if (appraisal.Stage < AppraisalStage.SecondLevelReview)
                throw new ValidationException(nameof(dto.Id), "The manager can sign only after completing the manager review.");
            // Counter-signature is a side action (doesn't advance the stage): gate it to the ManagerReview step's approver.
            if (!await workflowService.CanActOnStageAsync(appraisal, nameof(AppraisalStage.ManagerReview)))
                throw new ValidationException(nameof(dto.Id), "Only the direct manager can counter-sign this appraisal.");

            appraisal.ManagerSign(dto.Signature);
            await history.WriteAsync("Appraisal", dto.Id, "ManagerSigned", $"Manager signed ({dto.Signature}).");
            await repository.SaveChangesAsync();
            logger.LogInformation("Appraisal {Id} manager-signed", dto.Id);
        }
    }

    // ---- Appeal (HC143–HC144) -----------------------------------------------
    public class SubmitAppraisalAppeal(
        IRepository<AppraisalAppeal> repository,
        IRepository<Appraisal> appraisalRepository,
        IPerformanceHistoryWriter history,
        IAppraisalWorkflowService workflowService,
        IValidator<SubmitAppraisalAppealDto> validator,
        ILogger<SubmitAppraisalAppeal> logger) : ISubmitAppraisalAppeal
    {
        public async Task<Guid> SubmitAsync(SubmitAppraisalAppealDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await appraisalRepository.GetAll().FirstOrDefaultAsync(a => a.Id == dto.AppraisalId)
                ?? throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.ToString());
            if (appraisal.Stage != AppraisalStage.EmployeeAcknowledgment)
                throw new ValidationException(nameof(dto.AppraisalId), "An appraisal can be appealed only while it awaits the employee's acknowledgment.");
            await workflowService.EnsureCanActAsync(appraisal);
            if (appraisal.AcknowledgmentStatus == Dom.Entities.Core.AcknowledgmentStatus.Accepted)
                throw new ValidationException(nameof(dto.AppraisalId), "An already-accepted appraisal cannot be appealed.");
            if (await repository.GetAll().AnyAsync(x => x.AppraisalId == dto.AppraisalId
                    && (x.Status == AppealStatus.Open || x.Status == AppealStatus.UnderReview)))
                throw new ValidationException(nameof(dto.AppraisalId), "An appeal is already open for this appraisal.");

            var appeal = AppraisalAppeal.Create(dto.AppraisalId, appraisal.EmployeeId, dto.Comments, dto.RequestFollowUp);
            appraisal.MarkAppealed();   // triggers the HR/management review flow (HC144)
            await repository.AddAsync(appeal);
            appraisalRepository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", dto.AppraisalId, "Appealed",
                $"Appeal submitted{(dto.RequestFollowUp ? " (follow-up requested)" : "")}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Appeal {Id} submitted for Appraisal {AppraisalId}", appeal.Id, dto.AppraisalId);
            return appeal.Id;
        }
    }

    public class StartAppraisalAppealReview(
        IRepository<AppraisalAppeal> repository,
        IAppraisalWorkflowService workflowService,
        ILogger<StartAppraisalAppealReview> logger) : IStartAppraisalAppealReview
    {
        public async Task StartAsync(Guid id)
        {
            var appeal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(AppraisalAppeal), id.ToString());
            if (!await workflowService.CanAdministerAsync())
                throw new ValidationException(nameof(id), "Only an HR administrator can review appeals.");
            if (appeal.Status != AppealStatus.Open)
                throw new ValidationException(nameof(id), "Only an open appeal can be moved to review.");
            appeal.StartReview();
            await repository.SaveChangesAsync();
            logger.LogInformation("Appeal {Id} moved to review", id);
        }
    }

    public class ResolveAppraisalAppeal(
        IRepository<AppraisalAppeal> repository,
        IAppraisalWorkflowService workflowService,
        IPerformanceHistoryWriter history,
        IValidator<ResolveAppraisalAppealDto> validator,
        ILogger<ResolveAppraisalAppeal> logger) : IResolveAppraisalAppeal
    {
        public async Task ResolveAsync(ResolveAppraisalAppealDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appeal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(AppraisalAppeal), dto.Id.ToString());
            if (!await workflowService.CanAdministerAsync())
                throw new ValidationException(nameof(dto.Id), "Only an HR administrator can resolve appeals.");
            if (appeal.Status is AppealStatus.Resolved or AppealStatus.Rejected)
                throw new ValidationException(nameof(dto.Id), "This appeal has already been closed.");

            appeal.Resolve(dto.Upheld, dto.Resolution);
            await history.WriteAsync("Appraisal", appeal.AppraisalId, "AppealResolved",
                $"Appeal {(dto.Upheld ? "upheld" : "rejected")}: {dto.Resolution}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Appeal {Id} resolved (upheld={Upheld})", dto.Id, dto.Upheld);
        }
    }

    // ---- Reads --------------------------------------------------------------
    internal static class AppraisalAppealMapper
    {
        internal static AppraisalAppealDto Map(AppraisalAppeal x, string? employeeName) => new()
        {
            Id = x.Id,
            AppraisalId = x.AppraisalId,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            Comments = x.Comments,
            RequestFollowUp = x.RequestFollowUp,
            Status = x.Status.ToString(),
            Resolution = x.Resolution,
            ResolvedAt = x.ResolvedAt
        };
    }

    public class GetAppraisalAppealById(
        IRepository<AppraisalAppeal> repository,
        IRepository<Employee> employeeRepository,
        IAppraisalWorkflowService workflowService) : IGetAppraisalAppealById
    {
        public async Task<AppraisalAppealDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(AppraisalAppeal), id.ToString());
            // Visibility: the appealing employee or an HR administrator.
            var myEmp = await workflowService.CurrentEmployeeIdAsync();
            if (!(myEmp.HasValue && myEmp.Value == entity.EmployeeId) && !await workflowService.CanAdministerAsync())
                throw new ValidationException("access", "You do not have access to this appeal.");
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            return AppraisalAppealMapper.Map(entity, employeeName);
        }
    }

    public class GetAllAppraisalAppeals(
        IRepository<AppraisalAppeal> repository,
        IRepository<Employee> employeeRepository,
        IAppraisalWorkflowService workflowService) : IGetAllAppraisalAppeals
    {
        public async Task<PaginatedResponse<AppraisalAppealDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            // Non-admin visibility: only the caller's own appeals (empty-guid match = none for unlinked accounts).
            if (!await workflowService.CanAdministerAsync())
            {
                var myEmp = await workflowService.CurrentEmployeeIdAsync() ?? Guid.Empty;
                query = query.Where(x => x.EmployeeId == myEmp);
            }
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (request.ObjectiveId.HasValue)   // reused as appraisalId filter
                query = query.Where(x => x.AppraisalId == request.ObjectiveId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppealStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync();

            // PERFORMANCE: batch-load the employee names for the page in ONE query (was one per row).
            var empIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<AppraisalAppealDto>(rows.Count);
            foreach (var r in rows)
            {
                data.Add(AppraisalAppealMapper.Map(r, employeeNames.GetValueOrDefault(r.EmployeeId)));
            }
            return new PaginatedResponse<AppraisalAppealDto> { Total = total, Data = data };
        }
    }
}
