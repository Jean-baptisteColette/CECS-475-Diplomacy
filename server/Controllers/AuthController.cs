namespace Diplomacy.Controllers
{
    using AutoMapper.Configuration;
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
    using Microsoft.Extensions.Options;

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
    }
}
