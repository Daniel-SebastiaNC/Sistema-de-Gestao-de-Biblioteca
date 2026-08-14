namespace DTO;

public class LivroResponseDTO
{
    public Guid Id { get; set; }
    public string ISBN { get; set; }
    public string Titulo { get; set; }
    public int AnoPublicacao { get; set; }
    public AutorResponseDto Autor { get; set; }
    public int Quantidade { get; set; }
}