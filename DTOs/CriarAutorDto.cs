namespace DTO;
public record CriarAutorDto(
        string Nome,  
        DateTime DataNascimento,
        string Nacionalidade
    );