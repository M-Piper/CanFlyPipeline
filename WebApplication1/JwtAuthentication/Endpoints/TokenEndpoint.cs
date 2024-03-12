using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CanFlyPipeline.JwtAuthentication.Endpoints;

public static class TokenEndpoint
{
    //handles requests from /connect/token
    //removed async but may need alternate solution
    public static Task<IResult> Connect(
        HttpContext ctx,
        JwtOptions jwtOptions)
    {
        throw new NotImplementedException();
    }

    //
    static (string, DateTime) CreateAccessToken(
        JwtOptions jwtOptions,
        string username,
        string[] permissions)
    {
        throw new NotImplementedException();
    }

    static string CreateAccessToken(
    JwtOptions jwtOptions,
    string username,
    TimeSpan expiration,
    string[] permissions)
    {
        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);
        var symmetricKey = new SymmetricSecurityKey(keyBytes);

        var signingCredentials = new SigningCredentials(
            symmetricKey,
            // one of the most popular. 
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
    {
        new Claim("sub", username),
        new Claim("name", username),
        new Claim("aud", jwtOptions.Audience)
    };

        var roleClaims = permissions.Select(x => new Claim("role", x));
        claims.AddRange(roleClaims);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.Add(expiration),
            signingCredentials: signingCredentials);

        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
        return rawToken;
    }
}
