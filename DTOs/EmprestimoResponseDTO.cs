namespace DTO;

public record EmprestimoResponseDTO(
        Guid Id,
        AlunoResponseDTO Aluno,
        LivroResponseDTO Livro,
        DateTime DataEmprestimo,
        DateTime DataPrevistaDevolucao,
        DateTime DataDevolucao
    );