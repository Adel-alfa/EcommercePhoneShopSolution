using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;

namespace PhoneShopeClient.Services
{
    public interface IAccountService
    {
        Task<ServiceResponse> Register(UserDTO user);
        Task<LoginResponse> Login(LoginDTO model);
        Task <UserClaim> GetUserInfo();
    }
}
