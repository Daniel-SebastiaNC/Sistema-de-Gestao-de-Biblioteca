namespace DTO;

public record LivroResponseDTO(
        Guid Id,
        string ISBN,
        string Titulo,
        int AnoPublicacao,
        AutorResponseDto Autor,
        int QuantidadeDisponivel
    );