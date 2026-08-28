namespace DTO;

public class AuditoriaResponseDTO
{
    public Guid Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
}
