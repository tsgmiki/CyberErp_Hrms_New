using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class SuccessionDevelopmentActionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = nameof(SuccessionActionType.Training);
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = nameof(SuccessionActionStatus.Planned);
        public Guid? MentorEmployeeId { get; set; }
    }

    public class KnowledgeTransferDto
    {
        public Guid Id { get; set; }
        public string Topic { get; set; } = string.Empty;
        public Guid? FromEmployeeId { get; set; }
        public string Status { get; set; } = nameof(KnowledgeTransferStatus.NotStarted);
        public DateTime? TargetDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class SuccessionCandidateDto
    {
        public Guid Id { get; set; }
        public Guid SuccessionPlanId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public int Rank { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
        public string? GapSummary { get; set; }
        public string? Notes { get; set; }
        public List<SuccessionDevelopmentActionDto> DevelopmentActions { get; set; } = [];
        public List<KnowledgeTransferDto> KnowledgeTransfers { get; set; } = [];
    }

    public class SaveSuccessionDevelopmentActionDto
    {
        public string Type { get; set; } = nameof(SuccessionActionType.Training);
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = nameof(SuccessionActionStatus.Planned);
        public Guid? MentorEmployeeId { get; set; }
    }

    public class SaveKnowledgeTransferDto
    {
        public string Topic { get; set; } = string.Empty;
        public Guid? FromEmployeeId { get; set; }
        public string Status { get; set; } = nameof(KnowledgeTransferStatus.NotStarted);
        public DateTime? TargetDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class SaveSuccessionCandidateDto
    {
        public Guid? Id { get; set; }
        public Guid SuccessionPlanId { get; set; }
        public Guid EmployeeId { get; set; }
        public int Rank { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
        public string? GapSummary { get; set; }
        public string? Notes { get; set; }
        public List<SaveSuccessionDevelopmentActionDto> DevelopmentActions { get; set; } = [];
        public List<SaveKnowledgeTransferDto> KnowledgeTransfers { get; set; } = [];
    }

    public class SaveSuccessionCandidateDtoValidator : AbstractValidator<SaveSuccessionCandidateDto>
    {
        public SaveSuccessionCandidateDtoValidator()
        {
            RuleFor(x => x.SuccessionPlanId).NotEmpty();
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Rank).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Readiness).NotEmpty().Must(v => Enum.TryParse<ReadinessLevel>(v, out _)).WithMessage("Invalid readiness level.");
            RuleFor(x => x.ReadinessScore).InclusiveBetween(0, 100).When(x => x.ReadinessScore.HasValue);
            RuleForEach(x => x.DevelopmentActions).ChildRules(a =>
            {
                a.RuleFor(x => x.Type).Must(v => Enum.TryParse<SuccessionActionType>(v, out _)).WithMessage("Invalid action type.");
                a.RuleFor(x => x.Status).Must(v => Enum.TryParse<SuccessionActionStatus>(v, out _)).WithMessage("Invalid action status.");
                a.RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
            });
            RuleForEach(x => x.KnowledgeTransfers).ChildRules(k =>
            {
                k.RuleFor(x => x.Topic).NotEmpty().MaximumLength(300);
                k.RuleFor(x => x.Status).Must(v => Enum.TryParse<KnowledgeTransferStatus>(v, out _)).WithMessage("Invalid transfer status.");
            });
        }
    }

    // ---- Competency gap (HC155) ---------------------------------------------
    public class CompetencyGapItemDto
    {
        public Guid CompetencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class CompetencyGapDto
    {
        public Guid SuccessionCandidateId { get; set; }
        public int RequiredCount { get; set; }
        public int MetCount { get; set; }
        public List<CompetencyGapItemDto> Gaps { get; set; } = [];
    }

    internal static class SuccessionCandidateMapper
    {
        internal static SuccessionCandidateDto ToDto(SuccessionCandidate c) => new()
        {
            Id = c.Id, SuccessionPlanId = c.SuccessionPlanId, EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee?.Person != null ? $"{c.Employee.Person.FirstName} {c.Employee.Person.GrandFatherName}" : null,
            EmployeeNumber = c.Employee?.EmployeeNumber,
            Rank = c.Rank, Readiness = c.Readiness.ToString(), ReadinessScore = c.ReadinessScore,
            GapSummary = c.GapSummary, Notes = c.Notes,
            DevelopmentActions = c.DevelopmentActions.Select(a => new SuccessionDevelopmentActionDto
            {
                Id = a.Id, Type = a.Type.ToString(), Description = a.Description, DueDate = a.DueDate,
                Status = a.Status.ToString(), MentorEmployeeId = a.MentorEmployeeId
            }).ToList(),
            KnowledgeTransfers = c.KnowledgeTransfers.Select(k => new KnowledgeTransferDto
            {
                Id = k.Id, Topic = k.Topic, FromEmployeeId = k.FromEmployeeId, Status = k.Status.ToString(),
                TargetDate = k.TargetDate, CompletedDate = k.CompletedDate
            }).ToList()
        };

        internal static void StampChildTenant(SuccessionCandidate c)
        {
            foreach (var a in c.DevelopmentActions) if (string.IsNullOrEmpty(a.TenantId)) a.TenantId = c.TenantId;
            foreach (var k in c.KnowledgeTransfers) if (string.IsNullOrEmpty(k.TenantId)) k.TenantId = c.TenantId;
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveSuccessionCandidate { Task<Guid> SaveAsync(SaveSuccessionCandidateDto dto); }
    public interface IDeleteSuccessionCandidate { Task DeleteAsync(Guid id); }
    public interface IGetSuccessionCandidateById { Task<SuccessionCandidateDto> GetAsync(Guid id); }
    public interface IGetAllSuccessionCandidates { Task<PaginatedResponse<SuccessionCandidateDto>> GetAsync(GetAllRequest request); }
    public interface IGetSuccessionCandidateGap { Task<CompetencyGapDto> GetAsync(Guid successionCandidateId); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveSuccessionCandidate(
        IRepository<SuccessionCandidate> repository,
        IRepository<SuccessionDevelopmentAction> actionRepository,
        IRepository<KnowledgeTransfer> transferRepository,
        IValidator<SaveSuccessionCandidateDto> validator,
        ILogger<SaveSuccessionCandidate> logger) : ISaveSuccessionCandidate
    {
        public async Task<Guid> SaveAsync(SaveSuccessionCandidateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.SuccessionPlanId == dto.SuccessionPlanId
                    && x.EmployeeId == dto.EmployeeId && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(SuccessionCandidate), nameof(dto.EmployeeId), dto.EmployeeId.ToString());

            var readiness = Enum.Parse<ReadinessLevel>(dto.Readiness);
            var actionSpecs = dto.DevelopmentActions.Select(a => new SuccessionActionSpec(
                null, Enum.Parse<SuccessionActionType>(a.Type), a.Description, a.DueDate,
                Enum.Parse<SuccessionActionStatus>(a.Status), a.MentorEmployeeId)).ToList();
            var transferSpecs = dto.KnowledgeTransfers.Select(k => new KnowledgeTransferSpec(
                null, k.Topic, k.FromEmployeeId, Enum.Parse<KnowledgeTransferStatus>(k.Status), k.TargetDate, k.CompletedDate)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll()
                        .Include(x => x.DevelopmentActions).Include(x => x.KnowledgeTransfers)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(SuccessionCandidate), dto.Id.Value.ToString());

                foreach (var old in entity.DevelopmentActions.ToList()) actionRepository.Delete(old);
                foreach (var old in entity.KnowledgeTransfers.ToList()) transferRepository.Delete(old);
                entity.Update(dto.EmployeeId, dto.Rank, readiness, dto.ReadinessScore, dto.GapSummary, dto.Notes);
                entity.SetDevelopmentActions(actionSpecs);
                entity.SetKnowledgeTransfers(transferSpecs);
                SuccessionCandidateMapper.StampChildTenant(entity);
                foreach (var a in entity.DevelopmentActions) await actionRepository.AddAsync(a);
                foreach (var k in entity.KnowledgeTransfers) await transferRepository.AddAsync(k);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = SuccessionCandidate.Create(dto.SuccessionPlanId, dto.EmployeeId, dto.Rank, readiness, dto.ReadinessScore, dto.GapSummary, dto.Notes);
            created.SetDevelopmentActions(actionSpecs);
            created.SetKnowledgeTransfers(transferSpecs);
            await repository.AddAsync(created);
            SuccessionCandidateMapper.StampChildTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created SuccessionCandidate {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteSuccessionCandidate(IRepository<SuccessionCandidate> repository) : IDeleteSuccessionCandidate
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(SuccessionCandidate), id.ToString());
            repository.Delete(entity); // cascade removes dev-actions + knowledge-transfer
            await repository.SaveChangesAsync();
        }
    }

    public class GetSuccessionCandidateById(IRepository<SuccessionCandidate> repository) : IGetSuccessionCandidateById
    {
        public async Task<SuccessionCandidateDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll()
                    .Include(x => x.Employee).ThenInclude(e => e!.Person)
                    .Include(x => x.DevelopmentActions)
                    .Include(x => x.KnowledgeTransfers)
                    .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(SuccessionCandidate), id.ToString());
            return SuccessionCandidateMapper.ToDto(entity);
        }
    }

    public class GetAllSuccessionCandidates(IRepository<SuccessionCandidate> repository) : IGetAllSuccessionCandidates
    {
        public async Task<PaginatedResponse<SuccessionCandidateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll();
            if (request.ParentId.HasValue) // scoped to a succession plan
                query = query.Where(x => x.SuccessionPlanId == request.ParentId.Value);

            var total = await query.CountAsync();
            // Lightweight list — child collections load only on GetById.
            var data = await query.OrderBy(x => x.Rank).Skip(skip).Take(take)
                .Select(c => new SuccessionCandidateDto
                {
                    Id = c.Id, SuccessionPlanId = c.SuccessionPlanId, EmployeeId = c.EmployeeId,
                    EmployeeName = c.Employee != null && c.Employee.Person != null
                        ? (c.Employee.Person.FirstName + " " + c.Employee.Person.GrandFatherName) : null,
                    EmployeeNumber = c.Employee != null ? c.Employee.EmployeeNumber : null,
                    Rank = c.Rank, Readiness = c.Readiness.ToString(), ReadinessScore = c.ReadinessScore,
                    GapSummary = c.GapSummary, Notes = c.Notes
                }).ToListAsync();

            return new PaginatedResponse<SuccessionCandidateDto> { Total = total, Data = data };
        }
    }

    /// <summary>
    /// Competency gap (HC155): the target role's required competencies that the candidate's CURRENT
    /// position does not carry. Two small set-based queries against the position-competency framework —
    /// no per-competency loops or N+1.
    /// </summary>
    public class GetSuccessionCandidateGap(
        IRepository<SuccessionCandidate> candidateRepository,
        IRepository<SuccessionPlan> planRepository,
        IRepository<CriticalPosition> criticalPositionRepository,
        IRepository<Employee> employeeRepository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        IRepository<Competency> competencyRepository) : IGetSuccessionCandidateGap
    {
        public async Task<CompetencyGapDto> GetAsync(Guid successionCandidateId)
        {
            var candidate = await candidateRepository.GetAll().Where(c => c.Id == successionCandidateId)
                    .Select(c => new { c.Id, c.SuccessionPlanId, c.EmployeeId }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(SuccessionCandidate), successionCandidateId.ToString());

            var targetPositionId = await planRepository.GetAll()
                .Where(p => p.Id == candidate.SuccessionPlanId)
                .Join(criticalPositionRepository.GetAll(), p => p.CriticalPositionId, cp => cp.Id, (p, cp) => cp.PositionId)
                .FirstOrDefaultAsync();
            var currentPositionId = await employeeRepository.GetAll()
                .Where(e => e.Id == candidate.EmployeeId).Select(e => e.PositionId).FirstOrDefaultAsync();

            // Target role's required competencies (id + name + weight).
            var required = await positionCompetencyRepository.GetAll()
                .Where(pc => pc.PositionId == targetPositionId)
                .Join(competencyRepository.GetAll(), pc => pc.CompetencyId, c => c.Id,
                    (pc, c) => new CompetencyGapItemDto { CompetencyId = pc.CompetencyId, Name = c.Name, Weight = pc.Weight })
                .ToListAsync();

            // Competencies the employee's current position already covers.
            var currentIds = currentPositionId.HasValue
                ? await positionCompetencyRepository.GetAll().Where(pc => pc.PositionId == currentPositionId.Value)
                    .Select(pc => pc.CompetencyId).ToListAsync()
                : [];
            var currentSet = currentIds.ToHashSet();

            var gaps = required.Where(r => !currentSet.Contains(r.CompetencyId)).ToList();
            return new CompetencyGapDto
            {
                SuccessionCandidateId = successionCandidateId,
                RequiredCount = required.Count,
                MetCount = required.Count - gaps.Count,
                Gaps = gaps
            };
        }
    }
}
