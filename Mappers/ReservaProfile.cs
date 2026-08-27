using AutoMapper;
using DTO;
using Models;

namespace Mapper;

public class ReservaProfile : Profile
{
    public ReservaProfile()
    {
        CreateMap<CriarReservaDTO, Reserva>();
        CreateMap<Reserva, ReservaResponseDTO>()
            .ForMember(dest => dest.AlunoNome, opt => opt.MapFrom(src => src.Aluno != null ? src.Aluno.Nome : string.Empty))
            .ForMember(dest => dest.LivroTitulo, opt => opt.MapFrom(src => src.Livro != null ? src.Livro.Titulo : string.Empty));
    }
}
