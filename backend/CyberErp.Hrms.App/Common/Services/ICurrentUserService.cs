namespace CyberErp.Hrms.App.Common.Services
{
    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        string? GetCurrentUserName();

        /// <summary>The branch the current user administers (null for Head Office / unassigned).</summary>
        Guid? GetCurrentBranchId();

        /// <summary>True when the current user has global (Head Office) visibility across all branches.</summary>
        bool IsHeadOffice();
    }
}
