using System.Security.Claims;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.AutenticarAsync(dto);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponseDTO>> ObterUsuarioLogado()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var usuarioId))
        {
            return Unauthorized(new { message = "Identificador do usuário não encontrado no token." });
        }

        var usuario = await _authService.ObterUsuarioAtualAsync(usuarioId);
        return Ok(usuario);
    }

    [HttpPost("usuarios")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UsuarioResponseDTO>> CriarUsuario([FromBody] CriarUsuarioDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _authService.RegistrarAsync(dto);
        return Created($"/api/auth/usuarios/{usuario.Id}", usuario);
    }
}
