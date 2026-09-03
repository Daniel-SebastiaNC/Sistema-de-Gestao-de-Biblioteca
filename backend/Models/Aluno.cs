namespace Models
{
    public class Aluno
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }

        public Guid? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    }
}