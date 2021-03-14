using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using T3App.API.Authentication;
using T3App.Shared;

namespace T3App.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;

        public AccountController(ILogger<AccountController> logger, IUserService userService, IJwtAuthManager jwtAuthManager)
        {
            _logger = logger;
            _userService = userService;
            _jwtAuthManager = jwtAuthManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginModel loginModel)
        {
            if (!ModelState.IsValid)  return BadRequest();
            if (!_userService.IsValidUserCredentials(loginModel.Email, loginModel.Password)) return Unauthorized();

            var role = _userService.GetUserRole(loginModel.Email);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,loginModel.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var jwtResult = _jwtAuthManager.GenerateTokens(loginModel.Email, claims, DateTime.Now);
            _logger.LogInformation($"User [{loginModel.Email}] logged in the system.");

            return Ok(new LoginResult
            {
                Email = loginModel.Email,
                Role = role,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString,
                Successful = true
            });
        }

        [HttpGet("user")]
        [Authorize]
        public ActionResult GetCurrentUser()
        {
            return Ok(new LoginResult
            {
                Email = User.Identity?.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
                OriginalEMail = User.FindFirst("OriginalEmail")?.Value
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // optionally "revoke" JWT token on the server side --> add the current token to a block-list
            // https://github.com/auth0/node-jsonwebtoken/issues/375

            var userName = User.Identity?.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userName);
            _logger.LogInformation($"User [{userName}] logged out the system.");
            return Ok();
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var userName = User.Identity?.Name;
                _logger.LogInformation($"User [{userName}] is trying to refresh JWT token.");

                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);
                _logger.LogInformation($"User [{userName}] has refreshed JWT token.");
                return Ok(new LoginResult
                {
                    Email = userName,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); // return 401 so that the client side can redirect the user to login page
            }
        }

        [HttpPost("impersonation")]
        [Authorize(Roles = UserRoles.Admin)]
        public ActionResult Impersonate([FromBody] ImpersonationRequest request)
        {
            var userName = User.Identity?.Name;
            _logger.LogInformation($"User [{userName}] is trying to impersonate [{request.Email}].");

            var impersonatedRole = _userService.GetUserRole(request.Email);
            if (string.IsNullOrWhiteSpace(impersonatedRole))
            {
                _logger.LogInformation($"User [{userName}] failed to impersonate [{request.Email}] due to the target user not found.");
                return BadRequest($"The target user [{request.Email}] is not found.");
            }
            if (impersonatedRole == UserRoles.Admin)
            {
                _logger.LogInformation($"User [{userName}] is not allowed to impersonate another Admin.");
                return BadRequest("This action is not supported.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,request.Email),
                new Claim(ClaimTypes.Role, impersonatedRole),
                new Claim("OriginalEmail", userName ?? string.Empty)
            };

            var jwtResult = _jwtAuthManager.GenerateTokens(request.Email, claims, DateTime.Now);
            _logger.LogInformation($"User [{request.Email}] is impersonating [{request.Email}] in the system.");
            return Ok(new LoginResult
            {
                Email = request.Email,
                Role = impersonatedRole,
                OriginalEMail = userName,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        [HttpPost("stop-impersonation")]
        public ActionResult StopImpersonation()
        {
            var userName = User.Identity?.Name;
            var originalUserName = User.FindFirst("OriginalEmail")?.Value;
            if (string.IsNullOrWhiteSpace(originalUserName))
            {
                return BadRequest("You are not impersonating anyone.");
            }
            _logger.LogInformation($"User [{originalUserName}] is trying to stop impersonate [{userName}].");

            var role = _userService.GetUserRole(originalUserName);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,originalUserName),
                new Claim(ClaimTypes.Role, role)
            };

            var jwtResult = _jwtAuthManager.GenerateTokens(originalUserName, claims, DateTime.Now);
            _logger.LogInformation($"User [{originalUserName}] has stopped impersonation.");
            return Ok(new LoginResult
            {
                Email = originalUserName,
                Role = role,
                OriginalEMail = null,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }
    }

    //public class LoginRequest
    //{
    //    [Required]
    //    [JsonPropertyName("username")]
    //    public string UserName { get; set; }

    //    [Required]
    //    [JsonPropertyName("password")]
    //    public string Password { get; set; }
    //}

    //public class LoginResult
    //{
    //    [JsonPropertyName("username")]
    //    public string UserName { get; set; }

    //    [JsonPropertyName("role")]
    //    public string Role { get; set; }

    //    [JsonPropertyName("originalUserName")]
    //    public string OriginalEmail { get; set; }

    //    [JsonPropertyName("accessToken")]
    //    public string AccessToken { get; set; }

    //    [JsonPropertyName("refreshToken")]
    //    public string RefreshToken { get; set; }
    //}

    //public class RefreshTokenRequest
    //{
    //    [JsonPropertyName("refreshToken")]
    //    public string RefreshToken { get; set; }
    //}

    //public class ImpersonationRequest
    //{
    //    [JsonPropertyName("email")]
    //    public string Email { get; set; }
    //}
}
