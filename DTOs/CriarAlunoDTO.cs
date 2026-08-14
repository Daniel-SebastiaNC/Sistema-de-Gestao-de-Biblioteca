using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DTO;

public record CriarAlunoDTO(
    [NotNull]
    string Nome,

    [NotNull]
    string Matricula,
    
    [EmailAddress]
    string Email
);