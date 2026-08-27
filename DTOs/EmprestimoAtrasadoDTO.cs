namespace DTO;

public class EmprestimoAtrasadoDTO
{
    public Guid EmprestimoId { get; set; }
    public Guid AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string AlunoMatricula { get; set; } = string.Empty;
    public Guid LivroId { get; set; }
    public string LivroTitulo { get; set; } = string.Empty;
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public int DiasAtraso { get; set; }
    public decimal MultaEstimada { get; set; }
}
