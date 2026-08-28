namespace Models
{
    public class Autor
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Nacionalidade { get; set; }

        public List<Livro> Livros { get; set; } = new List<Livro>();
    }
}
