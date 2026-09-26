using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    // ---- DTOs ---------------------------------------------------------------

    public class ApprovalDelegationDto
    {
        public Guid Id { get; set; }
        public Guid FromEmployeeId { get; set; }
        public string? FromEmployeeName { get; set; }
        public Guid ToEmployeeId { get; set; }
        public string? ToEmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public bool AllProcesses { get; set; }
        public List<string> EntityTypes { get; set; } = [];
        public decimal? ApprovalLimit { get; set; }
        /// <summary>Derived from the dates and the revocation flag — never stored.</summary>
        public string Status { get; set; } = nameof(DelegationStatus.Scheduled);
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public string? RevocationReason { get; set; }
    }

    public class SaveApprovalDelegationDto
    {
        public Guid? Id { get; set; }
        public Guid FromEmployeeId { get; set; }
        public Guid ToEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public bool AllProcesses { get; set; } = true;
        public List<string> EntityTypes { get; set; } = [];
        public decimal? ApprovalLimit { get; set; }
    }

    public class RevokeApprovalDelegationDto
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>Whether the caller has anyone they could hand their approvals to.</summary>
    public class MyDelegationScopeDto
    {
        /// <summary>True when at least one colleague is inside their delegation scope.</summary>
        public bool CanArrangeCover { get; set; }
        /// <summary>How many — drives an honest empty state rather than a bare "no".</summary>
        public int DelegatableCount { get; set; }
    }

    public class DelegationPolicyDto
    {
        public int MinDelegateExperienceYears { get; set; }
        public int MinSalaryRatioPercent { get; set; }
        public bool RequireManagerialDelegate { get; set; }
        public int MaxDelegationDays { get; set; }
        public decimal? DefaultApprovalLimit { get; set; }
        public bool AllowSelfServiceDelegation { get; set; }
        /// <summary>Confine delegates to the approver's own department and those beneath it.</summary>
        public bool RestrictToOwnDepartment { get; set; }
    }

    /// <summary>The eligibility answer, for the "can this person stand in?" preview on the form.</summary>
    public class DelegationEligibilityDto
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = [];
        public decimal DelegateExperienceYears { get; set; }
        public decimal? SalaryRatioPercent { get; set; }
    }

    public class SaveApprovalDelegationDtoValidator : AbstractValidator<SaveApprovalDelegationDto>
    {
        public SaveApprovalDelegationDtoValidator()
        {
            RuleFor(x => x.FromEmployeeId).NotEmpty();
            RuleFor(x => x.ToEmployeeId).NotEmpty()
                .NotEqual(x => x.FromEmployeeId).WithMessage("An approver cannot delegate to themselves.");
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty()
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("The delegation cannot end before it starts.");
            RuleFor(x => x.Reason).MaximumLength(1000);
            RuleFor(x => x.ApprovalLimit).GreaterThanOrEqualTo(0).When(x => x.ApprovalLimit.HasValue);
            RuleFor(x => x.EntityTypes)
                .Must(t => t.Count > 0)
                .When(x => !x.AllProcesses)
                .WithMessage("Select at least one process to delegate, or delegate all processes.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface ISaveApprovalDelegation { Task<Guid> SaveAsync(SaveApprovalDelegationDto dto); }
    public interface IRevokeApprovalDelegation { Task RevokeAsync(RevokeApprovalDelegationDto dto); }
    public interface IGetApprovalDelegations { Task<PaginatedResponse<ApprovalDelegationDto>> GetAsync(GetAllRequest request); }
    public interface IGetMyDelegations { Task<List<ApprovalDelegationDto>> GetAsync(); }
    public interface ICheckDelegationEligibility { Task<DelegationEligibilityDto> CheckAsync(Guid fromEmployeeId, Guid toEmployeeId); }
    public interface IGetDelegationPolicy { Task<DelegationPolicyDto> GetAsync(); }
    public interface IGetMyDelegationScope { Task<MyDelegationScopeDto> GetAsync(); }
    public interface ISaveDelegationPolicy { Task SaveAsync(DelegationPolicyDto dto); }

    // ---- Save ---------------------------------------------------------------

    /// <summary>
    /// Creates or amends a delegation, after the seniority rules have had their say.
    /// </summary>
    public class SaveApprovalDelegation(
        IRepository<ApprovalDelegation> repository,
        IRepository<Employee> employees,
        IRepository<User> users,
        IDelegationEligibilityService eligibility,
        IDelegationScopeService scope,
        IPortalNotifier portalNotifier,
        ICurrentUserService currentUser,
        IValidator<SaveApprovalDelegationDto> validator,
        ILogger<SaveApprovalDelegation> logger) : ISaveApprovalDelegation
    {
        public async Task<Guid> SaveAsync(SaveApprovalDelegationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var policy = await eligibility.GetPolicyAsync();

            // ---- Who is allowed to set this up at all? ------------------------------------
            var myEmployeeId = await MyEmployeeIdAsync();
            var isOwnAuthority = myEmployeeId.HasValue && myEmployeeId.Value == dto.FromEmployeeId;
            var isHrAdmin = currentUser.IsHeadOffice();

            if (!isOwnAuthority && !isHrAdmin)
                throw new ValidationException(nameof(dto.FromEmployeeId),
                    "You can only delegate your own approval authority.");
            if (isOwnAuthority && !policy.AllowSelfServiceDelegation && !isHrAdmin)
                throw new ValidationException(nameof(dto.FromEmployeeId),
                    "Self-service delegation is switched off — ask HR to set this up.");

            // ---- Window length -------------------------------------------------------------
            if (policy.MaxDelegationDays > 0)
            {
                var days = (dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;
                if (days > policy.MaxDelegationDays)
                    throw new ValidationException(nameof(dto.EndDate),
                        $"A delegation may run for at most {policy.MaxDelegationDays} day(s); this one is {days:0}.");
            }

            // ---- Department scope -------------------------------------------------------------
            // ⚠️ A department head may only hand their authority to somebody inside their own
            // branch of the org chart. Enforced HERE and not only in the picker: the picker is
            // scoped for convenience, but a scope that exists only in the browser is a suggestion.
            // HR is exempt — administering other people's delegations is the job.
            if (policy.RestrictToOwnDepartment && !isHrAdmin
                && !await scope.CanDelegateToAsync(dto.FromEmployeeId, dto.ToEmployeeId))
                throw new ValidationException(nameof(dto.ToEmployeeId),
                    "You can only delegate to someone in your own department or a department beneath it.");

            // ---- Seniority: experience + salary --------------------------------------------
            var verdict = await eligibility.EvaluateAsync(dto.FromEmployeeId, dto.ToEmployeeId);
            if (!verdict.IsEligible)
                throw new ValidationException(nameof(dto.ToEmployeeId), string.Join(" ", verdict.Reasons));

            // ⚠️ NO ONWARD DELEGATION. Somebody who is currently standing in for another approver
            // cannot hand that authority on: the chain would move real approval rights to a person
            // nobody chose, and every individual record in it would look reasonable.
            var today = DateTime.UtcNow.Date;
            var delegatorIsThemselvesADelegate = await repository.GetAll().AsNoTracking()
                .AnyAsync(d => d.ToEmployeeId == dto.FromEmployeeId && !d.IsRevoked
                               && d.StartDate <= dto.EndDate.Date && d.EndDate >= dto.StartDate.Date);
            if (delegatorIsThemselvesADelegate)
                throw new ValidationException(nameof(dto.FromEmployeeId),
                    "This approver is already standing in for somebody else over part of that period. "
                    + "Authority received through a delegation cannot be delegated onward.");

            var limit = dto.ApprovalLimit ?? policy.DefaultApprovalLimit;

            ApprovalDelegation entity;
            var isAmendment = dto.Id is Guid existing && existing != Guid.Empty;
            if (dto.Id is Guid id && id != Guid.Empty)
            {
                entity = await repository.GetAll().Include(d => d.Scopes).FirstOrDefaultAsync(d => d.Id == id)
                    ?? throw new NotFoundException(nameof(ApprovalDelegation), id.ToString());
                if (entity.IsRevoked)
                    throw new ValidationException(nameof(dto.Id), "A revoked delegation can no longer be changed.");

                try
                {
                    entity.Update(dto.StartDate, dto.EndDate, dto.Reason, dto.AllProcesses, dto.EntityTypes, limit);
                }
                catch (ArgumentException ex) { throw new ValidationException(nameof(dto.EndDate), ex.Message); }
                repository.UpdateAsync(entity);
            }
            else
            {
                if (!await employees.GetAll().AnyAsync(e => e.Id == dto.FromEmployeeId))
                    throw new NotFoundException(nameof(Employee), dto.FromEmployeeId.ToString());
                if (!await employees.GetAll().AnyAsync(e => e.Id == dto.ToEmployeeId))
                    throw new NotFoundException(nameof(Employee), dto.ToEmployeeId.ToString());

                try
                {
                    entity = ApprovalDelegation.Create(dto.FromEmployeeId, dto.ToEmployeeId,
                        dto.StartDate, dto.EndDate, dto.Reason, dto.AllProcesses, dto.EntityTypes, limit);
                }
                catch (ArgumentException ex) { throw new ValidationException(nameof(dto.ToEmployeeId), ex.Message); }
                await repository.AddAsync(entity);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation(
                "Delegation {Id}: {From} -> {To} {Start:yyyy-MM-dd}..{End:yyyy-MM-dd} limit {Limit}",
                entity.Id, dto.FromEmployeeId, dto.ToEmployeeId, dto.StartDate, dto.EndDate, limit);

            await NotifyDelegateAsync(entity, isAmendment);
            return entity.Id;
        }

        private async Task<Guid?> MyEmployeeIdAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return null;
            return await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tell the delegate, in the portal, that they are now standing in for somebody.
        /// </summary>
        /// <remarks>
        /// <para>⚠️ A delegation used to take effect in complete silence. Approvals simply began
        /// appearing in somebody's queue on another person's authority, and the one person best
        /// placed to notice that a delegation is wider than intended — the delegate — was the only
        /// one never told it existed.</para>
        ///
        /// <para>An AMENDMENT notifies too. Quietly widening somebody's authority, or moving its
        /// dates, is the same problem as granting it silently; the wording distinguishes the two so
        /// the alert does not claim to be news when it is a change.</para>
        ///
        /// <para>Severity tracks whether it is live: a delegation that starts TODAY may already
        /// have requests waiting, which is an Action; one scheduled for next month is Info. Both are
        /// worth saying, but only one of them is worth interrupting somebody for.</para>
        ///
        /// <para>Best-effort, like every other portal alert in this codebase — a notification
        /// failure must never undo a delegation that is already saved and legally in force.</para>
        /// </remarks>
        private async Task NotifyDelegateAsync(ApprovalDelegation delegation, bool isAmendment)
        {
            try
            {
                var recipients = await users.GetAll().AsNoTracking()
                    .Where(u => u.EmployeeId == delegation.ToEmployeeId)
                    .Select(u => u.Id)
                    .ToListAsync();
                if (recipients.Count == 0)
                {
                    // An employee with no login cannot be told, and cannot act either. Worth a line
                    // in the log: the delegation is valid but nothing will reach them.
                    logger.LogInformation(
                        "Delegation {Id}: delegate {EmployeeId} has no user account — no portal alert raised.",
                        delegation.Id, delegation.ToEmployeeId);
                    return;
                }

                var approverName = await employees.GetAll().AsNoTracking()
                    .Where(e => e.Id == delegation.FromEmployeeId)
                    .Select(e => e.Person != null
                        ? e.Person.FirstName + " " + e.Person.GrandFatherName
                        : e.EmployeeNumber)
                    .FirstOrDefaultAsync();
                approverName = string.IsNullOrWhiteSpace(approverName) ? "a colleague" : approverName.Trim();

                var live = delegation.IsEffectiveOn(DateTime.UtcNow.Date);
                var covers = delegation.AllProcesses
                    ? "all request types"
                    : string.Join(", ", delegation.Scopes.Select(s => s.EntityType));
                var ceiling = delegation.ApprovalLimit is decimal cap
                    ? $", up to {cap:N0}"
                    : string.Empty;

                var title = isAmendment
                    ? $"Your delegation from {approverName} has changed"
                    : $"You are standing in for {approverName}";
                var body =
                    $"{delegation.StartDate:yyyy-MM-dd} to {delegation.EndDate:yyyy-MM-dd} — {covers}{ceiling}. "
                    + (live
                        ? "Their requests now appear in your approvals, marked as acting on their behalf."
                        : "Their requests will appear in your approvals once it starts.");

                await portalNotifier.NotifyUsersAsync(
                    recipients, title, body,
                    // The portal screen, where they can see exactly what they were given.
                    "/myDelegations",
                    live ? "Action" : "Info",
                    nameof(ApprovalDelegation),
                    delegation.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Delegation {Id}: failed to alert the delegate", delegation.Id);
            }
        }
    }

    // ---- Revoke -------------------------------------------------------------

    public class RevokeApprovalDelegation(
        IRepository<ApprovalDelegation> repository,
        IRepository<User> users,
        IRepository<Employee> employees,
        IPortalNotifier portalNotifier,
        ICurrentUserService currentUser,
        ILogger<RevokeApprovalDelegation> logger) : IRevokeApprovalDelegation
    {
        public async Task RevokeAsync(RevokeApprovalDelegationDto dto)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == dto.Id)
                ?? throw new NotFoundException(nameof(ApprovalDelegation), dto.Id.ToString());

            var userId = currentUser.GetCurrentUserId();
            var myEmployeeId = userId is null ? null : await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();

            // The delegator may always withdraw their own; HR may withdraw anyone's. The DELEGATE
            // cannot — dropping authority you were given is a conversation, not a button.
            if (!(currentUser.IsHeadOffice() || (myEmployeeId.HasValue && myEmployeeId.Value == entity.FromEmployeeId)))
                throw new ValidationException("id", "Only the delegating approver or HR can withdraw a delegation.");

            // Who ended it changes what the delegate should be told: their own approver taking
            // the cover back is ordinary, HR removing it behind both their backs is not.
            var endedByDelegator = myEmployeeId.HasValue && myEmployeeId.Value == entity.FromEmployeeId;

            entity.Revoke(currentUser.GetCurrentUserName(), dto.Reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Delegation {Id} revoked", dto.Id);

            // ⚠️ ORDER MATTERS. Clear the "you are standing in for X" alert FIRST, then raise the
            // ending one — ResolveAsync marks every unread alert for this delegation read, so
            // raising the new one first would immediately mark it read and the delegate would never
            // see it.
            //
            // The clear on its own is not enough. An alert that outlives the authority it announced
            // is worse than never having sent one — the delegate is left believing they are
            // covering, and requests sit waiting for somebody who can no longer act — but silently
            // withdrawing the alert leaves the same belief, just without the evidence. Say it.
            //
            // Best-effort throughout: the withdrawal itself is already committed and must stand
            // whatever the portal does.
            try
            {
                await portalNotifier.ResolveAsync(nameof(ApprovalDelegation), entity.Id);
                await NotifyDelegateOfEndAsync(entity, endedByDelegator, dto.Reason);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Delegation {Id}: failed to update the delegate's portal alerts", entity.Id);
            }
        }

        /// <summary>
        /// Tell the delegate their cover has ended.
        /// </summary>
        /// <remarks>
        /// <para>The counterpart of the grant alert. Clearing the old one stops the delegate seeing
        /// a claim that is no longer true, but it does not tell them anything — they would simply
        /// find, at some point, that requests had stopped arriving. Somebody who believes they are
        /// covering a colleague needs to hear that they are not.</para>
        ///
        /// <para>⚠️ Info, never Action. Nothing is being asked of them; the whole content is that
        /// they can stop watching. An alert that interrupts somebody to tell them they have less to
        /// do has misjudged what interrupting is for.</para>
        ///
        /// <para>The reason is included when one was given, and WHO ended it changes the wording —
        /// their own approver taking the cover back is ordinary; HR removing it, possibly without
        /// either party asking, is worth naming as different.</para>
        /// </remarks>
        private async Task NotifyDelegateOfEndAsync(
            ApprovalDelegation delegation, bool endedByDelegator, string? reason)
        {
            var recipients = await users.GetAll().AsNoTracking()
                .Where(u => u.EmployeeId == delegation.ToEmployeeId)
                .Select(u => u.Id)
                .ToListAsync();
            if (recipients.Count == 0) return;

            var approverName = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == delegation.FromEmployeeId)
                .Select(e => e.Person != null
                    ? e.Person.FirstName + " " + e.Person.GrandFatherName
                    : e.EmployeeNumber)
                .FirstOrDefaultAsync();
            approverName = string.IsNullOrWhiteSpace(approverName) ? "a colleague" : approverName.Trim();

            var title = endedByDelegator
                ? $"{approverName} has ended your cover"
                : $"Your cover for {approverName} has been withdrawn";
            var body = "Their requests no longer appear in your approvals."
                + (string.IsNullOrWhiteSpace(reason) ? string.Empty : $" Reason: {reason.Trim()}");

            await portalNotifier.NotifyUsersAsync(
                recipients, title, body, "/myDelegations",
                // Info, not Action: there is nothing left for them to do.
                "Info", nameof(ApprovalDelegation), delegation.Id);
        }
    }

    // ---- Reads --------------------------------------------------------------


    internal static class DelegationMapper
    {
        internal static ApprovalDelegationDto ToDto(ApprovalDelegation d, DateTime today,
            IReadOnlyDictionary<Guid, string> names) => new()
            {
                Id = d.Id,
                FromEmployeeId = d.FromEmployeeId,
                FromEmployeeName = names.TryGetValue(d.FromEmployeeId, out var f) ? f : null,
                ToEmployeeId = d.ToEmployeeId,
                ToEmployeeName = names.TryGetValue(d.ToEmployeeId, out var t) ? t : null,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Reason = d.Reason,
                AllProcesses = d.AllProcesses,
                EntityTypes = [.. d.Scopes.Select(s => s.EntityType)],
                ApprovalLimit = d.ApprovalLimit,
                Status = d.StatusOn(today).ToString(),
                RevokedAt = d.RevokedAt,
                RevokedBy = d.RevokedBy,
                RevocationReason = d.RevocationReason
            };

        /// <summary>Employee id -> display name, in one query for the whole page.</summary>
        internal static async Task<Dictionary<Guid, string>> NamesAsync(
            IRepository<Employee> employees, IEnumerable<Guid> ids)
        {
            var list = ids.Distinct().ToList();
            if (list.Count == 0) return [];
            return await employees.GetAll().AsNoTracking()
                .Where(e => list.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    Name = e.Person != null
                        ? e.Person.FirstName + " " + e.Person.GrandFatherName
                        : e.EmployeeNumber
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name.Trim());
        }
    }

    public class GetApprovalDelegations(
        IRepository<ApprovalDelegation> repository,
        IRepository<Employee> employees) : IGetApprovalDelegations
    {
        public async Task<PaginatedResponse<ApprovalDelegationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            IQueryable<ApprovalDelegation> query = repository.GetAll().AsNoTracking().Include(d => d.Scopes);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(d => d.Reason != null && d.Reason.Contains(term));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(d => d.StartDate)
                .Skip(skip).Take(take)
                .ToListAsync();

            var names = await DelegationMapper.NamesAsync(employees,
                rows.SelectMany(r => new[] { r.FromEmployeeId, r.ToEmployeeId }));
            var today = DateTime.UtcNow.Date;

            return new PaginatedResponse<ApprovalDelegationDto>
            {
                Total = total,
                Data = [.. rows.Select(r => DelegationMapper.ToDto(r, today, names))]
            };
        }
    }

    /// <summary>Both directions for the signed-in user: what they lent, and what they hold.</summary>
    public class GetMyDelegations(
        IRepository<ApprovalDelegation> repository,
        IRepository<Employee> employees,
        IRepository<User> users,
        ICurrentUserService currentUser) : IGetMyDelegations
    {
        public async Task<List<ApprovalDelegationDto>> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return [];
            var me = await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
            if (me is null) return [];

            var rows = await repository.GetAll().AsNoTracking().Include(d => d.Scopes)
                .Where(d => d.FromEmployeeId == me.Value || d.ToEmployeeId == me.Value)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync();

            var names = await DelegationMapper.NamesAsync(employees,
                rows.SelectMany(r => new[] { r.FromEmployeeId, r.ToEmployeeId }));
            var today = DateTime.UtcNow.Date;
            return [.. rows.Select(r => DelegationMapper.ToDto(r, today, names))];
        }
    }

    /// <summary>
    /// The live "may this person stand in?" check behind the delegation form.
    /// </summary>
    /// <remarks>
    /// ⚠️ SCOPED TO THE CALLER'S OWN AUTHORITY. The endpoint is <c>[SelfScoped]</c>, which exempts
    /// it from the controller's permission gate on the understanding that the HANDLER confines the
    /// answer to the caller — and this one did not. It took <c>fromEmployeeId</c> from the query
    /// string and answered for ANY pair, so a signed-in employee could walk employee ids and read
    /// back "the delegate's salary is 143% of the approver's" for colleagues whose pay they have no
    /// business knowing. Non-HR callers may now only ask about their own authority.
    /// </remarks>
    public class CheckDelegationEligibility(
        IDelegationEligibilityService eligibility,
        IRepository<User> users,
        ICurrentUserService currentUser) : ICheckDelegationEligibility
    {
        public async Task<DelegationEligibilityDto> CheckAsync(Guid fromEmployeeId, Guid toEmployeeId)
        {
            if (!currentUser.IsHeadOffice())
            {
                var userId = currentUser.GetCurrentUserId();
                var me = userId is null ? null : await users.GetAll().AsNoTracking()
                    .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
                if (me is null || me.Value != fromEmployeeId)
                    throw new ValidationException(nameof(fromEmployeeId),
                        "You can only check delegates for your own approval authority.");
            }

            var r = await eligibility.EvaluateAsync(fromEmployeeId, toEmployeeId);
            return new DelegationEligibilityDto
            {
                IsEligible = r.IsEligible,
                Reasons = [.. r.Reasons],
                DelegateExperienceYears = r.DelegateExperienceYears,
                SalaryRatioPercent = r.SalaryRatioPercent
            };
        }
    }

    /// <summary>
    /// "Could I arrange cover at all?" — the portal sidebar's visibility probe.
    /// </summary>
    /// <remarks>
    /// The Home sidebar hides an item whose screen would be empty, so it needs a cheap yes/no
    /// before rendering the entry. A department head manages a unit and gets people; an employee
    /// who manages nothing and sits alone in their unit gets none, and never sees the menu item.
    /// </remarks>
    public class GetMyDelegationScope(
        IDelegationScopeService scope,
        IRepository<User> users,
        ICurrentUserService currentUser) : IGetMyDelegationScope
    {
        public async Task<MyDelegationScopeDto> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return new MyDelegationScopeDto();
            var me = await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
            if (me is null) return new MyDelegationScopeDto();

            var ids = await scope.DelegatableEmployeeIdsAsync(me.Value);
            return new MyDelegationScopeDto { CanArrangeCover = ids.Count > 0, DelegatableCount = ids.Count };
        }
    }

    public class GetDelegationPolicy(IDelegationEligibilityService eligibility) : IGetDelegationPolicy
    {
        public async Task<DelegationPolicyDto> GetAsync()
        {
            var p = await eligibility.GetPolicyAsync();
            return new DelegationPolicyDto
            {
                MinDelegateExperienceYears = p.MinDelegateExperienceYears,
                MinSalaryRatioPercent = p.MinSalaryRatioPercent,
                RequireManagerialDelegate = p.RequireManagerialDelegate,
                MaxDelegationDays = p.MaxDelegationDays,
                DefaultApprovalLimit = p.DefaultApprovalLimit,
                AllowSelfServiceDelegation = p.AllowSelfServiceDelegation,
                RestrictToOwnDepartment = p.RestrictToOwnDepartment
            };
        }
    }

    public class SaveDelegationPolicy(
        IRepository<DelegationPolicy> repository,
        IDelegationEligibilityService eligibility,
        ILogger<SaveDelegationPolicy> logger) : ISaveDelegationPolicy
    {
        public async Task SaveAsync(DelegationPolicyDto dto)
        {
            // Ensure the singleton exists, then re-read it TRACKED so the update is persisted.
            await eligibility.GetPolicyAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(DelegationPolicy), "singleton");

            try
            {
                entity.Update(dto.MinDelegateExperienceYears, dto.MinSalaryRatioPercent,
                    dto.RequireManagerialDelegate, dto.MaxDelegationDays, dto.DefaultApprovalLimit,
                    dto.AllowSelfServiceDelegation, dto.RestrictToOwnDepartment);
            }
            catch (ArgumentException ex) { throw new ValidationException("policy", ex.Message); }

            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Delegation policy updated");
        }
    }
}
