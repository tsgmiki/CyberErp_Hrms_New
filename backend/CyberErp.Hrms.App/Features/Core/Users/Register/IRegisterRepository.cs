using CyberErp.Hrms.App.Features.Core.Users.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Users.Register
{
    public interface IRegisterRepository
    {
        Task<RegisterResult> RegisterAsync(RegisterUserDto dto);
        Task<RegisterResult> RegisterWithGoogleAsync(RegisterWithGoogleDto dto);
    }
}
