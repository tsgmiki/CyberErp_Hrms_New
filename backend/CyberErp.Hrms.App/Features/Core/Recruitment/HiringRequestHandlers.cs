using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- Interfaces -----------------------------------------------------------

    public interface ISaveHiringRequest { Task<Guid> SaveAsync(SaveHiringRequestDto dto); }
    public interface IGetHiringRequestById { Task<HiringRequestDto> GetAsync(Guid id); }
    public interface IGetAllHiringRequests { Task<PaginatedResponse<HiringRequestDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteHiringRequest { Task DeleteAsync(Guid id); }
    public interface ISubmitHiringRequest { Task SubmitAsync(Guid id); }
    public interface ICloseHiringRequest { Task CloseAsync(Guid id); }
    public interface IGetRecruitmentBudgetMonitor { Task<List<RecruitmentBudgetRowDto>> GetAsync(); }

    internal static class RecruitmentShared
    {
        /// <summary>
        /// Race-safe sequential document numbering via the per-tenant atomic counter
        /// (logic.md §7.1 adoption #5) — replaced the count+1 approach, which double-allocated
        /// under concurrent creates. Existing tenants' counters were seeded from their current max.
        /// </summary>
        internal static async Task<string> NextNumberAsync(INumberSequenceService sequence, string key, string prefix)
            => $"{prefix}-{await sequence.NextAsync(key):D4}";

        /// <summary>Vacant seats for a unit × role — the establishment limit hiring is checked against (HC082).</summary>
        internal static Task<int> VacantSeatsAsync(IRepository<Position> positions, Guid unitId, Guid classId) =>
            positions.GetAll().CountAsync(p =>
                p.OrganizationUnitId == unitId && p.PositionClassId == classId && p.IsVacant);
    }

    // ---- Save (create / correct while editable) --------------------------------------

    public class SaveHiringRequest(
        IRepository<HiringRequest> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<WorkforcePlan> workforcePlanRepository,
        INumberSequenceService numberSequence,
        IWorkflowGate workflowGate,
        IValidator<SaveHiringRequestDto> validator,
        ILogger<SaveHiringRequest> logger) : ISaveHiringRequest
    {
        public async Task<Guid> SaveAsync(SaveHiringRequestDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await organizationUnitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.ToString());
            if (!await positionClassRepository.GetAll().AnyAsync(c => c.Id == dto.PositionClassId))
                throw new NotFoundException(nameof(PositionClass), dto.PositionClassId.ToString());
            if (dto.WorkforcePlanId.HasValue &&
                !await workforcePlanRepository.GetAll().AnyAsync(p => p.Id == dto.WorkforcePlanId.Value))
                throw new NotFoundException(nameof(WorkforcePlan), dto.WorkforcePlanId.Value.ToString());

            var employmentType = Enum.Parse<PlannedEmploymentType>(dto.EmploymentType, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.HiringRequest, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(HiringRequest), dto.Id.Value.ToString());
                if (entity.Status is not (HiringRequestStatus.Draft or HiringRequestStatus.Rejected))
                    throw new ValidationException("status", $"A {entity.Status} hiring request can no longer be edited.");

                entity.Update(dto.OrganizationUnitId, dto.PositionClassId, dto.NumberOfPositions,
                    employmentType, dto.Justification, dto.JobRequirements, dto.ExpectedStartDate,
                    dto.TimelineRemarks, dto.EstimatedBudget, dto.WorkforcePlanId);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated HiringRequest {Id}", entity.Id);
                return entity.Id;
            }

            var number = await RecruitmentShared.NextNumberAsync(numberSequence, "HiringRequest", "HRQ");
            var created = HiringRequest.Create(number, dto.OrganizationUnitId, dto.PositionClassId,
                dto.NumberOfPositions, employmentType, dto.Justification, dto.JobRequirements,
                dto.ExpectedStartDate, dto.TimelineRemarks, dto.EstimatedBudget, dto.WorkforcePlanId);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created HiringRequest {Id} ({Number})", created.Id, number);
            return created.Id;
        }
    }

    // ---- Submit (establishment validation + workflow, HC078/HC080/HC082) ----------------

    public class SubmitHiringRequest(
        IRepository<HiringRequest> repository,
        IRepository<Position> positionRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        ILogger<SubmitHiringRequest> logger) : ISubmitHiringRequest
    {
        public async Task SubmitAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.HiringRequest, id);

            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(HiringRequest), id.ToString());
            if (request.Status is not (HiringRequestStatus.Draft or HiringRequestStatus.Rejected))
                throw new ValidationException("status", $"A {request.Status} hiring request cannot be submitted.");

            // HC082: a hiring need may not exceed the approved establishment — every requested
            // position must map onto a currently vacant seat of the unit × role.
            var vacant = await RecruitmentShared.VacantSeatsAsync(
                positionRepository, request.OrganizationUnitId, request.PositionClassId);
            if (request.NumberOfPositions > vacant)
                throw new ValidationException("numberOfPositions",
                    $"The request exceeds the approved establishment: {vacant} vacant seat(s) exist for this " +
                    "unit and role. Expand the establishment (positions) first or reduce the request.");

            request.Submit();
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();

            // Seeded chain: Directorate Head → HR → Finance (HC078); no definition → direct approval.
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.HiringRequest, request.Id, null,
                $"Hiring Need — {request.RequestNumber}: {request.NumberOfPositions} × role (budget {request.EstimatedBudget:N0})");

            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.HiringRequest, request.Id))
            {
                request.Approve();
                repository.UpdateAsync(request);
                await repository.SaveChangesAsync();
            }

            logger.LogInformation("Submitted HiringRequest {Id} ({Vacant} vacant seats)", id, vacant);
        }
    }

    // ---- Close ---------------------------------------------------------------------

    public class CloseHiringRequest(
        IRepository<HiringRequest> repository,
        IWorkflowGate workflowGate,
        ILogger<CloseHiringRequest> logger) : ICloseHiringRequest
    {
        public async Task CloseAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.HiringRequest, id);

            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(HiringRequest), id.ToString());
            if (request.Status == HiringRequestStatus.Closed)
                throw new ValidationException("status", "The hiring request is already closed.");

            request.Close();
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();
            logger.LogInformation("Closed HiringRequest {Id}", id);
        }
    }

    // ---- Get by id -------------------------------------------------------------------

    public class GetHiringRequestById(
        IRepository<HiringRequest> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<WorkforcePlan> workforcePlanRepository,
        IRepository<Position> positionRepository,
        IRepository<JobRequisition> requisitionRepository,
        IWorkflowGate workflowGate) : IGetHiringRequestById
    {
        public async Task<HiringRequestDto> GetAsync(Guid id)
        {
            var row = await repository.GetAll()
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        Request = r,
                        UnitName = organizationUnitRepository.GetAll()
                            .Where(u => u.Id == r.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                        ClassInfo = positionClassRepository.GetAll()
                            .Where(c => c.Id == r.PositionClassId)
                            .Select(c => new
                            {
                                c.Title,
                                Grade = c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null
                            })
                            .FirstOrDefault(),
                        PlanName = workforcePlanRepository.GetAll()
                            .Where(p => p.Id == r.WorkforcePlanId).Select(p => p.Name).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(HiringRequest), id.ToString());

            var r0 = row.Request;
            var dto = new HiringRequestDto
            {
                Id = r0.Id,
                RequestNumber = r0.RequestNumber,
                OrganizationUnitId = r0.OrganizationUnitId,
                OrganizationUnitName = row.UnitName,
                PositionClassId = r0.PositionClassId,
                PositionClassTitle = row.ClassInfo?.Title,
                JobGradeName = row.ClassInfo?.Grade,
                NumberOfPositions = r0.NumberOfPositions,
                EmploymentType = r0.EmploymentType.ToString(),
                Justification = r0.Justification,
                JobRequirements = r0.JobRequirements,
                ExpectedStartDate = r0.ExpectedStartDate,
                TimelineRemarks = r0.TimelineRemarks,
                EstimatedBudget = r0.EstimatedBudget,
                WorkforcePlanId = r0.WorkforcePlanId,
                WorkforcePlanName = row.PlanName,
                Status = r0.Status.ToString(),
                SubmittedAt = r0.SubmittedAt,
                ApprovedAt = r0.ApprovedAt
            };

            dto.AwaitingWorkflow = r0.Status == HiringRequestStatus.Submitted
                && await workflowGate.HasRunningAsync(WorkflowEntityTypes.HiringRequest, dto.Id);
            dto.VacantSeats = await RecruitmentShared.VacantSeatsAsync(
                positionRepository, dto.OrganizationUnitId, dto.PositionClassId);
            dto.RequisitionedPositions = await requisitionRepository.GetAll()
                .Where(q => q.HiringRequestId == id && q.Status != RequisitionStatus.Cancelled)
                .SumAsync(q => (int?)q.NumberOfPositions) ?? 0;
            return dto;
        }
    }

    // ---- Get all (paged) ---------------------------------------------------------------

    public class GetAllHiringRequests(
        IRepository<HiringRequest> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<WorkforcePlan> workforcePlanRepository) : IGetAllHiringRequests
    {
        public async Task<PaginatedResponse<HiringRequestDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<HiringRequestStatus>(request.Status, true, out var status))
                query = query.Where(r => r.Status == status);
            if (request.ParentId.HasValue)
                query = query.Where(r => r.OrganizationUnitId == request.ParentId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(r => r.RequestNumber.Contains(term) || r.Justification.Contains(term));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip).Take(take)
                .Select(r => new
                {
                    Request = r,
                    UnitName = organizationUnitRepository.GetAll()
                        .Where(u => u.Id == r.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    ClassInfo = positionClassRepository.GetAll()
                        .Where(c => c.Id == r.PositionClassId)
                        .Select(c => new
                        {
                            c.Title,
                            Grade = c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null
                        })
                        .FirstOrDefault(),
                    PlanName = workforcePlanRepository.GetAll()
                        .Where(p => p.Id == r.WorkforcePlanId).Select(p => p.Name).FirstOrDefault()
                })
                .ToListAsync();

            var data = rows.Select(x => new HiringRequestDto
            {
                Id = x.Request.Id,
                RequestNumber = x.Request.RequestNumber,
                OrganizationUnitId = x.Request.OrganizationUnitId,
                OrganizationUnitName = x.UnitName,
                PositionClassId = x.Request.PositionClassId,
                PositionClassTitle = x.ClassInfo?.Title,
                JobGradeName = x.ClassInfo?.Grade,
                NumberOfPositions = x.Request.NumberOfPositions,
                EmploymentType = x.Request.EmploymentType.ToString(),
                Justification = x.Request.Justification,
                JobRequirements = x.Request.JobRequirements,
                ExpectedStartDate = x.Request.ExpectedStartDate,
                TimelineRemarks = x.Request.TimelineRemarks,
                EstimatedBudget = x.Request.EstimatedBudget,
                WorkforcePlanId = x.Request.WorkforcePlanId,
                WorkforcePlanName = x.PlanName,
                Status = x.Request.Status.ToString(),
                SubmittedAt = x.Request.SubmittedAt,
                ApprovedAt = x.Request.ApprovedAt
            }).ToList();

            return new PaginatedResponse<HiringRequestDto> { Total = total, Data = data };
        }
    }

    // ---- Delete (drafts / rejected only) --------------------------------------------------

    public class DeleteHiringRequest(
        IRepository<HiringRequest> repository,
        IRepository<JobRequisition> requisitionRepository,
        IWorkflowGate workflowGate,
        ILogger<DeleteHiringRequest> logger) : IDeleteHiringRequest
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.HiringRequest, id);

            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(HiringRequest), id.ToString());
            if (request.Status is not (HiringRequestStatus.Draft or HiringRequestStatus.Rejected))
                throw new ValidationException("status",
                    $"A {request.Status} hiring request is part of the recruitment record — close it instead of deleting.");
            if (await requisitionRepository.GetAll().AnyAsync(q => q.HiringRequestId == id))
                throw new ValidationException("id", "Requisitions reference this hiring request.");

            repository.Delete(request);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted HiringRequest {Id}", id);
        }
    }

    // ---- Budget monitor (HC083) --------------------------------------------------------------

    public class GetRecruitmentBudgetMonitor(
        IRepository<HiringRequest> repository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<OrganizationUnit> organizationUnitRepository) : IGetRecruitmentBudgetMonitor
    {
        public async Task<List<RecruitmentBudgetRowDto>> GetAsync()
        {
            var groups = await repository.GetAll()
                .Where(r => r.Status == HiringRequestStatus.Approved || r.Status == HiringRequestStatus.Submitted)
                .GroupBy(r => r.OrganizationUnitId)
                .Select(g => new
                {
                    OrganizationUnitId = g.Key,
                    ApprovedRequests = g.Count(r => r.Status == HiringRequestStatus.Approved),
                    RequestedPositions = g.Sum(r => r.NumberOfPositions),
                    EstimatedBudget = g.Sum(r => r.EstimatedBudget)
                })
                .ToListAsync();

            var openRequisitions = await requisitionRepository.GetAll()
                .Where(q => q.Status == RequisitionStatus.Approved || q.Status == RequisitionStatus.Posted
                    || q.Status == RequisitionStatus.PendingApproval)
                .GroupBy(q => q.OrganizationUnitId)
                .Select(g => new { OrganizationUnitId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.OrganizationUnitId, g => g.Count);

            var unitIds = groups.Select(g => g.OrganizationUnitId).ToList();
            var unitNames = await organizationUnitRepository.GetAll()
                .Where(u => unitIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            return groups
                .Select(g => new RecruitmentBudgetRowDto
                {
                    OrganizationUnitId = g.OrganizationUnitId,
                    OrganizationUnitName = unitNames.GetValueOrDefault(g.OrganizationUnitId),
                    ApprovedRequests = g.ApprovedRequests,
                    RequestedPositions = g.RequestedPositions,
                    EstimatedBudget = g.EstimatedBudget,
                    OpenRequisitions = openRequisitions.GetValueOrDefault(g.OrganizationUnitId)
                })
                .OrderBy(r => r.OrganizationUnitName)
                .ToList();
        }
    }
}
