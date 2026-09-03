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
        private readonly IAuditoriaService? _auditoriaService;
        private readonly ICacheService? _cacheService;

        public LivroService(
            ILivroRepository repository,
            IAutorRepository autorRepository,
            IMapper mapper,
            ILogger<LivroService>? logger = null,
            IAuditoriaService? auditoriaService = null,
            ICacheService? cacheService = null)
        {
            _repositopry = repository;
            _autorRepository = autorRepository;
            _mapper = mapper;
            _logger = logger ?? NullLogger<LivroService>.Instance;
            _auditoriaService = auditoriaService;
            _cacheService = cacheService;
        }

        public async Task<LivroResponseDTO> AddLivroAsync(CriarLivroDto dto)
        {
            _logger.LogInformation("Tentando cadastrar livro '{Titulo}' para AutorId {AutorId}", dto.Titulo, dto.AutorId);

            if (await _repositopry.ExistsByIsbnAsync(dto.ISBN))
            {
                _logger.LogWarning("Falha ao cadastrar livro: ISBN '{ISBN}' já está cadastrado", dto.ISBN);
                throw new ConflictException($"Já existe um livro cadastrado com o ISBN '{dto.ISBN}'.");
            }

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

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarAcaoAsync("CRIACAO_LIVRO", $"Livro '{livro.Titulo}' cadastrado (ID: {livro.Id})");
            }

            var response = _mapper.Map<LivroResponseDTO>(livro);

            if (_cacheService != null)
            {
                await _cacheService.SetAsync($"livro:{livro.Id}", response, TimeSpan.FromMinutes(10));
                await _cacheService.RemoveAsync("dashboard:stats");
                await _cacheService.RemoveAsync("relatorios:populares:5");
                await _cacheService.RemoveAsync("relatorios:populares:10");
            }

            return response;
        }

        public async Task<List<LivroResponseDTO>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor)
        {
            _logger.LogInformation("Buscando livros com filtros - Titulo: '{Titulo}', Autor: '{Autor}'", titulo, autor);
            List<Livro> livros = await _repositopry.GetLivrosByAutorOrTitleAsync(titulo, autor);
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }

        public async Task<PagedResultDTO<LivroResponseDTO>> GetPagedLivrosAsync(string? titulo, string? autor, PaginationParamsDTO paginationParams)
        {
            return await GetPagedLivrosAsync(null, titulo, autor, paginationParams);
        }

        public async Task<PagedResultDTO<LivroResponseDTO>> GetPagedLivrosAsync(string? termo, string? titulo, string? autor, PaginationParamsDTO paginationParams)
        {
            _logger.LogInformation("Buscando livros paginados - Termo: '{Termo}', Titulo: '{Titulo}', Autor: '{Autor}', Página {PageNumber}, Tamanho {PageSize}",
                termo, titulo, autor, paginationParams.PageNumber, paginationParams.PageSize);

            var (items, totalCount) = await _repositopry.GetPagedLivrosAsync(termo, titulo, autor, paginationParams.PageNumber, paginationParams.PageSize);
            var mappedItems = _mapper.Map<List<LivroResponseDTO>>(items);
            return new PagedResultDTO<LivroResponseDTO>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<LivroResponseDTO> GetLivrosByIdAsync(Guid id)
        {
            var cacheKey = $"livro:{id}";

            if (_cacheService != null)
            {
                var cachedLivro = await _cacheService.GetAsync<LivroResponseDTO>(cacheKey);
                if (cachedLivro != null)
                {
                    return cachedLivro;
                }
            }

            _logger.LogInformation("Buscando livro com ID {Id} no banco de dados", id);
            Livro livro = await _repositopry.GetLivroByIdAsync(id);
            if (livro == null)
            {
                _logger.LogWarning("Livro com ID {Id} não encontrado", id);
                throw new NotFoundException($"Livro com id {id} não encontrado.");
            }

            var response = _mapper.Map<LivroResponseDTO>(livro);

            if (_cacheService != null)
            {
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            }

            return response;
        }

        public async Task<LivroResponseDTO> UpdateLivroAsync(Guid id, AtualizarLivroDto dto)
        {
            _logger.LogInformation("Atualizando livro com ID {Id}", id);

            var livro = await _repositopry.GetLivroByIdAsync(id);
            if (livro == null)
            {
                _logger.LogWarning("Livro com ID {Id} não encontrado para atualização", id);
                throw new NotFoundException($"Livro com id {id} não encontrado.");
            }

            if (await _repositopry.ExistsByIsbnAsync(dto.ISBN, id))
            {
                _logger.LogWarning("Falha ao atualizar livro: ISBN '{ISBN}' já está sendo utilizado por outro livro", dto.ISBN);
                throw new ConflictException($"Já existe outro livro cadastrado com o ISBN '{dto.ISBN}'.");
            }

            var autor = await _autorRepository.GetAutorByIdAsync(dto.AutorId);
            if (autor == null)
            {
                _logger.LogWarning("Falha ao atualizar livro: Autor com Id {AutorId} não encontrado", dto.AutorId);
                throw new NotFoundException($"Autor com Id {dto.AutorId} não encontrado");
            }

            livro.Titulo = dto.Titulo;
            livro.ISBN = dto.ISBN;
            livro.AnoPublicacao = dto.AnoPublicacao;
            livro.Quantidade = dto.Quantidade;
            livro.AutorId = dto.AutorId;
            livro.Autor = autor;

            await _repositopry.UpdateLivroAsync(livro);
            _logger.LogInformation("Livro ID {Id} atualizado com sucesso", id);

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarAcaoAsync("ATUALIZACAO_LIVRO", $"Livro '{livro.Titulo}' atualizado (ID: {livro.Id})");
            }

            var response = _mapper.Map<LivroResponseDTO>(livro);

            if (_cacheService != null)
            {
                await _cacheService.SetAsync($"livro:{id}", response, TimeSpan.FromMinutes(10));
                await _cacheService.RemoveAsync("dashboard:stats");
            }

            return response;
        }

        public async Task DeleteLivroAsync(Guid id)
        {
            _logger.LogInformation("Tentando excluir livro com ID {Id}", id);

            var livro = await _repositopry.GetLivroByIdAsync(id);
            if (livro == null)
            {
                _logger.LogWarning("Livro com ID {Id} não encontrado para exclusão", id);
                throw new NotFoundException($"Livro com id {id} não encontrado.");
            }

            bool hasActiveLoans = await _repositopry.HasActiveLoansAsync(id);
            if (hasActiveLoans)
            {
                _logger.LogWarning("Falha ao excluir: Livro com ID {Id} possui empréstimos ativos pendentes", id);
                throw new ConflictException("Não é possível excluir um livro com empréstimos ativos em andamento.");
            }

            await _repositopry.DeleteLivroAsync(livro);
            _logger.LogInformation("Livro com ID {Id} excluído com sucesso", id);

            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync($"livro:{id}");
                await _cacheService.RemoveAsync("dashboard:stats");
            }

            if (_auditoriaService != null)
            {
                await _auditoriaService.RegistrarAcaoAsync("EXCLUSAO_LIVRO", $"Livro '{livro.Titulo}' excluído (ID: {id})");
            }
        }

        public async Task<List<LivroResponseDTO>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os livros");
            List<Livro> livros = await _repositopry.GetAllLivrosAsync();
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }
    }
}