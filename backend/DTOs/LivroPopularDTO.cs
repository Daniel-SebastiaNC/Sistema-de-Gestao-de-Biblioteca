namespace DTO;

public class LivroPopularDTO
{
    public Guid LivroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string AutorNome { get; set; } = string.Empty;
    public int TotalEmprestimos { get; set; }
}
