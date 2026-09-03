using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var secret = _configuration["JWT_SECRET"]
                     ?? _configuration["Jwt:Secret"]
                     ?? "BibliotecaChaveSuperSecretaParaAssinaturaJwt123456789";

        var issuer = _configuration["JWT_ISSUER"]
                     ?? _configuration["Jwt:Issuer"]
                     ?? "BibliotecaApi";

        var audience = _configuration["JWT_AUDIENCE"]
                       ?? _configuration["Jwt:Audience"]
                       ?? "BibliotecaClient";

        var expirationHoursStr = _configuration["JWT_EXPIRATION_HOURS"]
                                 ?? _configuration["Jwt:ExpirationHours"]
                                 ?? "8";

        if (!int.TryParse(expirationHoursStr, out var expirationHours))
        {
            expirationHours = 8;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("perfil", usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.Aluno != null)
        {
            claims.Add(new Claim("alunoId", usuario.Aluno.Id.ToString()));
            claims.Add(new Claim("matricula", usuario.Aluno.Matricula));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return (tokenString, expires);
    }
}
