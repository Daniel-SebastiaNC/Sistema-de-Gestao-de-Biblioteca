using AutoMapper;
using DTO;
using Exceptions;
using Repository;
using Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Services;

public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AutorService> _logger;

    public AutorService(
        IAutorRepository autorRepository,
        IMapper mapper,
        ILogger<AutorService>? logger = null)
    {
        _autorRepository = autorRepository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<AutorService>.Instance;
    }

    public async Task<AutorResponseDto> AddAutorAsync(CriarAutorDto dto)
    {
        _logger.LogInformation("Cadastrando novo autor: {Nome}", dto.Nome);

        var autor = _mapper.Map<Autor>(dto);
        var autorSalvo = await _autorRepository.AddAutorAsync(autor);

        _logger.LogInformation("Autor {Nome} cadastrado com sucesso com ID {Id}", autorSalvo.Nome, autorSalvo.Id);

        return _mapper.Map<AutorResponseDto>(autorSalvo);
    }

    public async Task<AutorResponseDto> GetAutorByIdAsync(Guid id)
    {
        _logger.LogInformation("Buscando autor com ID {Id}", id);

        var autor = await _autorRepository.GetAutorByIdAsync(id);
        if (autor == null)
        {
            _logger.LogWarning("Autor com ID {Id} não encontrado", id);
            throw new NotFoundException($"Autor com id {id} não encontrado.");
        }

        return _mapper.Map<AutorResponseDto>(autor);
    }

    public async Task<List<AutorResponseDto>> GetAllAutoresAsync()
    {
        _logger.LogInformation("Buscando todos os autores");
        var autores = await _autorRepository.GetAllAutoresAsync();
        return _mapper.Map<List<AutorResponseDto>>(autores);
    }

    public async Task<PagedResultDTO<AutorResponseDto>> GetPagedAutoresAsync(PaginationParamsDTO paginationParams)
    {
        _logger.LogInformation("Buscando autores paginados - Página {PageNumber}, Tamanho {PageSize}",
            paginationParams.PageNumber, paginationParams.PageSize);

        var (items, totalCount) = await _autorRepository.GetPagedAutoresAsync(paginationParams.PageNumber, paginationParams.PageSize);
        var mappedItems = _mapper.Map<List<AutorResponseDto>>(items);

        return new PagedResultDTO<AutorResponseDto>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<AutorResponseDto> UpdateAutorAsync(Guid id, CriarAutorDto dto)
    {
        _logger.LogInformation("Atualizando autor com ID {Id}", id);

        var autor = await _autorRepository.GetAutorByIdAsync(id);
        if (autor == null)
        {
            _logger.LogWarning("Falha ao atualizar: Autor com ID {Id} não encontrado", id);
            throw new NotFoundException($"Autor com id {id} não encontrado.");
        }

        autor.Nome = dto.Nome;
        autor.DataNascimento = dto.DataNascimento;
        autor.Nacionalidade = dto.Nacionalidade;

        await _autorRepository.UpdateAutorAsync(autor);

        _logger.LogInformation("Autor com ID {Id} atualizado com sucesso", id);

        return _mapper.Map<AutorResponseDto>(autor);
    }

    public async Task DeleteAutorAsync(Guid id)
    {
        _logger.LogInformation("Tentando excluir autor com ID {Id}", id);

        var autor = await _autorRepository.GetAutorByIdAsync(id);
        if (autor == null)
        {
            _logger.LogWarning("Falha ao excluir: Autor com ID {Id} não encontrado", id);
            throw new NotFoundException($"Autor com id {id} não encontrado.");
        }

        await _autorRepository.DeleteAutorAsync(autor);
        _logger.LogInformation("Autor com ID {Id} excluído com sucesso", id);
    }
}