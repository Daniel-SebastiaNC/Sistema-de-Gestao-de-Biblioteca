using AutoMapper;
using DTO;
using Exceptions;
using Models;
using Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Services;

public class EmprestimoService : IEmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EmprestimoService> _logger;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        IAlunoRepository alunoRepository,
        ILivroRepository livroRepository,
        IMapper mapper,
        ILogger<EmprestimoService>? logger = null)
    {
        _emprestimoRepository = emprestimoRepository;
        _alunoRepository = alunoRepository;
        _livroRepository = livroRepository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<EmprestimoService>.Instance;
    }

    public async Task<EmprestimoResponseDTO> AddEmprestimoAsync(CriarEmprestimoDTO dto)
    {
        _logger.LogInformation("Iniciando processo de empréstimo para AlunoId {IdAluno} e LivroId {IdLivro}", dto.IdAluno, dto.IdLivro);

        var aluno = await _alunoRepository.GetAlunoByIdAsync(dto.IdAluno);
        if (aluno == null)
        {
            _logger.LogWarning("Falha no empréstimo: Aluno com ID {IdAluno} não encontrado", dto.IdAluno);
            throw new NotFoundException($"Aluno com id {dto.IdAluno} não encontrado.");
        }

        var livro = await _livroRepository.GetLivroByIdAsync(dto.IdLivro);
        if (livro == null)
        {
            _logger.LogWarning("Falha no empréstimo: Livro com ID {IdLivro} não encontrado", dto.IdLivro);
            throw new NotFoundException($"Livro com id {dto.IdLivro} não encontrado.");
        }

        bool hasActiveLoan = await _emprestimoRepository.ExistsEmpresitimoAtivoAsync(dto.IdAluno, dto.IdLivro);
        if (hasActiveLoan)
        {
            _logger.LogWarning("Conflito: Aluno {IdAluno} já possui um empréstimo ativo do livro {IdLivro}", dto.IdAluno, dto.IdLivro);
            throw new ConflictException("O aluno já possui um empréstimo ativo deste mesmo livro.");
        }

        if (livro.Quantidade <= 0)
        {
            _logger.LogWarning("Estoque insuficiente para o livro '{Titulo}' (ID {IdLivro})", livro.Titulo, dto.IdLivro);
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

        _logger.LogInformation("Empréstimo ID {Id} criado com sucesso para Aluno '{AlunoNome}' e Livro '{LivroTitulo}'",
            emprestimoCriado.Id, aluno.Nome, livro.Titulo);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoCriado);
    }

    public async Task<EmprestimoResponseDTO> ReturnEmprestimoAsync(Guid id)
    {
        _logger.LogInformation("Processando devolução para Empréstimo ID {Id}", id);

        var emprestimo = await _emprestimoRepository.GetEmprestimoByIdAsync(id);
        if (emprestimo == null)
        {
            _logger.LogWarning("Falha na devolução: Empréstimo com ID {Id} não encontrado", id);
            throw new NotFoundException($"Empréstimo com id {id} não encontrado.");
        }

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
        {
            _logger.LogWarning("Falha na devolução: Empréstimo ID {Id} já estava devolvido", id);
            throw new ConflictException("Este empréstimo já foi devolvido.");
        }

        emprestimo.DataDevolucao = DateTime.Now;
        emprestimo.Status = StatusEmprestimo.Devolvido;

        if (emprestimo.Livro != null)
        {
            emprestimo.Livro.Quantidade += 1;
            await _livroRepository.UpdateLivroAsync(emprestimo.Livro);
            _logger.LogInformation("Estoque do livro '{LivroTitulo}' incrementado após devolução", emprestimo.Livro.Titulo);
        }

        var emprestimoAtualizado = await _emprestimoRepository.UpdateEmprestimoAsync(emprestimo);

        _logger.LogInformation("Devolução do Empréstimo ID {Id} concluída com sucesso", id);

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoAtualizado);
    }

    public async Task<List<EmprestimoResponseDTO>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todos os empréstimos");
        var emprestimos = await _emprestimoRepository.GetAllAsync();
        return _mapper.Map<List<EmprestimoResponseDTO>>(emprestimos);
    }

    public void ValidarDisponibilidade(int quantidade)
    {
        if (!LivroDisponivel(quantidade))
        {
            _logger.LogWarning("Validação de disponibilidade falhou: quantidade = {Quantidade}", quantidade);
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
        var multa = diasAtraso * valorPorDia;
        _logger.LogInformation("Multa calculada para {DiasAtraso} dias de atraso: R$ {Multa}", diasAtraso, multa);
        return multa;
    }
}