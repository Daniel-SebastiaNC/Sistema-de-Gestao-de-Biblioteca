namespace DTO;
public record CriarLivroDto(
        string ISBN,
        string Titulo,
        int AnoPublicacao,
        Guid IdAutor,
        int QuantidadeDisponivel
    );