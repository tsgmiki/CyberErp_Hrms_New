namespace CyberErp.Hrms.App.Features.Core.Performance
{
    /// <summary>
    /// Extension point invoked after an appraisal is finalized (HC147). Other modules react to a completed
    /// appraisal without the Performance module depending on them — e.g. Career Development refreshes the
    /// employee's succession readiness (HC153). Registered like any DI service; invocation is best-effort.
    /// </summary>
    public interface IAppraisalCompletedHandler
    {
        Task OnAppraisalCompletedAsync(Guid appraisalId, Guid employeeId);
    }
}
