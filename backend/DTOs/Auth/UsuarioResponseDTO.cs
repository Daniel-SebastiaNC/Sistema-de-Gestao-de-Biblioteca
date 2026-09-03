namespace DTO;

public class UsuarioResponseDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public Guid? AlunoId { get; set; }
    public string? Matricula { get; set; }
}
