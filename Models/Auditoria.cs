namespace Models;

public class Auditoria
{
    public Guid Id { get; set; }
    public string Usuario { get; set; } = "Sistema";
    public string Acao { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
