using System.Diagnostics.CodeAnalysis;

namespace DTO;

public record CriarEmprestimoDTO(
    [NotNull]
    Guid IdAluno,

    [NotNull]
    Guid IdLivro
);