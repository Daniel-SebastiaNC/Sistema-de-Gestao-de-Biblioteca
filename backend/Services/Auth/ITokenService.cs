using Models;

namespace Services;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
