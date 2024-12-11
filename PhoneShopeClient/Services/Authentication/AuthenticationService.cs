using Blazored.LocalStorage;
using Microsoft.AspNetCore.WebUtilities;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace PhoneShopeClient.Services.Authentication
{
    public class AuthenticationService
    {
        private readonly ILocalStorageService localStorageService;
        private readonly HttpClient httpClient;
        public AuthenticationService(ILocalStorageService _localStorageService, HttpClient _httpClient)
        { 
            localStorageService = _localStorageService;
            httpClient = _httpClient;
        }
        public async Task<UserClaim> GetUserDetailsAsync()
        {
            var token = await GetTokenFromLocalStorage();
            if (string.IsNullOrWhiteSpace(token)) return null!;
            var httpClient = await AddHeaderToHttpClientAsync();
            var user = General.DeserializeJsonString<TokenProp>(token);
            if (user == null || string.IsNullOrEmpty(user.Token)) return null!;

            var response = await GetUserDetailsFromApi();
            if (response.IsSuccessStatusCode) return await GetUserSession(response);

            if (httpClient.DefaultRequestHeaders.Contains("Authorization") &&
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return await HandleUnauthorizedResponse(user.RefreshToken);
            return null;
        }
        private async Task<UserClaim> HandleUnauthorizedResponse(string refreshToken)
        {
            var validToken = GenerateEncodedToken(refreshToken);
            var model = new PostRefreshTokenDTO() { RefrshToken = validToken };

            var result =  await httpClient.PostAsync("api/account/refresh-token",
                    General.GenerateStringContent(General.SerializeObj(model)));

                if (result.IsSuccessStatusCode && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var apiResponse = await result.Content.ReadAsStringAsync();   
                    var loginResponse = General.DeserializeJsonString<LoginResponse>(apiResponse);
                    await SetTokenToLocalStorage(General.SerializeObj(new TokenProp()
                    {
                        Token = loginResponse.Token,
                        RefreshToken = loginResponse.RefreshToken,
                    }));
                    var callApiAgain = await GetUserDetailsFromApi();
                    if (!callApiAgain.IsSuccessStatusCode)
                    {
                        return await GetUserSession(callApiAgain);
                    }
                }
            
            return null!;
        }

    private static string GenerateEncodedToken(string token)
    {
        var encodedToken = Encoding.UTF8.GetBytes(token);
        return WebEncoders.Base64UrlEncode(encodedToken);
    }
    private async   Task<string> GetTokenFromLocalStorage()
        {
            return await localStorageService.GetItemAsStringAsync("access_token");
        }
        public async Task<HttpClient> AddHeaderToHttpClientAsync()
        {
            var token = await GetTokenFromLocalStorage();//AI
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (token == null) 
                return null!;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
                        ("Bearer", General.DeserializeJsonString<TokenProp>(await GetTokenFromLocalStorage()).Token);
            return httpClient;
        }
        private async Task<HttpResponseMessage> GetUserDetailsFromApi()
        {
            var httpClient = await AddHeaderToHttpClientAsync();
            return await httpClient.GetAsync("api/account/user-info");
        }
        public static async Task<UserClaim> GetUserSession(HttpResponseMessage response)
        {
            var apiResponse = await response.Content.ReadAsStringAsync();
            return General.DeserializeJsonString<UserClaim>(apiResponse);
        }
        public async Task SetTokenToLocalStorage(string token)
        {
            await localStorageService.SetItemAsStringAsync("access_token", token);
        }

        public   ClaimsPrincipal SetClaimPrincipal(UserClaim userClaims)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>
                {
                    new(ClaimTypes.Name, userClaims.Name!),
                    new(ClaimTypes.Email, userClaims.Email!),
                    new(ClaimTypes.Role, userClaims.Role!)
                }, "AccessTokenAuth"));
        }
        public async Task RemoveTokenToLocalStorage()
        {
            await localStorageService.RemoveItemAsync("access_token");
        }
    }

    
}
