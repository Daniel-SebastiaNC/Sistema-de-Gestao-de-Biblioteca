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

        public LivroResponseDTO AddLivro(CriarLivroDto dto)
        {
            var autor = _autorRepository.GetAutorById(dto.AutorId) ?? throw new NotFoundException($"Autor com Id {dto.AutorId} não encontrado");
            var livro = _mapper.Map<Livro>(dto);
            
            livro.Autor = autor;

            livro = _repositopry.AddLivro(livro);
            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public List<LivroResponseDTO> GetLivrosByAutorOrTitle(string? titulo, string? autor)
        {
            List<Livro> livros = _repositopry.GetLivrosByAutorOrTitle(titulo, autor);
            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }

        public LivroResponseDTO GetLivrosById(Guid id)
        {
            Livro livro = _repositopry.GetLivroById(id) ?? throw new NotFoundException($"Livro com id {id} não encontrado.");
            return _mapper.Map<LivroResponseDTO>(livro);
        }

        public List<LivroResponseDTO> GetAll()
        {
            List<Livro> livros = _repositopry.GetAllLivros();

            return _mapper.Map<List<LivroResponseDTO>>(livros);
        }
    }
}