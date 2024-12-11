using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;

namespace PhoneShopServer.Repositories
{
    public interface IAuthentication
    {        
        Task<ServiceResponse> Register(UserDTO user);
        Task<LoginResponse> Login(LoginDTO model);
        Task<UserClaim> GetUserByToken(string token);
        Task<LoginResponse> GetRefreshToken(PostRefreshTokenDTO token);
    }
}
