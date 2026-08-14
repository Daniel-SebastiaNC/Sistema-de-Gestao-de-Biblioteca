using DTO;

namespace Services;

public interface IAutorService
{
    AutorResponseDto AddAutor(CriarAutorDto dto);
    AutorResponseDto GetAutorById(Guid id);
    List<AutorResponseDto> GetAllAutores();
    AutorResponseDto UpdateAutor(Guid id, CriarAutorDto dto);
    void DeleteAutor(Guid id);
}