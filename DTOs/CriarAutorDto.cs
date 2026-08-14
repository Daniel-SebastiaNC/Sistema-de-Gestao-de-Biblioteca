using System.Diagnostics.CodeAnalysis;

namespace DTO;
public record CriarAutorDto(
    [NotNull]
    string Nome,

    [NotNull]  
    DateTime DataNascimento,

    [NotNull]
    string Nacionalidade
);