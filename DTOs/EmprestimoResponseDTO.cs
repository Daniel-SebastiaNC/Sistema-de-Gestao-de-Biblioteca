namespace DTO;

public record EmprestimoResponseDTO(
        Guid Id,
        DateTime DataEmprestimo,
        DateTime DataPrevistaDevolucao,
        DateTime DataDevolucao
    );