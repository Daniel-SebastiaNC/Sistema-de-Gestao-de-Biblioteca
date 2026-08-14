using AutoMapper;
using DTO;
using Repository;
using Models;
using Exceptions;

namespace Services;

public class EmprestimoService : IEmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ILivroRepository _livroRepository;
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
        var livro = _livroRepository.GetLivroById(dto.IdLivro);

        if (_emprestimoRepository.ExistsEmpresitimoAtivo(dto.IdAluno, dto.IdLivro))
        {
            throw new ConflictException("O aluno já possui um empréstimo ativo deste mesmo livro.");
        }
        
        if (livro == null) 
            throw new ConflictException("Livro não encontrado.");
            
        if (livro.Quantidade <= 0) 
            throw new ConflictException("Livro indisponível no estoque.");

        livro.Quantidade -= 1;
        _livroRepository.UpdateLivro(livro);

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
        var response = _mapper.Map<EmprestimoResponseDTO>(emprestimoCriado);

        return response;
    }

    public EmprestimoResponseDTO ReturnEmprestimo(Guid id)
    {
        var emprestimo = _emprestimoRepository.GetEmprestimoById(id);
        
        if (emprestimo == null) 
            throw new NotFoundException("Empréstimo não encontrado.");
            
        if (emprestimo.Status == StatusEmprestimo.Devolvido) 
            throw new ConflictException("Este empréstimo já foi devolvido.");

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