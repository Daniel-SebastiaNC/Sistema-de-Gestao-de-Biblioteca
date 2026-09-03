using System.ComponentModel.DataAnnotations;

namespace DTO;

public class LoginDTO
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}
