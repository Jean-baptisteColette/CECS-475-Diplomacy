namespace Diplomacy.Controllers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Diplomacy.Data.Entities;
    using Diplomacy.Data.Repositories;
    using Diplomacy.Models;
    using Diplomacy.Models.AuthModels;
    using Diplomacy.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "ApiUser")]
    public class AuthController : ControllerBase
    {

        public AuthController(
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationSchemeProvider schemeProvider,
            IConfiguration config,
            IOptions<JwtIssuerOptions> jwtOptions,
            IJwtFactory jwtFactory,
            IAuthRepository authRepository)
        {
            this.UserManager = userManager;
            this.Config = config;
            this._jwtOptions = jwtOptions.Value;
            this._jwtFactory = jwtFactory;
            this.AuthRepository = authRepository;
        }
        private UserManager<User> UserManager { get; }

        private IConfiguration Config { get; set; }

        private readonly JwtIssuerOptions _jwtOptions;

        private readonly IJwtFactory _jwtFactory;

        public IAuthRepository AuthRepository { get; }

        [HttpPost]
        public ActionResult Register([FromBody]RegisterModel model)
        {
            return this.Ok();
        }

        [HttpPost]
        public ActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            return this.Ok();
        }

        #region Helpers

        private object CreateToken(User user)
        {
            var claims = new[]
                             {
                                 new Claim(ClaimTypes.Name, user.Email),
                                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                 new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                this.Config["Tokens:Issuer"],
                this.Config["Tokens:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds);

            var results = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            };
            return results;
        }
        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await UserManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            try
            {
                // check the credentials
                if (await UserManager.CheckPasswordAsync(userToVerify, password))
                {
                    return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }
            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
        #endregion Helpers
    }
}
