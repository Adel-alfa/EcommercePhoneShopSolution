
namespace PhoneShopSharedLibrary.Responses
{
    public record  class ServiceResponse(bool flag, string message=null!);
    public record class LoginResponse(bool flag, string message,string Token = null!,string RefreshToken = null!);
    
}
