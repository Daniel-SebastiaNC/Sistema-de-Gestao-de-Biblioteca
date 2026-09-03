using DTO;

namespace Services;

public interface IAuthService
{
    Task<LoginResponseDTO> AutenticarAsync(LoginDTO dto);
    Task<UsuarioResponseDTO> RegistrarAsync(CriarUsuarioDTO dto);
    Task<UsuarioResponseDTO> ObterUsuarioAtualAsync(Guid usuarioId);
    Task<List<UsuarioResponseDTO>> ListarUsuariosAsync();
}
