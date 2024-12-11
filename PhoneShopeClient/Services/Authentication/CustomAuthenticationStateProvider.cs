using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace PhoneShopeClient.Services.Authentication
{
    public class CustomAuthenticationStateProvider(AuthenticationService authenticationService) : AuthenticationStateProvider
    {

        private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());
        private bool _authenticated = false;
        public async override  Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            // default to not authenticated
            var user = Unauthenticated;
            try
            {
                var getUserSession = await authenticationService.GetUserDetailsAsync();
                if (getUserSession is null || string.IsNullOrEmpty(getUserSession.Email))
                    return await Task.FromResult(new AuthenticationState(Unauthenticated));
                var claimPricipal = authenticationService.SetClaimPrincipal(getUserSession);
                _authenticated = true;
                return await Task.FromResult(new AuthenticationState(claimPricipal));
            }
            catch (Exception ex)
            {

                return await Task.FromResult(new AuthenticationState(Unauthenticated));
            }
        }
        public async  Task UpdateAuthenticationState(TokenProp tokenProp)
        {
            ClaimsPrincipal claimsPrincipal = new();
            if (tokenProp is not null && !string.IsNullOrEmpty(tokenProp!.Token))
            {
                await authenticationService.SetTokenToLocalStorage(General.SerializeObj(tokenProp));
                var getUserSession = await authenticationService.GetUserDetailsAsync();
                if(getUserSession is not null && !string.IsNullOrEmpty(getUserSession!.Email))
                    claimsPrincipal = authenticationService.SetClaimPrincipal(getUserSession);
            }
            else
            {
                claimsPrincipal = Unauthenticated;
                await authenticationService.RemoveTokenToLocalStorage();
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
