using System.Text;
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

    public interface ISaveJobRequisition { Task<Guid> SaveAsync(SaveJobRequisitionDto dto); }
    public interface IGetJobRequisitionById { Task<JobRequisitionDto> GetAsync(Guid id); }
    public interface IGetAllJobRequisitions { Task<PaginatedResponse<JobRequisitionDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteJobRequisition { Task DeleteAsync(Guid id); }
    public interface ISubmitJobRequisition { Task SubmitAsync(Guid id); }
    public interface ISetRequisitionPosting { Task SetAsync(SetPostingDto dto); }
    public interface IGenerateRequisitionPosting { Task<string> GenerateAsync(Guid id); }
    public interface IPostJobRequisition { Task PostAsync(Guid id); }
    public interface ICloseJobRequisition { Task CloseAsync(Guid id); }
    public interface ICancelJobRequisition { Task CancelAsync(Guid id); }

    internal static class RequisitionShared
    {
        /// <summary>The repository stamps only aggregate roots — cascade-inserted criteria copy it here.</summary>
        internal static void StampCriteriaTenant(JobRequisition requisition)
        {
            foreach (var c in requisition.ScreeningCriteria)
                if (string.IsNullOrEmpty(c.TenantId))
                    c.TenantId = requisition.TenantId;
        }

        internal static JobRequisitionDto ToDto(
            JobRequisition q,
            string? requestNumber, string? unitName, string? classTitle, string? gradeName,
            string? locationName, decimal? scaleAmount, int applicationCount)
        {
            return new JobRequisitionDto
            {
                Id = q.Id,
                RequisitionNumber = q.RequisitionNumber,
                HiringRequestId = q.HiringRequestId,
                HiringRequestNumber = requestNumber,
                OrganizationUnitId = q.OrganizationUnitId,
                OrganizationUnitName = unitName,
                PositionClassId = q.PositionClassId,
                PositionClassTitle = classTitle,
                JobGradeName = gradeName,
                WorkLocationId = q.WorkLocationId,
                WorkLocationName = locationName,
                NumberOfPositions = q.NumberOfPositions,
                EmploymentType = q.EmploymentType.ToString(),
                Title = q.Title,
                Description = q.Description,
                MinQualifications = q.MinQualifications,
                MinExperienceYears = q.MinExperienceYears,
                Skills = q.Skills,
                SalaryScaleId = q.SalaryScaleId,
                SalaryScaleAmount = scaleAmount,
                PostingChannel = q.PostingChannel.ToString(),
                PostingText = q.PostingText,
                OpenFrom = q.OpenFrom,
                OpenUntil = q.OpenUntil,
                Status = q.Status.ToString(),
                SubmittedAt = q.SubmittedAt,
                ApprovedAt = q.ApprovedAt,
                PostedAt = q.PostedAt,
                ClosedAt = q.ClosedAt,
                ApplicationCount = applicationCount,
                ScreeningCriteria = q.ScreeningCriteria
                    .OrderBy(c => c.Name)
                    .Select(c => new ScreeningCriterionDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        IsMandatory = c.IsMandatory,
                        Weight = c.Weight,
                        EvaluatorType = c.EvaluatorType.ToString(),
                        EvaluatorEmployeeId = c.EvaluatorEmployeeId,
                        EvaluatorName = c.EvaluatorName
                    })
                    .ToList()
            };
        }
    }

    // ---- Save (create only from an APPROVED hiring request — HC080) ----------------------

    public class SaveJobRequisition(
        IRepository<JobRequisition> repository,
        IRepository<RequisitionScreeningCriterion> criterionRepository,
        IRepository<HiringRequest> hiringRequestRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<WorkLocation> workLocationRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        IValidator<SaveJobRequisitionDto> validator,
        ILogger<SaveJobRequisition> logger) : ISaveJobRequisition
    {
        /// <summary>
        /// Builds criterion specs, resolving employee-evaluator display names server-side (never
        /// trusting client text) and validating the referenced employees exist.
        /// </summary>
        private async Task<List<ScreeningCriterionSpec>> BuildCriterionSpecsAsync(SaveJobRequisitionDto dto)
        {
            var employeeIds = dto.ScreeningCriteria
                .Where(c => string.Equals(c.EvaluatorType, "Employee", StringComparison.OrdinalIgnoreCase)
                    && c.EvaluatorEmployeeId.HasValue)
                .Select(c => c.EvaluatorEmployeeId!.Value)
                .Distinct()
                .ToList();
            var employeeNames = await employeeRepository.GetAll()
                .Where(e => employeeIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .ToDictionaryAsync(e => e.Id, e => e.Name);

            var specs = new List<ScreeningCriterionSpec>();
            foreach (var c in dto.ScreeningCriteria)
            {
                var type = Enum.Parse<CriterionEvaluatorType>(c.EvaluatorType, true);
                string? name = c.EvaluatorName;
                if (type == CriterionEvaluatorType.Employee)
                {
                    if (!c.EvaluatorEmployeeId.HasValue || !employeeNames.TryGetValue(c.EvaluatorEmployeeId.Value, out name))
                        throw new NotFoundException(nameof(Employee), c.EvaluatorEmployeeId?.ToString() ?? "(none)");
                }
                specs.Add(new ScreeningCriterionSpec(c.Name, c.IsMandatory, c.Weight, type,
                    c.EvaluatorEmployeeId, name));
            }
            return specs;
        }

        public async Task<Guid> SaveAsync(SaveJobRequisitionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC080: recruitment cannot start until the hiring need is fully approved.
            var request = await hiringRequestRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.Id == dto.HiringRequestId)
                ?? throw new NotFoundException(nameof(HiringRequest), dto.HiringRequestId.ToString());
            if (request.Status != HiringRequestStatus.Approved)
                throw new ValidationException("hiringRequestId",
                    $"Requisitions can only be raised from an APPROVED hiring request (current: {request.Status}) — HC080.");

            // Role details default from the request's position class (HC084).
            var positionClass = await positionClassRepository.GetAll()
                    .Where(c => c.Id == request.PositionClassId)
                    .Select(c => new { c.Title, c.Description, c.MinQualifications, c.MinExperienceYears, c.Skills, c.SalaryScaleId })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(PositionClass), request.PositionClassId.ToString());

            if (dto.WorkLocationId.HasValue &&
                !await workLocationRepository.GetAll().AnyAsync(w => w.Id == dto.WorkLocationId.Value))
                throw new NotFoundException(nameof(WorkLocation), dto.WorkLocationId.Value.ToString());
            var salaryScaleId = dto.SalaryScaleId ?? positionClass.SalaryScaleId;
            if (dto.SalaryScaleId.HasValue &&
                !await salaryScaleRepository.GetAll().AnyAsync(x => x.Id == dto.SalaryScaleId.Value))
                throw new NotFoundException(nameof(SalaryScale), dto.SalaryScaleId.Value.ToString());

            var employmentType = Enum.Parse<PlannedEmploymentType>(dto.EmploymentType, true);
            var title = string.IsNullOrWhiteSpace(dto.Title) ? positionClass.Title : dto.Title!;
            var description = dto.Description ?? positionClass.Description;
            var qualifications = dto.MinQualifications ?? positionClass.MinQualifications;
            var experience = dto.MinExperienceYears ?? positionClass.MinExperienceYears;
            var skills = dto.Skills ?? positionClass.Skills;
            var criteria = await BuildCriterionSpecsAsync(dto);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobRequisition, dto.Id.Value);

                var entity = await repository.GetAll()
                        .Include(q => q.ScreeningCriteria)
                        .FirstOrDefaultAsync(q => q.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(JobRequisition), dto.Id.Value.ToString());
                if (entity.Status is not (RequisitionStatus.Draft or RequisitionStatus.Rejected))
                    throw new ValidationException("status", $"A {entity.Status} requisition can no longer be edited.");

                await EnsureWithinRequestAsync(request, dto.NumberOfPositions, entity.Id);

                entity.Update(dto.NumberOfPositions, employmentType, title, description,
                    qualifications, experience, skills, dto.WorkLocationId, salaryScaleId);
                entity.SetScreeningCriteria(criteria);
                RequisitionShared.StampCriteriaTenant(entity);
                // Replacement criteria are new rows: mark them Added explicitly, otherwise
                // context.Update(root) treats the app-generated keys as existing (Modified).
                foreach (var c in entity.ScreeningCriteria)
                    await criterionRepository.AddAsync(c);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated JobRequisition {Id}", entity.Id);
                return entity.Id;
            }

            await EnsureWithinRequestAsync(request, dto.NumberOfPositions, null);

            var number = await RecruitmentShared.NextNumberAsync(repository, "REQ");
            var created = JobRequisition.Create(number, request.Id, request.OrganizationUnitId,
                request.PositionClassId, dto.NumberOfPositions, employmentType, title, description,
                qualifications, experience, skills, dto.WorkLocationId, salaryScaleId);
            created.SetScreeningCriteria(criteria);
            await repository.AddAsync(created);   // stamps the root's TenantId
            RequisitionShared.StampCriteriaTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created JobRequisition {Id} ({Number}) from HiringRequest {RequestId}",
                created.Id, number, request.Id);
            return created.Id;
        }

        /// <summary>The requisitions of a request may not exceed its approved position count (HC082).</summary>
        private async Task EnsureWithinRequestAsync(HiringRequest request, int positions, Guid? excludeRequisitionId)
        {
            var existing = await repository.GetAll()
                .Where(q => q.HiringRequestId == request.Id
                    && q.Status != RequisitionStatus.Cancelled
                    && (excludeRequisitionId == null || q.Id != excludeRequisitionId.Value))
                .SumAsync(q => (int?)q.NumberOfPositions) ?? 0;
            if (existing + positions > request.NumberOfPositions)
                throw new ValidationException("numberOfPositions",
                    $"The hiring request approves {request.NumberOfPositions} position(s); " +
                    $"{existing} are already requisitioned. Reduce this requisition or raise a new hiring need.");
        }
    }

    // ---- Submit → workflow (HC085) -----------------------------------------------------

    public class SubmitJobRequisition(
        IRepository<JobRequisition> repository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        ILogger<SubmitJobRequisition> logger) : ISubmitJobRequisition
    {
        public async Task SubmitAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobRequisition, id);

            var requisition = await repository.GetAll().FirstOrDefaultAsync(q => q.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());
            if (requisition.Status is not (RequisitionStatus.Draft or RequisitionStatus.Rejected))
                throw new ValidationException("status", $"A {requisition.Status} requisition cannot be submitted.");

            requisition.Submit();
            repository.UpdateAsync(requisition);
            await repository.SaveChangesAsync();

            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.JobRequisition, requisition.Id, null,
                $"Requisition {requisition.RequisitionNumber} — {requisition.Title} ({requisition.NumberOfPositions} position(s))");

            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.JobRequisition, requisition.Id))
            {
                requisition.Approve();
                repository.UpdateAsync(requisition);
                await repository.SaveChangesAsync();
            }

            logger.LogInformation("Submitted JobRequisition {Id}", id);
        }
    }

    // ---- Posting: generate text (HC091) + set + publish (HC088) ---------------------------

    public class GenerateRequisitionPosting(
        IRepository<JobRequisition> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<WorkLocation> workLocationRepository,
        IRepository<PositionClass> positionClassRepository) : IGenerateRequisitionPosting
    {
        public async Task<string> GenerateAsync(Guid id)
        {
            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());

            var unitName = await organizationUnitRepository.GetAll()
                .Where(u => u.Id == q.OrganizationUnitId).Select(u => u.Name).FirstOrDefaultAsync();
            var locationName = q.WorkLocationId.HasValue
                ? await workLocationRepository.GetAll()
                    .Where(w => w.Id == q.WorkLocationId.Value).Select(w => w.Name).FirstOrDefaultAsync()
                : null;
            var gradeName = await positionClassRepository.GetAll()
                .Where(c => c.Id == q.PositionClassId)
                .Select(c => c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null)
                .FirstOrDefaultAsync();

            // Standard advertisement generated from the requisition details (HC091) — the text is
            // stored on the requisition and stays fully editable before publishing.
            var sb = new StringBuilder();
            sb.AppendLine($"JOB VACANCY — {q.Title}");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine($"Requisition: {q.RequisitionNumber}");
            sb.AppendLine($"Unit: {unitName}");
            if (!string.IsNullOrEmpty(gradeName)) sb.AppendLine($"Grade: {gradeName}");
            if (!string.IsNullOrEmpty(locationName)) sb.AppendLine($"Location: {locationName}");
            sb.AppendLine($"Employment type: {q.EmploymentType}");
            sb.AppendLine($"Openings: {q.NumberOfPositions}");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(q.Description))
            {
                sb.AppendLine("About the role");
                sb.AppendLine(q.Description);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(q.MinQualifications))
            {
                sb.AppendLine("Minimum qualifications");
                sb.AppendLine(q.MinQualifications);
                sb.AppendLine();
            }
            if (q.MinExperienceYears.HasValue)
                sb.AppendLine($"Experience: at least {q.MinExperienceYears} year(s).");
            if (!string.IsNullOrWhiteSpace(q.Skills))
                sb.AppendLine($"Skills: {q.Skills}");
            sb.AppendLine();
            sb.AppendLine("How to apply: submit your application, resume and supporting documents through the HR office / career portal.");

            return sb.ToString();
        }
    }

    public class SetRequisitionPosting(
        IRepository<JobRequisition> repository,
        ILogger<SetRequisitionPosting> logger) : ISetRequisitionPosting
    {
        public async Task SetAsync(SetPostingDto dto)
        {
            if (!Enum.TryParse<PostingChannel>(dto.PostingChannel, true, out var channel))
                throw new ValidationException("postingChannel", "PostingChannel must be Internal, External or Both.");
            if (dto.OpenFrom.HasValue && dto.OpenUntil.HasValue && dto.OpenUntil < dto.OpenFrom)
                throw new ValidationException("openUntil", "The posting close date cannot be before its open date.");

            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobRequisition), dto.Id.ToString());
            if (q.Status is RequisitionStatus.Closed or RequisitionStatus.Cancelled)
                throw new ValidationException("status", $"A {q.Status} requisition's posting can no longer change.");

            q.SetPosting(channel, dto.PostingText, dto.OpenFrom, dto.OpenUntil);
            repository.UpdateAsync(q);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated posting for JobRequisition {Id} ({Channel})", dto.Id, channel);
        }
    }

    public class PostJobRequisition(
        IRepository<JobRequisition> repository,
        ILogger<PostJobRequisition> logger) : IPostJobRequisition
    {
        public async Task PostAsync(Guid id)
        {
            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());
            if (q.Status != RequisitionStatus.Approved)
                throw new ValidationException("status", $"Only an approved requisition can be posted (current: {q.Status}).");
            if (string.IsNullOrWhiteSpace(q.PostingText))
                throw new ValidationException("postingText", "Generate or write the posting text before publishing.");

            q.Post();
            repository.UpdateAsync(q);
            await repository.SaveChangesAsync();
            logger.LogInformation("Posted JobRequisition {Id} to {Channel}", id, q.PostingChannel);
        }
    }

    public class CloseJobRequisition(
        IRepository<JobRequisition> repository,
        ILogger<CloseJobRequisition> logger) : ICloseJobRequisition
    {
        public async Task CloseAsync(Guid id)
        {
            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());
            if (q.Status is not (RequisitionStatus.Approved or RequisitionStatus.Posted))
                throw new ValidationException("status", $"Only an approved or posted requisition can be closed (current: {q.Status}).");

            q.Close();
            repository.UpdateAsync(q);
            await repository.SaveChangesAsync();
            logger.LogInformation("Closed JobRequisition {Id}", id);
        }
    }

    public class CancelJobRequisition(
        IRepository<JobRequisition> repository,
        IWorkflowGate workflowGate,
        ILogger<CancelJobRequisition> logger) : ICancelJobRequisition
    {
        public async Task CancelAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobRequisition, id);

            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());
            if (q.Status is RequisitionStatus.Closed or RequisitionStatus.Cancelled or RequisitionStatus.Posted)
                throw new ValidationException("status", $"A {q.Status} requisition cannot be cancelled — close it instead.");

            q.Cancel();
            repository.UpdateAsync(q);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled JobRequisition {Id}", id);
        }
    }

    // ---- Get by id / Get all -----------------------------------------------------------

    public class GetJobRequisitionById(
        IRepository<JobRequisition> repository,
        IRepository<HiringRequest> hiringRequestRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<WorkLocation> workLocationRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<JobApplication> applicationRepository,
        IWorkflowGate workflowGate) : IGetJobRequisitionById
    {
        public async Task<JobRequisitionDto> GetAsync(Guid id)
        {
            var q = await repository.GetAll()
                    .Include(x => x.ScreeningCriteria)
                    .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());

            var requestNumber = await hiringRequestRepository.GetAll()
                .Where(r => r.Id == q.HiringRequestId).Select(r => r.RequestNumber).FirstOrDefaultAsync();
            var unitName = await organizationUnitRepository.GetAll()
                .Where(u => u.Id == q.OrganizationUnitId).Select(u => u.Name).FirstOrDefaultAsync();
            var classInfo = await positionClassRepository.GetAll()
                .Where(c => c.Id == q.PositionClassId)
                .Select(c => new
                {
                    c.Title,
                    Grade = c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null
                })
                .FirstOrDefaultAsync();
            var locationName = q.WorkLocationId.HasValue
                ? await workLocationRepository.GetAll()
                    .Where(w => w.Id == q.WorkLocationId.Value).Select(w => w.Name).FirstOrDefaultAsync()
                : null;
            var scaleAmount = q.SalaryScaleId.HasValue
                ? await salaryScaleRepository.GetAll()
                    .Where(x => x.Id == q.SalaryScaleId.Value).Select(x => (decimal?)x.Salary).FirstOrDefaultAsync()
                : null;
            var applicationCount = await applicationRepository.GetAll().CountAsync(a => a.RequisitionId == id);

            var dto = RequisitionShared.ToDto(q, requestNumber, unitName, classInfo?.Title, classInfo?.Grade,
                locationName, scaleAmount, applicationCount);
            dto.AwaitingWorkflow = q.Status == RequisitionStatus.PendingApproval
                && await workflowGate.HasRunningAsync(WorkflowEntityTypes.JobRequisition, q.Id);
            return dto;
        }
    }

    public class GetAllJobRequisitions(
        IRepository<JobRequisition> repository,
        IRepository<HiringRequest> hiringRequestRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<JobApplication> applicationRepository) : IGetAllJobRequisitions
    {
        public async Task<PaginatedResponse<JobRequisitionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<RequisitionStatus>(request.Status, true, out var status))
                query = query.Where(q => q.Status == status);
            if (request.ParentId.HasValue)
                query = query.Where(q => q.OrganizationUnitId == request.ParentId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(q => q.RequisitionNumber.Contains(term) || q.Title.Contains(term));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip(skip).Take(take)
                .Select(q => new
                {
                    Requisition = q,
                    RequestNumber = hiringRequestRepository.GetAll()
                        .Where(r => r.Id == q.HiringRequestId).Select(r => r.RequestNumber).FirstOrDefault(),
                    UnitName = organizationUnitRepository.GetAll()
                        .Where(u => u.Id == q.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    ClassTitle = positionClassRepository.GetAll()
                        .Where(c => c.Id == q.PositionClassId).Select(c => c.Title).FirstOrDefault(),
                    ApplicationCount = applicationRepository.GetAll().Count(a => a.RequisitionId == q.Id)
                })
                .ToListAsync();

            var data = rows
                .Select(x => RequisitionShared.ToDto(x.Requisition, x.RequestNumber, x.UnitName,
                    x.ClassTitle, null, null, null, x.ApplicationCount))
                .ToList();

            return new PaginatedResponse<JobRequisitionDto> { Total = total, Data = data };
        }
    }

    public class DeleteJobRequisition(
        IRepository<JobRequisition> repository,
        IRepository<JobApplication> applicationRepository,
        IWorkflowGate workflowGate,
        ILogger<DeleteJobRequisition> logger) : IDeleteJobRequisition
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobRequisition, id);

            var q = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobRequisition), id.ToString());
            if (q.Status is not (RequisitionStatus.Draft or RequisitionStatus.Rejected or RequisitionStatus.Cancelled))
                throw new ValidationException("status",
                    $"A {q.Status} requisition is part of the recruitment record — close it instead of deleting.");
            if (await applicationRepository.GetAll().AnyAsync(a => a.RequisitionId == id))
                throw new ValidationException("id", "Applications reference this requisition.");

            repository.Delete(q);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted JobRequisition {Id}", id);
        }
    }
}
