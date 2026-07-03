using CyberErp.Hrms.App.Features.Core.Users.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Users.Login
{
    public interface ILoginUser
    {
        Task<UserResult> Loginsync(LoginUserDto dto);
    }
}

