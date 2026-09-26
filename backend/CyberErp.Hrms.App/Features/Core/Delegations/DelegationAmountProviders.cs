using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    /// <summary>
    /// The value of a loan request — the principal being borrowed.
    /// </summary>
    /// <remarks>
    /// Not the total repayable. A delegation ceiling is a statement about the decision being taken,
    /// and the decision is "may this person borrow this much"; interest is a consequence of the
    /// terms, not of the approver's authority.
    /// </remarks>
    public class LoanDelegationAmountProvider(IRepository<Loan> loans) : IDelegationAmountProvider
    {
        public bool Supports(string entityType) => entityType == WorkflowEntityTypes.EmployeeLoan;

        public async Task<decimal?> GetAmountAsync(string entityType, Guid entityId) =>
            await loans.GetAll().AsNoTracking()
                .Where(l => l.Id == entityId)
                .Select(l => (decimal?)l.PrincipalAmount)
                .FirstOrDefaultAsync();
    }

    /// <summary>
    /// The value of a medical claim — what was claimed, not what may eventually be approved.
    /// </summary>
    /// <remarks>
    /// ⚠️ <c>ClaimedAmount</c>, deliberately. <c>ApprovedAmount</c> is set BY the decision this
    /// ceiling governs, so using it would ask the limit to judge a figure that does not exist yet,
    /// and would read as null — i.e. unlimited — on every claim awaiting its first approval.
    /// </remarks>
    public class MedicalClaimDelegationAmountProvider(IRepository<MedicalClaim> claims) : IDelegationAmountProvider
    {
        public bool Supports(string entityType) => entityType == WorkflowEntityTypes.MedicalClaim;

        public async Task<decimal?> GetAmountAsync(string entityType, Guid entityId) =>
            await claims.GetAll().AsNoTracking()
                .Where(c => c.Id == entityId)
                .Select(c => (decimal?)c.ClaimedAmount)
                .FirstOrDefaultAsync();
    }

    /// <summary>
    /// The value of a salary revision — the PROPOSED salary.
    /// </summary>
    /// <remarks>
    /// ⚠️ The proposed figure, not the increase. A ceiling of 25,000 means "this stand-in may sign
    /// off salaries up to 25,000", which is the question an approver is actually answering. Using
    /// the delta would let a 4,000 rise onto a 200,000 salary pass a 5,000 ceiling — the largest
    /// salaries would be the easiest ones for a junior stand-in to approve.
    /// </remarks>
    public class SalaryRevisionDelegationAmountProvider(
        IRepository<SalaryRevisionLine> lines) : IDelegationAmountProvider
    {
        public bool Supports(string entityType) => entityType == WorkflowEntityTypes.SalaryRevision;

        public async Task<decimal?> GetAmountAsync(string entityType, Guid entityId)
        {
            // A revision is a batch: the ceiling has to clear the LARGEST salary in it, because
            // approving the batch approves every line in it.
            var max = await lines.GetAll().AsNoTracking()
                .Where(l => l.SalaryRevisionId == entityId)
                .Select(l => (decimal?)l.ProposedSalary)
                .MaxAsync();
            return max;
        }
    }
}
