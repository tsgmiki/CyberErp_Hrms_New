namespace CyberErp.Hrms.App.Common.Services
{
    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        string? GetCurrentUserName();
    }
}

