namespace DTO;

public record AlunoResponseDTO(
    Guid Id,
    string Nome,
    string Matricula,
    string Email,
    List<EmprestimoResumoDTO> Emprestimos
);

public record AlunoResumoDTO(
    Guid Id,
    string Nome,
    string Matricula,
    string Email
);