namespace DTO;

public class DevolucaoResponseDTO
{
    public EmprestimoResponseDTO Emprestimo { get; set; } = null!;
    public int DiasAtraso { get; set; }
    public decimal ValorMulta { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
