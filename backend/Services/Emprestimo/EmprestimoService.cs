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
    private readonly IAuditoriaService? _auditoriaService;
    private readonly ICacheService? _cacheService;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        IAlunoRepository alunoRepository,
        ILivroRepository livroRepository,
        IMapper mapper,
        ILogger<EmprestimoService>? logger = null,
        IAuditoriaService? auditoriaService = null,
        ICacheService? cacheService = null)
    {
        _emprestimoRepository = emprestimoRepository;
        _alunoRepository = alunoRepository;
        _livroRepository = livroRepository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<EmprestimoService>.Instance;
        _auditoriaService = auditoriaService;
        _cacheService = cacheService;
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
        emprestimo.DataEmprestimo = DateTime.UtcNow;
        emprestimo.DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7);
        emprestimo.Status = StatusEmprestimo.Ativo;

        var emprestimoCriado = await _emprestimoRepository.AddEmprestimoAsync(emprestimo);

        _logger.LogInformation("Empréstimo ID {Id} criado com sucesso para Aluno '{AlunoNome}' e Livro '{LivroTitulo}'",
            emprestimoCriado.Id, aluno.Nome, livro.Titulo);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync("CRIACAO_EMPRESTIMO", $"Empréstimo ID {emprestimoCriado.Id} criado para aluno '{aluno.Nome}' do livro '{livro.Titulo}'");
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
            await _cacheService.RemoveAsync("relatorios:populares:5");
            await _cacheService.RemoveAsync("relatorios:populares:10");
        }

        return _mapper.Map<EmprestimoResponseDTO>(emprestimoCriado);
    }

    public async Task<EmprestimoResponseDTO> ReturnEmprestimoAsync(Guid id)
    {
        var devolucao = await DevolverComCalculoMultaAsync(new DevolverEmprestimoDTO { EmprestimoId = id });
        return devolucao.Emprestimo;
    }

    public async Task<DevolucaoResponseDTO> DevolverComCalculoMultaAsync(DevolverEmprestimoDTO dto)
    {
        _logger.LogInformation("Processando devolução para Empréstimo ID {Id}", dto.EmprestimoId);

        var emprestimo = await _emprestimoRepository.GetEmprestimoByIdAsync(dto.EmprestimoId);
        if (emprestimo == null)
        {
            _logger.LogWarning("Falha na devolução: Empréstimo com ID {Id} não encontrado", dto.EmprestimoId);
            throw new NotFoundException($"Empréstimo com id {dto.EmprestimoId} não encontrado.");
        }

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
        {
            _logger.LogWarning("Falha na devolução: Empréstimo ID {Id} já estava devolvido", dto.EmprestimoId);
            throw new ConflictException("Este empréstimo já foi devolvido.");
        }

        var dataDevolucao = DateTime.UtcNow;
        emprestimo.DataDevolucao = dataDevolucao;
        emprestimo.Status = StatusEmprestimo.Devolvido;

        // Calcular dias de atraso e multa
        int diasAtraso = 0;
        if (dataDevolucao > emprestimo.DataPrevistaDevolucao)
        {
            diasAtraso = (int)Math.Ceiling((dataDevolucao - emprestimo.DataPrevistaDevolucao).TotalDays);
        }

        decimal valorMulta = CalcularMulta(diasAtraso);

        if (emprestimo.Livro != null)
        {
            emprestimo.Livro.Quantidade += 1;
            await _livroRepository.UpdateLivroAsync(emprestimo.Livro);
            _logger.LogInformation("Estoque do livro '{LivroTitulo}' incrementado após devolução", emprestimo.Livro.Titulo);
        }

        var emprestimoAtualizado = await _emprestimoRepository.UpdateEmprestimoAsync(emprestimo);

        _logger.LogInformation("Devolução do Empréstimo ID {Id} concluída. Dias de atraso: {DiasAtraso}, Multa: R$ {Multa}",
            dto.EmprestimoId, diasAtraso, valorMulta);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                "DEVOLUCAO_EMPRESTIMO",
                $"Empréstimo ID {dto.EmprestimoId} devolvido. Dias de atraso: {diasAtraso}, Multa: R$ {valorMulta:F2}");
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
            await _cacheService.RemoveAsync("relatorios:populares:5");
            await _cacheService.RemoveAsync("relatorios:populares:10");
        }

        var responseDto = _mapper.Map<EmprestimoResponseDTO>(emprestimoAtualizado);
        string mensagem = diasAtraso > 0
            ? $"Devolução realizada com {diasAtraso} dia(s) de atraso. Multa gerada: R$ {valorMulta:F2}."
            : "Devolução realizada com sucesso dentro do prazo!";

        return new DevolucaoResponseDTO
        {
            Emprestimo = responseDto,
            DiasAtraso = diasAtraso,
            ValorMulta = valorMulta,
            Mensagem = mensagem
        };
    }

    public async Task<List<EmprestimoResponseDTO>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todos os empréstimos");
        var emprestimos = await _emprestimoRepository.GetAllAsync();
        return _mapper.Map<List<EmprestimoResponseDTO>>(emprestimos);
    }

    public async Task<PagedResultDTO<EmprestimoResponseDTO>> GetPagedAsync(PaginationParamsDTO paginationParams)
    {
        _logger.LogInformation("Buscando empréstimos paginados - Página {PageNumber}, Tamanho {PageSize}",
            paginationParams.PageNumber, paginationParams.PageSize);

        var (items, totalCount) = await _emprestimoRepository.GetPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
        var mappedItems = _mapper.Map<List<EmprestimoResponseDTO>>(items);

        return new PagedResultDTO<EmprestimoResponseDTO>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
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