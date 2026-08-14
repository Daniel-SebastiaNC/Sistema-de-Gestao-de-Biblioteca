using AutoMapper;
using DTO;
using Repository;
using Models;

namespace Services;

public class EmprestimoService : IEmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ILivroRepository _livroRepository; // Precisamos para gerenciar o estoque
    private readonly IMapper _mapper;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository, 
        ILivroRepository livroRepository, 
        IMapper mapper)
    {
        _emprestimoRepository = emprestimoRepository;
        _livroRepository = livroRepository;
        _mapper = mapper;
    }

    public EmprestimoResponseDTO AddEmprestimo(CriarEmprestimoDTO dto)
    {
        // 1. Busca o livro para validar o estoque
        var livro = _livroRepository.GetLivroById(dto.IdLivro);
        
        if (livro == null) 
            throw new Exception("Livro não encontrado.");
            
        if (livro.Quantidade <= 0) 
            throw new Exception("Livro indisponível no estoque.");

        livro.Quantidade -= 1;
        _livroRepository.UpdateLivro(livro);

        // 3. Cria o empréstimo
        var emprestimo = new Emprestimo
        {
            Id = Guid.NewGuid(),
            AlunoId = dto.IdAluno,
            LivroId = dto.IdLivro,
            DataEmprestimo = DateTime.Now,
            DataPrevistaDevolucao = DateTime.Now.AddDays(7),
            Status = StatusEmprestimo.Ativo
        };

        var emprestimoCriado = _emprestimoRepository.AddEmprestimo(emprestimo);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoCriado);
    }

    public EmprestimoResponseDTO ReturnEmprestimo(Guid id)
    {
        var emprestimo = _emprestimoRepository.GetEmprestimoById(id);
        
        if (emprestimo == null) 
            throw new Exception("Empréstimo não encontrado.");
            
        if (emprestimo.Status == StatusEmprestimo.Devolvido) 
            throw new Exception("Este empréstimo já foi devolvido.");

        emprestimo.DataDevolucao = DateTime.Now;
        emprestimo.Status = StatusEmprestimo.Devolvido;

        if (emprestimo.Livro != null)
        {
            emprestimo.Livro.Quantidade += 1;
        }

        _emprestimoRepository.UpdateEmprestimo(emprestimo);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimo);
    }

    public List<EmprestimoResponseDTO> GetAll()
    {
        return _mapper.Map<List<EmprestimoResponseDTO>>(_emprestimoRepository.GetAll());
    }
}