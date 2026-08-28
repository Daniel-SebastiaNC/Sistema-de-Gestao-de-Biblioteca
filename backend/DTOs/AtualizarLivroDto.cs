namespace DTO;

public class AtualizarLivroDto
{
    public string Titulo { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }
    public int Quantidade { get; set; }
    public Guid AutorId { get; set; }
}
