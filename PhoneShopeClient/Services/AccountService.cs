using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using System.Reflection;

namespace PhoneShopeClient.Services
{
    public class AccountService(HttpClient httpClient) : IAccountService
    {
        private const string AuthenticationBaseUrl = "api/account";

        public async Task<UserClaim> GetUserInfo()
        {
            var result = await httpClient.GetAsync($"{AuthenticationBaseUrl}/user-Info");
            if (!result.IsSuccessStatusCode)
                return null;
            var apiResponse = await result.Content.ReadAsStringAsync();
            var userclam = General.DeserializeJsonString<UserClaim>(apiResponse)!;
            return userclam;
        }
      
        public async Task<LoginResponse> Login(LoginDTO model)
        {
            var result = await httpClient.PostAsync($"{AuthenticationBaseUrl}/login",
                          General.GenerateStringContent(General.SerializeObj(model)));
            if (!result.IsSuccessStatusCode)
                return new LoginResponse(false, "une erreur s’est produite !");
            var apiResponse = await result.Content.ReadAsStringAsync();
            return General.DeserializeJsonString<LoginResponse>(apiResponse);
        }

        public async Task<ServiceResponse> Register(UserDTO user)
        {
            var response = await httpClient.PostAsync($"{AuthenticationBaseUrl}/register", General.GenerateStringContent(General.SerializeObj(user)));
            if (!response.IsSuccessStatusCode)
                return new ServiceResponse(false, "Une erreur s’est produite, réessayez plus tard!");
            var apiResponse =  await response.Content.ReadAsStringAsync();
            return General.DeserializeJsonString<ServiceResponse>(apiResponse);
        }
    }
}
