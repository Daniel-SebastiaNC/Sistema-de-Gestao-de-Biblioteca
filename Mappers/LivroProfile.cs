using Models;
using DTO;
using AutoMapper;

namespace Mapper;

public class LivroProfile : Profile
{
    public LivroProfile()
    {
        CreateMap<CriarLivroDto, Livro>();

        CreateMap<Livro, LivroResponseDTO>();
    }
}