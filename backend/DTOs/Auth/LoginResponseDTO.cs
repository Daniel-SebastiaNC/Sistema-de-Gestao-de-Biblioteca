namespace DTO;

public class LoginResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Bearer";
    public DateTime ExpiraEm { get; set; }
    public UsuarioResponseDTO Usuario { get; set; } = null!;
}
