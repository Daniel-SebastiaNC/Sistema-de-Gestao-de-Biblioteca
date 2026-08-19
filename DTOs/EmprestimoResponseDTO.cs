using Models;

namespace DTO;

public record EmprestimoResponseDTO(
    Guid Id,
    LivroResponseDTO Livro,
    AlunoResponseDTO Aluno,
    DateTime DataEmprestimo,
    DateTime DataPrevistaDevolucao,
    DateTime DataDevolucao,
    StatusEmprestimo Status
);

public record EmprestimoResumoDTO(
    Guid Id,
    LivroResponseDTO Livro,
    DateTime DataEmprestimo,
    DateTime DataPrevistaDevolucao,
    DateTime? DataDevolucao,
    StatusEmprestimo Status
);