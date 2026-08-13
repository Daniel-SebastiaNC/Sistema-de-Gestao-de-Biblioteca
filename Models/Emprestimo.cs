namespace Models
{
    public class Emprestimo
    {
        public Guid Id { get; set; }
        public Guid AlunoId { get; set; }
        public Aluno Aluno { get; set; }
        public Guid LivroId { get; set; }
        public Livro Livro { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public StatusEmprestimo Status { get; set; }
    }

    public enum StatusEmprestimo
    {
        Ativo,
        Devolvido,
        Atrasado
    }
}