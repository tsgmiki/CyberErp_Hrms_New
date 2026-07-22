using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class RewardNominationDto
    {
        public Guid Id { get; set; }
        public Guid NomineeEmployeeId { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeNumber { get; set; }
        public Guid RecognitionBadgeId { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeColor { get; set; }
        public string? BadgeIcon { get; set; }
        public string? RewardKind { get; set; }
        public decimal? MonetaryValue { get; set; }
        public int PointsValue { get; set; }
        public Guid? RecognitionProgramId { get; set; }
        public string? ProgramName { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? NominatedByEmployeeId { get; set; }
        public string? NominatedByName { get; set; }
        public DateTime NominatedOn { get; set; }
        public DateTime? DecidedOn { get; set; }
        public Guid? GrantedRecognitionId { get; set; }
    }

    public class SaveRewardNominationDto
    {
        public Guid? Id { get; set; }
        public Guid NomineeEmployeeId { get; set; }
        public Guid RecognitionBadgeId { get; set; }
        public Guid? RecognitionProgramId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class SaveRewardNominationDtoValidator : AbstractValidator<SaveRewardNominationDto>
    {
        public SaveRewardNominationDtoValidator()
        {
            RuleFor(x => x.NomineeEmployeeId).NotEmpty();
            RuleFor(x => x.RecognitionBadgeId).NotEmpty();
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveRewardNomination { Task<Guid> SaveAsync(SaveRewardNominationDto dto); }
    public interface IDeleteRewardNomination { Task DeleteAsync(Guid id); }
    public interface IGetRewardNominationById { Task<RewardNominationDto> GetAsync(Guid id); }
    public interface IGetAllRewardNominations { Task<PaginatedResponse<RewardNominationDto>> GetAsync(GetAllRequest request); }

    /// <summary>Applies a nomination's outcome — called by the workflow handler or (no workflow) by HR directly.</summary>
    public interface IApproveRewardNomination
    {
        Task ApproveAsync(Guid id);
        Task RejectAsync(Guid id);
    }

    /// <summary>
    /// Side effects every recognition grant carries (HC180/HC185): credit the badge's points to the
    /// employee's ledger and raise the payroll hand-off row for monetary awards. Rows are explicitly
    /// tenant-stamped from the recognition so background/workflow callers stay tenant-correct.
    /// </summary>
    internal static class RewardGrantShared
    {
        internal static async Task ApplyGrantSideEffectsAsync(
            EmployeeRecognition recognition,
            RecognitionBadge badge,
            IRepository<RewardPointsTransaction> pointsRepository,
            IRepository<RewardDisbursement> disbursementRepository)
        {
            if (badge.PointsValue > 0)
            {
                var txn = RewardPointsTransaction.Create(recognition.EmployeeId, badge.PointsValue,
                    RewardPointsSource.Recognition, DateTime.UtcNow.Date, recognition.Id, $"Awarded: {badge.Name}");
                if (string.IsNullOrEmpty(txn.TenantId)) txn.TenantId = recognition.TenantId;
                await pointsRepository.AddAsync(txn);
            }

            if (badge.MonetaryValue is > 0 && badge.RewardKind is RewardKind.GiftCard or RewardKind.MonetaryBonus)
            {
                var disbursement = RewardDisbursement.Create(recognition.EmployeeId, badge.MonetaryValue.Value,
                    badge.Id, recognition.Id);
                if (string.IsNullOrEmpty(disbursement.TenantId)) disbursement.TenantId = recognition.TenantId;
                await disbursementRepository.AddAsync(disbursement);
            }
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveRewardNomination(
        IRepository<RewardNomination> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<RecognitionProgram> programRepository,
        IPerformanceVisibilityService visibility,
        Employees.IDisciplinaryEligibilityService disciplineEligibility,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IPerformanceHistoryWriter history,
        IValidator<SaveRewardNominationDto> validator,
        ILogger<SaveRewardNomination> logger) : ISaveRewardNomination
    {
        public async Task<Guid> SaveAsync(SaveRewardNominationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC179 — managers nominate within their unit subtree, HR anywhere; never yourself.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && !scope.IsManager)
                throw new ValidationException(nameof(dto.NomineeEmployeeId), "Only managers or HR can raise nominations.");
            if (scope.EmployeeId.HasValue && scope.EmployeeId.Value == dto.NomineeEmployeeId)
                throw new ValidationException(nameof(dto.NomineeEmployeeId), "You cannot nominate yourself for an award.");
            if (!await visibility.CanAccessEmployeeAsync(dto.NomineeEmployeeId))
                throw new ValidationException(nameof(dto.NomineeEmployeeId), "The nominee is outside your scope.");

            // HC224/HC225 — an active disciplinary measure flagged AffectsReward blocks the nomination.
            await disciplineEligibility.EnsureEligibleForRewardAsync(dto.NomineeEmployeeId);

            var nominee = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => e.Id == dto.NomineeEmployeeId)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeNumber,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.NomineeEmployeeId.ToString());

            var badge = await badgeRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == dto.RecognitionBadgeId)
                ?? throw new NotFoundException(nameof(RecognitionBadge), dto.RecognitionBadgeId.ToString());
            if (!badge.IsActive)
                throw new ValidationException(nameof(dto.RecognitionBadgeId), "The selected award is inactive.");

            if (dto.RecognitionProgramId.HasValue)
            {
                var program = await programRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == dto.RecognitionProgramId.Value)
                    ?? throw new NotFoundException(nameof(RecognitionProgram), dto.RecognitionProgramId.Value.ToString());
                if (!program.IsActive)
                    throw new ValidationException(nameof(dto.RecognitionProgramId), "The selected program is inactive.");
                if (program.RecognitionBadgeId.HasValue && program.RecognitionBadgeId.Value != dto.RecognitionBadgeId)
                    throw new ValidationException(nameof(dto.RecognitionBadgeId),
                        "The program awards a different badge than the one selected.");
            }

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(RewardNomination), dto.Id.Value.ToString());
                if (!scope.IsAdmin && entity.NominatedByEmployeeId != scope.EmployeeId)
                    throw new ValidationException(nameof(dto.Id), "Only the nominator or HR can edit a nomination.");
                if (entity.Status != NominationStatus.Pending)
                    throw new ValidationException(nameof(dto.Id), "Only a pending nomination can be edited.");
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.RewardNomination, entity.Id);

                entity.UpdateRequest(dto.NomineeEmployeeId, dto.RecognitionBadgeId, dto.RecognitionProgramId, dto.Reason);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated RewardNomination {Id}", entity.Id);
                return entity.Id;
            }

            var created = RewardNomination.Create(dto.NomineeEmployeeId, dto.RecognitionBadgeId,
                dto.RecognitionProgramId, dto.Reason, scope.EmployeeId, DateTime.UtcNow.Date);
            await repository.AddAsync(created);
            await history.WriteAsync("RewardNomination", created.Id, "Nominated",
                $"{nominee.Name} nominated for \"{badge.Name}\".");
            await repository.SaveChangesAsync();

            // Route through the approval chain when one is configured (HC186); without a
            // definition the module operates directly (HR approves from the list).
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.RewardNomination, created.Id, dto.NomineeEmployeeId,
                $"Reward nomination — {nominee.Name} ({badge.Name})");

            logger.LogInformation("Created RewardNomination {Id} for employee {Employee}", created.Id, nominee.Id);
            return created.Id;
        }
    }

    public class ApproveRewardNomination(
        IRepository<RewardNomination> repository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<EmployeeRecognition> recognitionRepository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IRepository<RewardDisbursement> disbursementRepository,
        Employees.IDisciplinaryEligibilityService disciplineEligibility,
        IPerformanceHistoryWriter history,
        ILogger<ApproveRewardNomination> logger) : IApproveRewardNomination
    {
        public async Task ApproveAsync(Guid id)
        {
            var nomination = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(RewardNomination), id.ToString());
            if (nomination.Status == NominationStatus.Approved) return; // already granted — idempotent
            if (nomination.Status == NominationStatus.Rejected)
                throw new ValidationException(nameof(id), "A rejected nomination cannot be approved.");

            // HC224/HC225 — re-check at grant time: a disciplinary measure flagged AffectsReward
            // raised after the nomination still blocks the actual award.
            await disciplineEligibility.EnsureEligibleForRewardAsync(nomination.NomineeEmployeeId);

            var badge = await badgeRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == nomination.RecognitionBadgeId)
                ?? throw new NotFoundException(nameof(RecognitionBadge), nomination.RecognitionBadgeId.ToString());

            // The SourceRef doubles as the idempotency key: one grant per nomination, ever.
            var sourceRef = $"Nomination:{nomination.Id}";
            var recognition = await recognitionRepository.GetAll().FirstOrDefaultAsync(r => r.SourceRef == sourceRef);
            if (recognition is null)
            {
                recognition = EmployeeRecognition.Create(nomination.NomineeEmployeeId, nomination.RecognitionBadgeId,
                    nomination.Reason, DateTime.UtcNow.Date, isPublic: true, sourceRef: sourceRef);
                if (string.IsNullOrEmpty(recognition.TenantId)) recognition.TenantId = nomination.TenantId;
                await recognitionRepository.AddAsync(recognition);
                await RewardGrantShared.ApplyGrantSideEffectsAsync(recognition, badge, pointsRepository, disbursementRepository);
            }

            nomination.MarkApproved(recognition.Id, DateTime.UtcNow.Date);
            repository.UpdateAsync(nomination);
            await history.WriteAsync("RewardNomination", nomination.Id, "Approved",
                $"Nomination approved — \"{badge.Name}\" granted.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Approved RewardNomination {Id} → recognition {RecognitionId}", id, recognition.Id);
        }

        public async Task RejectAsync(Guid id)
        {
            var nomination = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(RewardNomination), id.ToString());
            if (nomination.Status == NominationStatus.Rejected) return;

            nomination.MarkRejected(DateTime.UtcNow.Date);
            repository.UpdateAsync(nomination);
            await history.WriteAsync("RewardNomination", nomination.Id, "Rejected", "Nomination rejected.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Rejected RewardNomination {Id}", id);
        }
    }

    public class DeleteRewardNomination(
        IRepository<RewardNomination> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate,
        ILogger<DeleteRewardNomination> logger) : IDeleteRewardNomination
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(RewardNomination), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && entity.NominatedByEmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only the nominator or HR can withdraw a nomination.");
            if (entity.Status != NominationStatus.Pending)
                throw new ValidationException(nameof(id), "Only a pending nomination can be withdrawn.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.RewardNomination, entity.Id);

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted RewardNomination {Id}", id);
        }
    }

    internal static class RewardNominationQueryShared
    {
        internal static IQueryable<RewardNominationDto> Project(
            IQueryable<RewardNomination> query,
            IQueryable<Employee> employees,
            IQueryable<RecognitionBadge> badges,
            IQueryable<RecognitionProgram> programs)
        {
            return query.Select(n => new RewardNominationDto
            {
                Id = n.Id,
                NomineeEmployeeId = n.NomineeEmployeeId,
                NomineeName = employees.Where(e => e.Id == n.NomineeEmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                NomineeNumber = employees.Where(e => e.Id == n.NomineeEmployeeId)
                    .Select(e => e.EmployeeNumber).FirstOrDefault(),
                RecognitionBadgeId = n.RecognitionBadgeId,
                BadgeName = badges.Where(b => b.Id == n.RecognitionBadgeId).Select(b => b.Name).FirstOrDefault(),
                BadgeColor = badges.Where(b => b.Id == n.RecognitionBadgeId).Select(b => b.Color).FirstOrDefault(),
                BadgeIcon = badges.Where(b => b.Id == n.RecognitionBadgeId).Select(b => b.Icon).FirstOrDefault(),
                RewardKind = badges.Where(b => b.Id == n.RecognitionBadgeId)
                    .Select(b => b.RewardKind.ToString()).FirstOrDefault(),
                MonetaryValue = badges.Where(b => b.Id == n.RecognitionBadgeId)
                    .Select(b => b.MonetaryValue).FirstOrDefault(),
                PointsValue = badges.Where(b => b.Id == n.RecognitionBadgeId)
                    .Select(b => b.PointsValue).FirstOrDefault(),
                RecognitionProgramId = n.RecognitionProgramId,
                ProgramName = programs.Where(p => p.Id == n.RecognitionProgramId).Select(p => p.Name).FirstOrDefault(),
                Reason = n.Reason,
                Status = n.Status.ToString(),
                NominatedByEmployeeId = n.NominatedByEmployeeId,
                NominatedByName = employees.Where(e => e.Id == n.NominatedByEmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                NominatedOn = n.NominatedOn,
                DecidedOn = n.DecidedOn,
                GrantedRecognitionId = n.GrantedRecognitionId
            });
        }
    }

    public class GetRewardNominationById(
        IRepository<RewardNomination> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<RecognitionProgram> programRepository,
        IPerformanceVisibilityService visibility) : IGetRewardNominationById
    {
        public async Task<RewardNominationDto> GetAsync(Guid id)
        {
            var dto = await RewardNominationQueryShared.Project(
                    repository.GetAll().AsNoTracking().Where(x => x.Id == id),
                    employeeRepository.GetAll(), badgeRepository.GetAll(), programRepository.GetAll())
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(RewardNomination), id.ToString());

            var scope = await visibility.GetScopeAsync();
            var allowed = scope.IsAdmin
                || dto.NomineeEmployeeId == scope.EmployeeId
                || dto.NominatedByEmployeeId == scope.EmployeeId
                || await visibility.CanAccessEmployeeAsync(dto.NomineeEmployeeId);
            if (!allowed)
                throw new ValidationException(nameof(id), "You do not have access to this nomination.");
            return dto;
        }
    }

    public class GetAllRewardNominations(
        IRepository<RewardNomination> repository,
        IRepository<Employee> employeeRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<RecognitionProgram> programRepository,
        IPerformanceVisibilityService visibility) : IGetAllRewardNominations
    {
        public async Task<PaginatedResponse<RewardNominationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // Role scoping as a single SQL predicate: HR sees all, a manager their subtree's nominees
            // plus nominations they raised, an employee only nominations naming them.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var emps = employeeRepository.GetAll();
                    var unitIds = scope.UnitIds;
                    query = query.Where(x => x.NominatedByEmployeeId == myEmp
                        || emps.Any(e => e.Id == x.NomineeEmployeeId && e.Position != null
                            && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(x => x.NomineeEmployeeId == myEmp);
                }
            }

            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.NomineeEmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<NominationStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                var emps = employeeRepository.GetAll();
                query = query.Where(x => x.Reason.Contains(term)
                    || emps.Any(e => e.Id == x.NomineeEmployeeId && (e.EmployeeNumber.Contains(term)
                        || (e.Person != null && (e.Person.FirstName.Contains(term) || e.Person.GrandFatherName.Contains(term))))));
            }

            var total = await query.CountAsync();
            var data = await RewardNominationQueryShared.Project(
                    query.OrderByDescending(x => x.NominatedOn).ThenByDescending(x => x.CreatedAt).Skip(skip).Take(take),
                    employeeRepository.GetAll(), badgeRepository.GetAll(), programRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<RewardNominationDto> { Total = total, Data = data };
        }
    }
}
