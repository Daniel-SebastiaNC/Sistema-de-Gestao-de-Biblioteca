using DTO;
using Models;
using Repository;
using Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<AlunoService> _logger;
    private readonly ICacheService? _cacheService;

    public AlunoService(
        IAlunoRepository repository,
        IMapper mapper,
        ILogger<AlunoService>? logger = null,
        ICacheService? cacheService = null)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<AlunoService>.Instance;
        _cacheService = cacheService;
    }

    public async Task<AlunoResponseDTO> AddAlunoAsync(CriarAlunoDTO dto)
    {
        _logger.LogInformation("Tentando cadastrar novo aluno com Matrícula {Matricula}", dto.Matricula);

        bool isExists = await _repository.ExistsAlunoByMatriculaAsync(dto.Matricula);

        if (isExists)
        {
            _logger.LogWarning("Falha ao cadastrar aluno: Matrícula {Matricula} já está em uso", dto.Matricula);
            throw new BadRequestException($"Já existe um Aluno com Matrícula {dto.Matricula}");
        }

        Aluno aluno = await _repository.AddAlunoAsync(
            _mapper.Map<Aluno>(dto)
        );

        _logger.LogInformation("Aluno cadastrado com sucesso com ID {Id}", aluno.Id);

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
        }

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }

    public async Task DeleteAlunoAsync(Guid id)
    {
        _logger.LogInformation("Tentando excluir aluno com ID {Id}", id);

        Aluno aluno = await _repository.GetAlunoByIdAsync(id);
        if (aluno == null)
        {
            _logger.LogWarning("Aluno com ID {Id} não foi encontrado para exclusão", id);
            throw new NotFoundException($"Aluno com id {id} não encontrado.");
        }

        await _repository.DeleteAlunoAsync(aluno);
        _logger.LogInformation("Aluno com ID {Id} excluído com sucesso", id);

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
        }
    }

    public async Task<List<AlunoResponseDTO>> GetAllAlunosAsync()
    {
        _logger.LogInformation("Buscando todos os alunos");
        List<Aluno> alunos = await _repository.GetAllAlunosAsync();
        return _mapper.Map<List<AlunoResponseDTO>>(alunos);
    }

    public async Task<PagedResultDTO<AlunoResponseDTO>> GetPagedAlunosAsync(PaginationParamsDTO paginationParams)
    {
        _logger.LogInformation("Buscando alunos paginados - Página {PageNumber}, Tamanho {PageSize}",
            paginationParams.PageNumber, paginationParams.PageSize);

        var (items, totalCount) = await _repository.GetPagedAlunosAsync(paginationParams.PageNumber, paginationParams.PageSize);
        var mappedItems = _mapper.Map<List<AlunoResponseDTO>>(items);

        return new PagedResultDTO<AlunoResponseDTO>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<AlunoResponseDTO> GetAlunoByIdAsync(Guid id)
    {
        _logger.LogInformation("Buscando aluno com ID {Id}", id);

        Aluno aluno = await _repository.GetAlunoByIdAsync(id);
        if (aluno == null)
        {
            _logger.LogWarning("Aluno com ID {Id} não encontrado", id);
            throw new NotFoundException($"Aluno com id {id} não encontrado.");
        }

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }
}