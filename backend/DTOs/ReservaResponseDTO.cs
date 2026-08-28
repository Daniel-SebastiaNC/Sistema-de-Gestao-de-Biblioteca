using Models;

namespace DTO;

public class ReservaResponseDTO
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public Guid LivroId { get; set; }
    public string LivroTitulo { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public int PosicaoFila { get; set; }
    public StatusReserva Status { get; set; }
}
