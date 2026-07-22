using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// Workflow outcomes for personnel movements (Transfer / Promotion / Demotion): final approval
    /// executes the movement immediately when due, or parks it as Approved until its effective date
    /// (HC176 — the daily scheduler applies it on the date); rejection cancels it.
    /// </summary>
    public class EmployeeMovementWorkflowHandler(
        IApproveEmployeeMovement approveHandler,
        ICancelEmployeeMovement cancelHandler) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            entityType.StartsWith("EmployeeMovement", StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) =>
            approveHandler.ApproveAsync(entityId);

        public Task OnRejectedAsync(string entityType, Guid entityId) =>
            cancelHandler.CancelAsync(entityId);
    }

    /// <summary>
    /// Workflow outcomes for reward nominations (HC179/HC186): final approval grants the recognition
    /// (crediting points and raising any monetary disbursement); rejection closes the nomination.
    /// </summary>
    public class RewardNominationWorkflowHandler(
        Rewards.IApproveRewardNomination approveHandler) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.RewardNomination, StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) =>
            approveHandler.ApproveAsync(entityId);

        public Task OnRejectedAsync(string entityType, Guid entityId) =>
            approveHandler.RejectAsync(entityId);
    }

    /// <summary>
    /// Workflow outcomes for training needs (HC188/HC201) — the chain is configured per need type
    /// ("TrainingNeed.Local" / "TrainingNeed.Abroad"): approval marks the need Approved (fulfilment
    /// follows from a completed enrollment); rejection closes it.
    /// </summary>
    public class TrainingNeedWorkflowHandler(
        Training.ITrainingNeedDecision decisionHandler) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            entityType.StartsWith("TrainingNeed", StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) =>
            decisionHandler.ApproveAsync(entityId);

        public Task OnRejectedAsync(string entityType, Guid entityId) =>
            decisionHandler.RejectAsync(entityId);
    }

    /// <summary>
    /// Workflow outcomes for termination cases: approval opens the departmental clearance
    /// checklist; rejection cancels the case.
    /// </summary>
    public class EmployeeTerminationWorkflowHandler(
        IRepository<EmployeeTermination> repository,
        IRepository<TerminationClearance> clearanceRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<CompanyAsset> assetRepository,
        IRepository<TerminationAssetRecovery> recoveryRepository,
        ITerminationNotifier terminationNotifier) : IWorkflowEntityHandler
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
            // HC215 — the asset-recovery checklist opens with the clearance phase.
            await AssetRecoveryShared.GenerateChecklistAsync(termination.Id, termination.EmployeeId,
                termination.TenantId, assetRepository, recoveryRepository);
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            await terminationNotifier.ApprovedAsync(entityId);   // HC220 — best-effort
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == entityId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), entityId.ToString());
            termination.Cancel();
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            await terminationNotifier.CancelledAsync(entityId);  // HC220 — best-effort
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
    /// <summary>HC242 — final approval computes the reimbursable amount and approves the claim; rejection rejects it.</summary>
    public class MedicalClaimWorkflowHandler(
        IRepository<MedicalClaim> repository,
        IRepository<MedicalPlan> planRepository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.MedicalClaim, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(MedicalClaim), entityId.ToString());
            var amount = await Medical.MedicalCoverage.ComputeAsync(planRepository, repository, entity, null);
            entity.Approve(amount, "Approved via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(MedicalClaim), entityId.ToString());
            entity.Reject("Rejected via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcome for an insurance coverage claim (HC249): final approval approves the full claimed
    /// amount (HR can adjust before submitting when running direct); rejection rejects it.
    /// </summary>
    public class InsuranceClaimWorkflowHandler(
        IRepository<InsuranceClaim> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.InsuranceClaim, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(InsuranceClaim), entityId.ToString());
            entity.Approve(entity.ClaimedAmount, "Approved via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(InsuranceClaim), entityId.ToString());
            entity.Reject("Rejected via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcome for an employee loan (HC251/HC259): final approval moves the loan to Approved
    /// (ready for disbursement); rejection rejects it.
    /// </summary>
    public class LoanWorkflowHandler(
        IRepository<Loan> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.EmployeeLoan, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(Loan), entityId.ToString());
            entity.Approve("Approved via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(Loan), entityId.ToString());
            entity.Reject("Rejected via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Workflow outcome for a business trip (HC261 — both Local and International chains): final approval
    /// approves the trip (ready for advance disbursement); rejection rejects it.
    /// </summary>
    public class TripRequestWorkflowHandler(
        IRepository<TripRequest> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            entityType.StartsWith("TripRequest.", StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(TripRequest), entityId.ToString());
            entity.Approve("Approved via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(TripRequest), entityId.ToString());
            entity.Reject("Rejected via workflow");
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>HC228 — approval confirms a salary revision (ready to apply); rejection cancels it.</summary>
    public class SalaryRevisionWorkflowHandler(
        IRepository<SalaryRevision> repository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.SalaryRevision, StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId) => TransitionAsync(entityId, approve: true);
        public Task OnRejectedAsync(string entityType, Guid entityId) => TransitionAsync(entityId, approve: false);

        private async Task TransitionAsync(Guid id, bool approve)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());
            if (approve) entity.Approve(); else entity.Reject();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class DisciplinaryMeasureWorkflowHandler(
        IRepository<DisciplinaryMeasure> repository,
        Employees.IDisciplinaryNotifier notifier) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.DisciplinaryMeasure, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            await SetStatusAsync(entityId, DisciplinaryStatus.Resolved);
            await notifier.ApprovedAsync(entityId);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            await SetStatusAsync(entityId, DisciplinaryStatus.Cancelled);
            await notifier.CancelledAsync(entityId);
        }

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
