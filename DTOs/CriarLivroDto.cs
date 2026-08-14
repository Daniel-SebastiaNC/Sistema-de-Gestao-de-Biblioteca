using System.Diagnostics.CodeAnalysis;

namespace DTO;
public record CriarLivroDto(
    [NotNull]
    string ISBN,

    [NotNull]
    string Titulo,

    [NotNull]
    int AnoPublicacao,

    [NotNull]
    Guid IdAutor,

    [NotNull]
    int QuantidadeDisponivel
);