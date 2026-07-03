namespace CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser
{
    using CyberErp.Hrms.App.Features.Core.Users.DTOs;

    public interface IGetCurrentUserRepository
    {
        Task<CurrentUserResult> GetAsync(CancellationToken cancellationToken = default);
    }
}
