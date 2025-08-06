using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UzlezzBlogs.Core.Configs;

namespace UzlezzBlogs.Services;

public class TokenValidatorService : ITokenValidatorService
{
    private TokenValidationParameters _parameters;
    private JwtSecurityTokenHandler _handler = new();

    public TokenValidatorService(IOptions<JwtConfig> config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key));
        _parameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = config.Value.Issuer,
            ValidAudience = config.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<AuthorizedUser?> ValidateAsync(string token)
    {
        var result = await _handler.ValidateTokenAsync(token, _parameters);
        if (!result.IsValid) return null;

        var id = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var name = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        var email = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (id is null || name is null || email is null) return null;
        return new AuthorizedUser(id!, name!, email!);
    }
}
