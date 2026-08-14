using AutoMapper;
using DTO;
using Exceptions;
using Repository;
using Models;

namespace Services;

public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;
    private readonly IMapper _mapper;

    public AutorService(IAutorRepository autorRepository, IMapper mapper)
    {
        _autorRepository = autorRepository;
        _mapper = mapper;
    }

    public AutorResponseDto AddAutor(CriarAutorDto dto)
    {
        var autor = _mapper.Map<Autor>(dto);
        var autorSalvo = _autorRepository.AddAutor(autor);
        return _mapper.Map<AutorResponseDto>(autorSalvo);
    }

    public AutorResponseDto GetAutorById(Guid id)
    {
        var autor = _autorRepository.GetAutorById(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        return _mapper.Map<AutorResponseDto>(autor);
    }

    public List<AutorResponseDto> GetAllAutores()
    {
        var autores = _autorRepository.GetAllAutores();
        return _mapper.Map<List<AutorResponseDto>>(autores);
    }

    public AutorResponseDto UpdateAutor(Guid id, CriarAutorDto dto)
    {
        var autor = _autorRepository.GetAutorById(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        autor.Nome = dto.Nome;
        autor.DataNascimento = dto.DataNascimento;
        autor.Nacionalidade = dto.Nacionalidade;

        _autorRepository.UpdateAutor(autor);
        return _mapper.Map<AutorResponseDto>(autor);
    }

    public void DeleteAutor(Guid id)
    {
        var autor = _autorRepository.GetAutorById(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        _autorRepository.DeleteAutor(autor);
    }
}