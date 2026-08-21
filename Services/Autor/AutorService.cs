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

    public async Task<AutorResponseDto> AddAutorAsync(CriarAutorDto dto)
    {
        var autor = _mapper.Map<Autor>(dto);
        var autorSalvo = await _autorRepository.AddAutorAsync(autor);
        return _mapper.Map<AutorResponseDto>(autorSalvo);
    }

    public async Task<AutorResponseDto> GetAutorByIdAsync(Guid id)
    {
        var autor = await _autorRepository.GetAutorByIdAsync(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        return _mapper.Map<AutorResponseDto>(autor);
    }

    public async Task<List<AutorResponseDto>> GetAllAutoresAsync()
    {
        var autores = await _autorRepository.GetAllAutoresAsync();
        return _mapper.Map<List<AutorResponseDto>>(autores);
    }

    public async Task<AutorResponseDto> UpdateAutorAsync(Guid id, CriarAutorDto dto)
    {
        var autor = await _autorRepository.GetAutorByIdAsync(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        autor.Nome = dto.Nome;
        autor.DataNascimento = dto.DataNascimento;
        autor.Nacionalidade = dto.Nacionalidade;

        await _autorRepository.UpdateAutorAsync(autor);
        return _mapper.Map<AutorResponseDto>(autor);
    }

    public async Task DeleteAutorAsync(Guid id)
    {
        var autor = await _autorRepository.GetAutorByIdAsync(id)
            ?? throw new NotFoundException($"Autor com id {id} não encontrado.");

        await _autorRepository.DeleteAutorAsync(autor);
    }
}