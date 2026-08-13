namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// Menu operations that identify an HR administrator, for the "owner OR HR" guards on
    /// self-service records.
    ///
    /// <para><b>Why these exist.</b> Those guards used to read
    /// <c>if (!scope.IsAdmin &amp;&amp; record.EmployeeId != mine) throw</c>. `IsAdmin` short-circuits on
    /// <c>IsHeadOffice()</c>, which is true for every employee in a single-branch tenant, so the whole
    /// condition was always false and the guard NEVER FIRED — any employee could cancel a colleague's
    /// loan, trip or guarantee. Checking a menu permission instead is the fix, because
    /// <see cref="IEndpointPermissionService"/> is strictly role-based with no head-office bypass
    /// (logic.md §11).</para>
    ///
    /// <para><b>How each link was chosen.</b> Each names the HR-SIDE register for the record, which
    /// ordinary staff do not hold — they hold the matching self-service screen instead
    /// (<c>/myLoans</c> vs <c>/loan</c>, <c>/myTrips</c> vs <c>/trip</c>, <c>/myGuarantees</c> vs
    /// <c>/employeeGuarantee</c>, <c>/myTraining</c> vs <c>/trainingNeed</c>). Verify that separation
    /// still holds before reusing one of these for a new guard: a link BOTH sides hold cannot
    /// discriminate, which is why grievances use <see cref="EmployeeRegister"/> — every employee holds
    /// <c>/grievance</c>, so it would have been useless here.</para>
    /// </summary>
    public static class HrScreens
    {
        /// <summary>The HR employee register — held only by Administrator and HR Admin.</summary>
        public static readonly string[] EmployeeRegister = ["employee"];

        /// <summary>HR loan register (staff hold <c>/myLoans</c>).</summary>
        public static readonly string[] LoanRegister = ["loan"];

        /// <summary>HR trip register (staff hold <c>/myTrips</c>).</summary>
        public static readonly string[] TripRegister = ["trip"];

        /// <summary>HR guarantee register (staff hold <c>/myGuarantees</c>).</summary>
        public static readonly string[] GuaranteeRegister = ["employeeGuarantee"];

        /// <summary>HR training-need register (staff hold <c>/myTraining</c>).</summary>
        public static readonly string[] TrainingNeedRegister = ["trainingNeed"];

        /// <summary>HR reward-nomination register.</summary>
        public static readonly string[] RewardNominationRegister = ["rewardNomination"];
    }
}
