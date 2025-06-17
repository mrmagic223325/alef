using System.Security.Claims;
using AsM.Interfaces;
using AsM.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AsM.Api;


[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICassandraDbContext _databaseService; // Kept for backward compatibility

    public AuthController(IAuthService authService, ICassandraDbContext databaseService)
    {
        _authService = authService;
        _databaseService = databaseService;
    }

    private readonly AuthenticationProperties _properties = new()
    {
        AllowRefresh = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        IsPersistent = true,
    };

    [HttpPost]
    [Route("api/auth/signin")]
    public async Task<IActionResult> SignInPost(SignInData value)
    {
        try
        {
            // Use the new AuthService to authenticate the user
            var user = await _authService.AuthenticateAsync(value.Account, value.Password);

            if (user == null || user.Id == null)
            {
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    { "Credentials", ["Incorrect credentials."] }
                }));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim("Guid", user.Id.ToString()!),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                _properties);

            return Ok(new { Success = true });
        }
        catch (Exception e)
        {
            Log.Error(e, "An unexpected error occurred during sign in.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
        }
    }

    [HttpPost]
    [Route("api/auth/signout")]
    public async Task<ActionResult> SignOutPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return this.Ok();
    }

    public class SignInData
    {
        public required string Account { get; set; }
        public required string Password { get; set; }
    }
}
