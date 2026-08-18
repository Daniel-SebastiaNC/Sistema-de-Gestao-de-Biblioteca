using Models;
using DTO;
using AutoMapper;

namespace Mapper;

public class LivroProfile : Profile
{
    public LivroProfile()
    {
        CreateMap<CriarLivroDto, Livro>()
        .ForMember(dest => dest.Quantidade, opt => opt.MapFrom(src => src.QuantidadeDisponivel))
        .ForMember(dest => dest.Autor, opt => opt.Ignore());

        CreateMap<Livro, LivroResponseDTO>();
    }
}