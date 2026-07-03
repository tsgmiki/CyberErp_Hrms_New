namespace CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser
{
    using CyberErp.Hrms.App.Features.Core.Users.DTOs;

    public interface IGetCurrentUser
    {
        Task<CurrentUserResult> GetAsync(CancellationToken cancellationToken = default);
    }
}
