using DTO;
using Exceptions;
using Models;
using Repository;

namespace Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuditoriaService _auditoriaService;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        IAuditoriaService auditoriaService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _auditoriaService = auditoriaService;
    }

    public async Task<LoginResponseDTO> AutenticarAsync(LoginDTO dto)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
        if (usuario == null)
        {
            throw new BadRequestException("Credenciais inválidas: e-mail ou senha incorretos.");
        }

        if (!usuario.Ativo)
        {
            throw new BadRequestException("Acesso negado: o usuário está desativado.");
        }

        bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
        if (!senhaValida)
        {
            throw new BadRequestException("Credenciais inválidas: e-mail ou senha incorretos.");
        }

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        await _auditoriaService.RegistrarAcaoAsync(
            usuario.Email,
            "LOGIN",
            $"Login bem-sucedido com perfil {usuario.Perfil}"
        );

        return new LoginResponseDTO
        {
            Token = token,
            Tipo = "Bearer",
            ExpiraEm = expiraEm,
            Usuario = new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil.ToString(),
                AlunoId = usuario.Aluno?.Id,
                Matricula = usuario.Aluno?.Matricula
            }
        };
    }

    public async Task<UsuarioResponseDTO> RegistrarAsync(CriarUsuarioDTO dto)
    {
        if (await _usuarioRepository.ExistsByEmailAsync(dto.Email))
        {
            throw new ConflictException($"O e-mail '{dto.Email}' já está cadastrado no sistema.");
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome.Trim(),
            Email = dto.Email.Trim().ToLower(),
            SenhaHash = senhaHash,
            Perfil = dto.Perfil,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        await _usuarioRepository.AddAsync(usuario);

        await _auditoriaService.RegistrarAcaoAsync(
            usuario.Email,
            "CRIAR_USUARIO",
            $"Usuário '{usuario.Email}' criado com perfil {usuario.Perfil}"
        );

        return new UsuarioResponseDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            AlunoId = usuario.Aluno?.Id,
            Matricula = usuario.Aluno?.Matricula
        };
    }

    public async Task<UsuarioResponseDTO> ObterUsuarioAtualAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new NotFoundException($"Usuário com ID '{usuarioId}' não encontrado.");
        }

        return new UsuarioResponseDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            AlunoId = usuario.Aluno?.Id,
            Matricula = usuario.Aluno?.Matricula
        };
    }
}
