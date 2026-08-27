namespace Models;

public class Reserva
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public Guid LivroId { get; set; }
    public Livro Livro { get; set; } = null!;
    public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    public StatusReserva Status { get; set; } = StatusReserva.Ativa;
}

public enum StatusReserva
{
    Ativa,
    Atendida,
    Cancelada
}
