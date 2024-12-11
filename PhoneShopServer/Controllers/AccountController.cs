using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Repositories;

namespace PhoneShopServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAuthentication authenticationService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse>> RegisterAsync(UserDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await authenticationService.Register(model);
            if (res.flag)
                return Ok(res);
            else return BadRequest(res);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginDTO model)
        {
             if (!ModelState.IsValid) 
                    return BadRequest(ModelState);
            
            var res = await authenticationService.Login(model);
             

                return Ok(res);
            
        }
        [HttpGet("user-Info")]
        public async Task<ActionResult> GetUserInfo()
        {
           
            var token = GetTokenFromHeader();
            if(string.IsNullOrEmpty( token)) return Unauthorized();

            var user = await authenticationService.GetUserByToken(token);
            if(user == null || string.IsNullOrEmpty(user.Email)) return Unauthorized();
            return Ok(user);

        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponse>> RefreshToken(PostRefreshTokenDTO model)
        {
            if (!ModelState.IsValid)
                return Unauthorized();

            var res = await authenticationService.GetRefreshToken(model);
            if (res is null) return Unauthorized();
            return Ok(res);
        }
        private string GetTokenFromHeader()
        {
            var token = string.Empty;
            foreach (var header in Request.Headers)
            {
                if(header.Key.ToString() == "Authorization")
                {
                    token = header.Value.ToString();
                    break;
                }
            }
            return  token.Split(" ").LastOrDefault()!;
        }
    }
}
