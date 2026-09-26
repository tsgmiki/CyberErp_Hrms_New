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

    public class DelegationPolicyDto
    {
        public int MinDelegateExperienceYears { get; set; }
        public int MinSalaryRatioPercent { get; set; }
        public bool RequireManagerialDelegate { get; set; }
        public int MaxDelegationDays { get; set; }
        public decimal? DefaultApprovalLimit { get; set; }
        public bool AllowSelfServiceDelegation { get; set; }
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
            return entity.Id;
        }

        private async Task<Guid?> MyEmployeeIdAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return null;
            return await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }
    }

    // ---- Revoke -------------------------------------------------------------

    public class RevokeApprovalDelegation(
        IRepository<ApprovalDelegation> repository,
        IRepository<User> users,
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

            entity.Revoke(currentUser.GetCurrentUserName(), dto.Reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Delegation {Id} revoked", dto.Id);
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

    public class CheckDelegationEligibility(IDelegationEligibilityService eligibility) : ICheckDelegationEligibility
    {
        public async Task<DelegationEligibilityDto> CheckAsync(Guid fromEmployeeId, Guid toEmployeeId)
        {
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
                AllowSelfServiceDelegation = p.AllowSelfServiceDelegation
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
                    dto.AllowSelfServiceDelegation);
            }
            catch (ArgumentException ex) { throw new ValidationException("policy", ex.Message); }

            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Delegation policy updated");
        }
    }
}
