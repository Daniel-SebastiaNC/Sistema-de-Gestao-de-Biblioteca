using System.ComponentModel.DataAnnotations;

namespace DTO;

public record CriarAlunoDTO(
        string Nome,
        string Matricula,
        [EmailAddress]
        string Email
    );