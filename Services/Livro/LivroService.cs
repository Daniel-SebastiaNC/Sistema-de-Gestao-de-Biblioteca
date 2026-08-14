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
            var autor = _autorRepository.GetAutorById(dto.IdAutor) ?? throw new NotFoundException($"Autor com Id {dto.IdAutor} não encontrado");
            var livro = new Livro();
            livro.AnoPublicacao = dto.AnoPublicacao;
            livro.ISBN = dto.ISBN;
            livro.Quantidade = dto.QuantidadeDisponivel;
            livro.Titulo = dto.Titulo;
            livro.Autor = autor;
            livro.AutorId = dto.IdAutor;
            livro = _repositopry.AddLivro(livro);

            var response = new LivroResponseDTO();

            response.Id = livro.Id;
            response.ISBN = livro.ISBN;
            response.Titulo = livro.Titulo;
            response.AnoPublicacao = livro.AnoPublicacao;

            var responseAutor = new AutorResponseDto(
                autor.Id,
                autor.Nome,
                autor.DataNascimento,
                autor.Nacionalidade
            );

            response.Autor =  responseAutor;
            response.Quantidade = livro.Quantidade;
            
            return response;
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