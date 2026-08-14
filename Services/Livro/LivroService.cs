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
        private readonly IMapper _mapper;

        public LivroService(ILivroRepository repository, IMapper mapper)
        {
            _repositopry = repository;
            _mapper = mapper;
        }

        public LivroResponseDTO AddLivro(CriarLivroDto dto)
        {
            var livro = _repositopry.AddLivro(_mapper.Map<Livro>(dto));

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