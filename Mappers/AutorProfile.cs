using AutoMapper;
using DTO;
using Models;


namespace Mapper;

public class AutorProfile : Profile
{
    public AutorProfile()
    {
        CreateMap<CriarAutorDto, Autor>();

        CreateMap<Autor, AutorResponseDto>();
    }
}