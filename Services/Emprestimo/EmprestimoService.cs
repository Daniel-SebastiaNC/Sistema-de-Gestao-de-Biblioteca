using AutoMapper;
using DTO;
using Exceptions;
using Models;
using Repository;

namespace Services;

public class EmprestimoService : IEmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IMapper _mapper;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        IAlunoRepository alunoRepository,
        ILivroRepository livroRepository,
        IMapper mapper)
    {
        _emprestimoRepository = emprestimoRepository;
        _alunoRepository = alunoRepository;
        _livroRepository = livroRepository;
        _mapper = mapper;
    }

    public async Task<EmprestimoResponseDTO> AddEmprestimoAsync(CriarEmprestimoDTO dto)
    {
        var aluno = await _alunoRepository.GetAlunoByIdAsync(dto.IdAluno)
            ?? throw new NotFoundException($"Aluno com id {dto.IdAluno} não encontrado.");

        var livro = await _livroRepository.GetLivroByIdAsync(dto.IdLivro)
            ?? throw new NotFoundException($"Livro com id {dto.IdLivro} não encontrado.");

        bool hasActiveLoan = await _emprestimoRepository.ExistsEmpresitimoAtivoAsync(dto.IdAluno, dto.IdLivro);
        if (hasActiveLoan)
        {
            throw new ConflictException("O aluno já possui um empréstimo ativo deste mesmo livro.");
        }

        if (livro.Quantidade <= 0)
        {
            throw new ConflictException("Livro indisponível no estoque.");
        }

        livro.Quantidade -= 1;
        await _livroRepository.UpdateLivroAsync(livro);

        var emprestimo = _mapper.Map<Emprestimo>(dto);
        emprestimo.Aluno = aluno;
        emprestimo.Livro = livro;
        emprestimo.DataEmprestimo = DateTime.Now;
        emprestimo.DataPrevistaDevolucao = DateTime.Now.AddDays(7);
        emprestimo.Status = StatusEmprestimo.Ativo;

        var emprestimoCriado = await _emprestimoRepository.AddEmprestimoAsync(emprestimo);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoCriado);
    }

    public async Task<EmprestimoResponseDTO> ReturnEmprestimoAsync(Guid id)
    {
        var emprestimo = await _emprestimoRepository.GetEmprestimoByIdAsync(id)
            ?? throw new NotFoundException($"Empréstimo com id {id} não encontrado.");

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
        {
            throw new ConflictException("Este empréstimo já foi devolvido.");
        }

        emprestimo.DataDevolucao = DateTime.Now;
        emprestimo.Status = StatusEmprestimo.Devolvido;

        if (emprestimo.Livro != null)
        {
            emprestimo.Livro.Quantidade += 1;
            await _livroRepository.UpdateLivroAsync(emprestimo.Livro);
        }

        var emprestimoAtualizado = await _emprestimoRepository.UpdateEmprestimoAsync(emprestimo);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoAtualizado);
    }

    public async Task<List<EmprestimoResponseDTO>> GetAllAsync()
    {
        var emprestimos = await _emprestimoRepository.GetAllAsync();
        return _mapper.Map<List<EmprestimoResponseDTO>>(emprestimos);
    }

    public void ValidarDisponibilidade(int quantidade)
    {
        if (!LivroDisponivel(quantidade))
        {
            throw new RegraNegocioException("Livro indisponível no estoque.");
        }
    }

    public bool LivroDisponivel(int quantidade)
    {
        return quantidade > 0;
    }

    public decimal CalcularMulta(int diasAtraso)
    {
        const decimal valorPorDia = 2.00m; //precisamos mudar pra 2 reais o dia (antigamente era 3)
        if (diasAtraso <= 0)
        {
            return 0;
        }
        return diasAtraso * valorPorDia;
    }

}