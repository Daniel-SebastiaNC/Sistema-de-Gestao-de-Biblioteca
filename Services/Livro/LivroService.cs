using Models;
using DTO;
using Repository;
using AutoMapper;
using Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Services
{
    public class LivroService : ILivroService
    {
        private readonly ILivroRepository _repositopry;
        private readonly IAutorRepository _autorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LivroService> _logger;

        public LivroService(
            ILivroRepository repository,
            IAutorRepository autorRepository,
            IMapper mapper,
            ILogger<LivroService>? logger = null)
        {
            _repositopry = repository;
            _autorRepository = autorRepository;
            _mapper = mapper;
            _logger = logger ?? NullLogger<LivroService>.Instance;
        }

        public async Task<LivroResponseDTO> AddLivroAsync(CriarLivroDto dto)
        {
            _logger.LogInformation("Tentando cadastrar livro '{Titulo}' para AutorId {AutorId}", dto.Titulo, dto.AutorId);

            var autor = await _autorRepository.GetAutorByIdAsync(dto.AutorId);
            if (autor == null)
            {
                _logger.LogWarning("Falha ao cadastrar livro: Autor com Id {AutorId} não encontrado", dto.AutorId);
                throw new NotFoundException($"Autor com Id {dto.AutorId} não encontrado");
            }

            var livro = _mapper.Map<Livro>(dto);
            livro.Autor = autor;

            livro = await _repositopry.AddLivroAsync(livro);
            _logger.LogInformation("Livro '{Titulo}' cadastrado com sucesso com ID {Id}", livro.Titulo, livro.Id);

            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public async Task<List<LivroResponseDTO>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor)
        {
            _logger.LogInformation("Buscando livros com filtros - Titulo: '{Titulo}', Autor: '{Autor}'", titulo, autor);
            List<Livro> livros = await _repositopry.GetLivrosByAutorOrTitleAsync(titulo, autor);
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }

        public async Task<PagedResultDTO<LivroResponseDTO>> GetPagedLivrosAsync(string? titulo, string? autor, PaginationParamsDTO paginationParams)
        {
            _logger.LogInformation("Buscando livros paginados - Titulo: '{Titulo}', Autor: '{Autor}', Página {PageNumber}, Tamanho {PageSize}",
                titulo, autor, paginationParams.PageNumber, paginationParams.PageSize);

            var (items, totalCount) = await _repositopry.GetPagedLivrosByAutorOrTitleAsync(titulo, autor, paginationParams.PageNumber, paginationParams.PageSize);
            var mappedItems = _mapper.Map<List<LivroResponseDTO>>(items);

            return new PagedResultDTO<LivroResponseDTO>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<LivroResponseDTO> GetLivrosByIdAsync(Guid id)
        {
            _logger.LogInformation("Buscando livro com ID {Id}", id);
            Livro livro = await _repositopry.GetLivroByIdAsync(id);
            if (livro == null)
            {
                _logger.LogWarning("Livro com ID {Id} não encontrado", id);
                throw new NotFoundException($"Livro com id {id} não encontrado.");
            }

            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public async Task<List<LivroResponseDTO>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os livros");
            List<Livro> livros = await _repositopry.GetAllLivrosAsync();
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }
    }
}