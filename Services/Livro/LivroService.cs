using Models;
using DTO;
using Repository;
using AutoMapper;
using Exceptions;

namespace Services
{
    public class LivroService : ILivroService
    {
        private readonly ILivroRepository _repositopry;
        private readonly IAutorRepository _autorRepository;
        private readonly IMapper _mapper;

        public LivroService(ILivroRepository repository, IAutorRepository autorRepository, IMapper mapper)
        {
            _repositopry = repository;
            _autorRepository = autorRepository;
            _mapper = mapper;
        }

        public async Task<LivroResponseDTO> AddLivroAsync(CriarLivroDto dto)
        {
            var autor = await _autorRepository.GetAutorByIdAsync(dto.AutorId) ?? throw new NotFoundException($"Autor com Id {dto.AutorId} não encontrado");
            var livro = _mapper.Map<Livro>(dto);
            
            livro.Autor = autor;

            livro = await _repositopry.AddLivroAsync(livro);
            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public async Task<List<LivroResponseDTO>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor)
        {
            List<Livro> livros = await _repositopry.GetLivrosByAutorOrTitleAsync(titulo, autor);
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }

        public async Task<LivroResponseDTO> GetLivrosByIdAsync(Guid id)
        {
            Livro livro = await _repositopry.GetLivroByIdAsync(id) ?? throw new NotFoundException($"Livro com id {id} não encontrado.");
            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public async Task<List<LivroResponseDTO>> GetAllAsync()
        {
            List<Livro> livros = await _repositopry.GetAllLivrosAsync();

            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }
    }
}