namespace DTO;

public record AutorResponseDto(
        Guid Id,
        string Nome,
        DateTime DataNascimento,
        string Nacionalidade
    );