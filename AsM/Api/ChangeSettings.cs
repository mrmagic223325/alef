using System.Security.Claims;
using AsM.Data;
using AsM.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AsM.Api;

[ApiController]
public class ChangeSettings : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICassandraDbContext _databaseService; // Kept for backward compatibility

    public ChangeSettings(IUserService userService, ICassandraDbContext databaseService)
    {
        _userService = userService;
        _databaseService = databaseService;
    }

    private static readonly AuthenticationProperties Properties = new()
    {
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        IsPersistent = true,
    };

    [HttpPost]
    [Route("api/settings/change")]
    public async Task<ActionResult> ChangeSettingsPost(ChangeSettingsData value)
    {
        try
        {
            // Validate the setting type
            string claimType;
            switch (value.Type)
            {
                case "Displayname":
                    claimType = "Displayname";
                    break;
                case "Username":
                    claimType = ClaimTypes.Name;
                    break;
                case "Email":
                    claimType = ClaimTypes.Email;
                    break;
                default:
                    return BadRequest("Invalid setting type");
            }

            // Get the user ID from claims
            var guidString = User.FindFirst("Guid")?.Value;
            if (string.IsNullOrEmpty(guidString))
            {
                return Unauthorized("User not authenticated properly");
            }

            var userId = new Guid(guidString);

            // Update the user setting using the UserService
            var success = await _userService.UpdateUserSettingAsync(userId, value.Type, value.Data);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update user setting");
            }

            // Update the claims
            var newClaims = new List<Claim>();
            foreach (var claim in User.Clone().Claims.Where(c => c.Type != claimType))
            {
                newClaims.Add(new Claim(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer));
            }
            newClaims.Add(new Claim(claimType, value.Data));

            var newIdentity = new ClaimsIdentity(newClaims, User.Identity?.AuthenticationType);
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            // Update the current user claims
            HttpContext.User = newPrincipal;

            // Sign out and sign in with new claims
            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, Properties);

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while changing settings: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public class ChangeSettingsData
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
