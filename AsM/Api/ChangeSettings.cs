using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AsM.Api;

[ApiController]
public class ChangeSettings : ControllerBase
{
    private static readonly AuthenticationProperties Properties = new()
    {
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        IsPersistent = true,
    };


    // TODO: Error Handling
    [HttpPost]
    [Route("api/settings/change")]
    public async Task<ActionResult> ChangeSettingsPost(ChangeSettingsData value)
    {

        var (cluster, session) = await DbHelper.Connect("accounts");

        var st = await session.PrepareAsync("UPDATE users SET displayname = ? WHERE id = ?");

        var res = await session.ExecuteAsync(st.Bind(value.Data, new Guid(User.FindFirst("Guid").Value)));

        var nc = new List<Claim>();
        
        foreach (var claim in User.Clone().Claims.Where(c => c.Type != "Displayname"))
        {
            nc.Add(new Claim(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer));
        }
        
        nc.Add(new Claim("Displayname", value.Data));

        var ni = new ClaimsIdentity(nc, User.Identity.AuthenticationType);

        var np = new ClaimsPrincipal(ni);

        HttpContext.User = np;

        await HttpContext.SignOutAsync();
        await HttpContext.SignInAsync(np);

        return Ok();
    }

    public class ChangeSettingsData
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}