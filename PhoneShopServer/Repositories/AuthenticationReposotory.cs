using Azure.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Data;
using System.Security.Cryptography;
using System.Text;

namespace PhoneShopServer.Repositories
{
    public class AuthenticationReposotory(AppDbContext appDbContext) : IAuthentication
    {
        public async Task<ServiceResponse> Register(UserDTO model)
        {
            if (await UserExists(model.Email)) return new ServiceResponse(false, "L'utilisateur est déjà enregistré");

            var user = new ApplicationUser()
            {
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Name = model.Name,
                Email = model.Email,
            };
             user =  appDbContext.ApplicationUser.Add(user).Entity;
            await Commit();
            // assign Role
            var IsAdminRoleCreated = await appDbContext.ApplicationRole.FirstOrDefaultAsync(r=>r.Name!.ToLower().Equals("admin"));
            if (IsAdminRoleCreated is null)
            {
                await AssignRole(user, "Admin");
            }
            else
            {
                await AssignRole(user, "User"); 
            }
            return new ServiceResponse(true, "L'utilisateur est enregistré avec succès");
        }

        private async Task AssignRole(ApplicationUser user, string roleName)
        {
            var role = await appDbContext.ApplicationRole.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (role == null)
            {
                role = new ApplicationRole { Name = roleName };
                appDbContext.ApplicationRole.Add(role);
                await Commit();
            }

            appDbContext.ApplicationUserRole.Add(new ApplicationUserRole { RoleId = role.Id, UserId = user.Id });
            await Commit();
        }
        public async Task<LoginResponse> GetRefreshToken(PostRefreshTokenDTO token)
        {
            var decodedToken = WebEncoders.Base64UrlDecode(token.RefrshToken!);
            var TokenStr = Encoding.UTF8.GetString(decodedToken);
            var dbToken = appDbContext.ApplicationTokenInfo.FirstOrDefault(_=>_.RefreshToken == TokenStr);
            //var dbToken = appDbContext.ApplicationTokenInfo.FirstOrDefault(_ => _.RefreshToken == token.RefrshToken); //api
            if (dbToken == null) return null!;

            var(newAccessToken, newRefreshToken) = await GenerateTokens();
            await SaveTokenInfo(dbToken.Id, newAccessToken, newRefreshToken);
            return new LoginResponse(true, "token-refreshed", newAccessToken, newRefreshToken);
        }

        public async Task<UserClaim> GetUserByToken(string token)
        {
            var tokenInfo = await appDbContext.ApplicationTokenInfo.FirstOrDefaultAsync(t=> t.AccessToken!.Equals(token));
            if (tokenInfo is null) return null!;
            var userInfo = await appDbContext.ApplicationUser.FirstOrDefaultAsync(u=>u.Id == tokenInfo.UserId);
            if (userInfo is null || tokenInfo.ExpiredDate < DateTime.Now) return null!;
            var userRole = await appDbContext.ApplicationUserRole.FirstOrDefaultAsync(r=> r.UserId == userInfo.Id);
            if(userRole is null) return null!;
            var roleName = await appDbContext.ApplicationRole.FirstOrDefaultAsync(_ => _.Id == userRole.RoleId);
            if (roleName is null) return null!;

            return new UserClaim() { Email = userInfo.Email, Name = userInfo.Name, Role = roleName.Name };
        }

        public async Task<LoginResponse> Login(LoginDTO model)
        {
            if (model == null) 
                return new LoginResponse(false, "Modèle invalide Réessayez!");
            var user = await appDbContext.ApplicationUser.FirstOrDefaultAsync(u => u.Email!.Equals(model.Email));
            if (user == null)
                return new LoginResponse(false, "L'utilisateur n'existe pas");
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return new LoginResponse(false, "Le nom d'utilisateur ou le mot de passe est incorrect !");
          
            var (accessToken, refreshToken) = await GenerateTokens();
            await SaveTokenInfo(user.Id, accessToken, refreshToken);
            return new LoginResponse(true,"Token", accessToken, refreshToken);
        }   
        private async Task SaveTokenInfo(int userId, string accessToken, string refreshToken)
        {
            var tokenInfo = appDbContext.ApplicationTokenInfo.FirstOrDefault(t => t.UserId == userId);
            if(tokenInfo == null)
            {
                appDbContext.ApplicationTokenInfo.Add(new ApplicationTokenInfo() 
                { 
                    UserId = userId ,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                });                
            }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.AccessToken = accessToken;
                tokenInfo.ExpiredDate = DateTime.Now.AddDays(1);               
            } 
            await Commit();
        }
        private async   Task <bool> VerifyToken (string refreshToken = null! ,string accessToken = null!)
        {           
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token =  await appDbContext.ApplicationTokenInfo.FirstOrDefaultAsync(u => u.RefreshToken!.Equals(refreshToken));
                return token == null ;
            }
            else
            {
                var token = await appDbContext.ApplicationTokenInfo.FirstOrDefaultAsync(u => u.AccessToken!.Equals(accessToken));
                return token is null ;
            }
        }
        private async Task<(string AccessToken,string RefreshToken)> GenerateTokens()
        {
            string accessToken = GenerateToken(256);
            string refreshToken = GenerateToken(64);
            
            while (!await VerifyToken(accessToken : accessToken))
                accessToken = GenerateToken(256);

            while (!await VerifyToken(refreshToken : refreshToken))
                refreshToken = GenerateToken(64);
            return (accessToken, refreshToken);
        }
        private static string GenerateToken(int numberOfBytes) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(numberOfBytes));
        private async Task Commit() => await appDbContext.SaveChangesAsync();

        private async Task<bool> UserExists(string email)=>  await appDbContext.ApplicationUser.AnyAsync(u => u.Email!.ToLower() == email.ToLower());
        
    }
}
