using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace WebApplicationWithVue.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDbService _dbService;

    public AuthController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginInformation loginInformation)
    {
       var rows = await _dbService.RegisterAccountAsync(loginInformation);

        return Ok(new { message = "Register successful.", rows });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginInformation loginInformation)
    {
        if (_dbService.LoginAsync(loginInformation.UserName, loginInformation.Password).Result)
        {
            (string access_tokenString, string refresh_tokenString) = JwtGenerator.NewToken();

            _dbService.SaveRefreshToken(refresh_tokenString);

            return Ok(new { access_token = access_tokenString, refresh_token = refresh_tokenString });

        } else
        {
            return BadRequest();
        }
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshToken refreshToken)
    {
        var token = _dbService.RefreshToken(refreshToken.TokenString);
        if(token.Result)
        {
            (string access_tokenString, string refresh_tokenString) = JwtGenerator.NewToken();
            return Ok(new {access_token = access_tokenString, refresh_token = refreshToken.TokenString });
        }
        return BadRequest();
    }
}

public class RefreshToken
{
    public string TokenString { get; set; }
}

public static class JwtGenerator
{
    public static (string, string) NewToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WPCBOXRYKSEFUTWL6QT6RXHYD424JFB3"));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "user@example.com"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var aToken = new JwtSecurityToken(
            issuer: "your-issuer",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: signingCredentials
        );
        var rToken = new JwtSecurityToken(
            issuer: "your-issuer",
            audience: "your-audience",
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var access_tokenString = tokenHandler.WriteToken(aToken);
        var refresh_tokenString = tokenHandler.WriteToken(rToken);

        return (access_tokenString, refresh_tokenString);
    }
}

public class LoginInformation
{
    public string UserName { get; set; }
    public string Password { get; set; }
}