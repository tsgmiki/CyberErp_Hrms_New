using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// Workflow outcomes for personnel movements (Transfer / Promotion / Demotion): final approval
    /// executes the movement against the employee master; rejection cancels it.
    /// </summary>
    public class EmployeeMovementWorkflowHandler(
        IExecuteEmployeeMovement executeHandler,
        ICancelEmployeeMovement cancelHandler) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            entityType.StartsWith("EmployeeMovement", StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) =>
            executeHandler.ExecuteAsync(entityId);

        public Task OnRejectedAsync(string entityType, Guid entityId) =>
            cancelHandler.CancelAsync(entityId);
    }

    /// <summary>
    /// Workflow outcomes for termination cases: approval opens the departmental clearance
    /// checklist; rejection cancels the case.
    /// </summary>
    public class EmployeeTerminationWorkflowHandler(
        IRepository<EmployeeTermination> repository,
        IRepository<TerminationClearance> clearanceRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.Termination, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var termination = await repository.GetAll()
                    .Include(t => t.Clearances)
                    .FirstOrDefaultAsync(t => t.Id == entityId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), entityId.ToString());
            await TerminationShared.BeginClearanceAsync(termination, clearanceRepository, clearanceDepartmentRepository);
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == entityId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), entityId.ToString());
            termination.Cancel();
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcomes for workforce plans (HC070): final approval makes the plan the approved
    /// workforce baseline and archives any older approved version of the same chain (HC071);
    /// rejection returns it to the planner for revision.
    /// </summary>
    public class WorkforcePlanWorkflowHandler(
        IRepository<WorkforcePlan> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.WorkforcePlan, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var plan = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == entityId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), entityId.ToString());
            plan.Approve();
            repository.UpdateAsync(plan);

            // One approved plan per version chain: the newly approved version supersedes older ones.
            var rootId = plan.RootPlanId ?? plan.Id;
            var superseded = await repository.GetAll()
                .Where(p => (p.RootPlanId == rootId || p.Id == rootId)
                    && p.Id != plan.Id
                    && p.Status == Dom.Entities.Core.WorkforcePlanStatus.Approved)
                .ToListAsync();
            foreach (var old in superseded)
            {
                old.Archive();
                repository.UpdateAsync(old);
            }

            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var plan = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == entityId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), entityId.ToString());
            plan.Reject();
            repository.UpdateAsync(plan);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcomes for hiring-need assessments (HC078): approval opens recruitment for the
    /// need (requisitions can be raised, HC080); rejection returns it to the requester.
    /// </summary>
    public class HiringRequestWorkflowHandler(
        IRepository<HiringRequest> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.HiringRequest, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == entityId)
                ?? throw new NotFoundException(nameof(HiringRequest), entityId.ToString());
            request.Approve();
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().FirstOrDefaultAsync(r => r.Id == entityId)
                ?? throw new NotFoundException(nameof(HiringRequest), entityId.ToString());
            request.Reject();
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcomes for job requisitions (HC085): approval readies the vacancy for posting;
    /// rejection returns it to HR for revision.
    /// </summary>
    public class JobRequisitionWorkflowHandler(
        IRepository<JobRequisition> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.JobRequisition, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var requisition = await repository.GetAll().FirstOrDefaultAsync(q => q.Id == entityId)
                ?? throw new NotFoundException(nameof(JobRequisition), entityId.ToString());
            requisition.Approve();
            repository.UpdateAsync(requisition);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var requisition = await repository.GetAll().FirstOrDefaultAsync(q => q.Id == entityId)
                ?? throw new NotFoundException(nameof(JobRequisition), entityId.ToString());
            requisition.Reject();
            repository.UpdateAsync(requisition);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcomes for job offers (HC112): FINAL approval readies the offer and immediately
    /// attempts auto-delivery — the letter is rendered as a PDF and e-mailed to the candidate; on
    /// success the offer marks Sent and the application moves to OfferPending. Delivery failure
    /// leaves the offer Approved (manual Send is the retry); rejection returns it to Draft.
    /// </summary>
    public class JobOfferWorkflowHandler(
        IRepository<JobOffer> repository,
        Recruitment.IOfferDelivery offerDelivery) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.JobOffer, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == entityId)
                ?? throw new NotFoundException(nameof(JobOffer), entityId.ToString());
            offer.Approve();
            repository.UpdateAsync(offer);
            await repository.SaveChangesAsync();
            await offerDelivery.TryAutoSendAsync(entityId);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == entityId)
                ?? throw new NotFoundException(nameof(JobOffer), entityId.ToString());
            offer.RejectToDraft();
            repository.UpdateAsync(offer);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcomes for disciplinary cases: approval confirms the measure (Resolved);
    /// rejection voids the case (Cancelled).
    /// </summary>
    public class DisciplinaryMeasureWorkflowHandler(
        IRepository<DisciplinaryMeasure> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.DisciplinaryMeasure, StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) => SetStatusAsync(entityId, DisciplinaryStatus.Resolved);

        public Task OnRejectedAsync(string entityType, Guid entityId) => SetStatusAsync(entityId, DisciplinaryStatus.Cancelled);

        private async Task SetStatusAsync(Guid id, DisciplinaryStatus status)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(DisciplinaryMeasure), id.ToString());
            entity.SetStatus(status);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }
}
