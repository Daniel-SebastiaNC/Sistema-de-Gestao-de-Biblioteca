using System.ComponentModel.DataAnnotations;
using Models;

namespace DTO;

public class CriarUsuarioDTO
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil é obrigatório")]
    public PerfilUsuario Perfil { get; set; }

    public Guid? AlunoId { get; set; }
}
