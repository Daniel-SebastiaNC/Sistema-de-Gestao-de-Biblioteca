namespace DTO;

public record AlunoResponseDTO(
    Guid Id,
    string Nome,
    string Matricula,
    string Email,
    List<EmprestimoResponseDTO> Emprestimos
);