namespace DTO;

public class HistoricoTransacaoDTO
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Emprestimo", "Devolucao", "Reserva"
    public string AlunoNome { get; set; } = string.Empty;
    public string LivroTitulo { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }
    public string Status { get; set; } = string.Empty;
}
