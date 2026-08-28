using AutoMapper;
using DTO;
using Models;


namespace Mapper;

public class AutorProfile : Profile
{
    public AutorProfile()
    {
        CreateMap<CriarAutorDto, Autor>()
            .ForMember(dest => dest.DataNascimento, opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DataNascimento, DateTimeKind.Utc)));

        CreateMap<Autor, AutorResponseDto>();
    }
}