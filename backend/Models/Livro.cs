namespace Models
{
    public class Livro
    {
        public Guid Id { get; set; }
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public int AnoPublicacao { get; set; }
        public int Quantidade { get; set; }
        public Guid AutorId { get; set; }
        public Autor Autor { get; set; }

    }
}