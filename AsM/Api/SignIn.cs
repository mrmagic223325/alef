using System.Security.Claims;
using AsM.Models;
using Cassandra.Mapping;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AsM.Api;


[ApiController]
public class AuthController : ControllerBase
{

    private readonly DatabaseService _databaseService;
    
    public AuthController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    
    private readonly AuthenticationProperties Properties = new()
    {
        AllowRefresh = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        IsPersistent = true,
    };

    // TODO: Error Handling
    // TODO: Rework queries
    [HttpPost]
    [Route("api/auth/signin")]
    public async Task<IActionResult> SignInPost(SignInData value)
    {
        var (cluster, session) = await _databaseService.Connect("accounts");

        // Determine whether username or email is given and query the database accordingly
        var q = value.Username.Contains("@") ? "email" : "username";

        var st = await session.PrepareAsync($"SELECT id FROM users WHERE {q} = ? ALLOW FILTERING;");

        var res = await session.ExecuteAsync(st.Bind(value.Username));

        var id = res.FirstOrDefault().GetValue<Guid>("id");
        
        var r = await _databaseService.CheckPassword(id, value.Password);

        var v = new ValidationProblemDetails(new Dictionary<string, string[]>()
        {
            { "Password", new[] { "Password incorrect." }}
        });
        
        if (r == false)
            return BadRequest(v);

        try
        {
            MappingConfiguration.Global.Define(new Map<User>().TableName("users").PartitionKey(u => u.Id));
        }
        // TODO: Exception Handling
        catch (Exception e)
        {
            ;
        }
        IMapper mapper = new Mapper(session);
        
        st = await session.PrepareAsync($"SELECT * FROM users WHERE id = ?;");
        var x = await mapper.SingleAsync<User>("SELECT * FROM users WHERE id = ?", id);
        
        Console.WriteLine(x.Id);
        
        if (q != "email")
        {
            st = await session.PrepareAsync($"SELECT email FROM users WHERE id = ?;");
            res = await session.ExecuteAsync(st.Bind(id));
            value.Email = res.FirstOrDefault().GetValue<string>("email");
        }
        else
        {
            st = await session.PrepareAsync($"SELECT username FROM users WHERE id = ?;");
            res = await session.ExecuteAsync(st.Bind(id));
            value.Username = res.FirstOrDefault().GetValue<string>("username");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, value.Email),
            new Claim(ClaimTypes.Name, value.Username),
            new Claim("Guid", x.Id.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var p = Properties;

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            p);

        return Ok(new { Success = true });
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
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}