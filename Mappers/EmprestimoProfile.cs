using Models;
using DTO;
using AutoMapper;

namespace Mapper;

public class EmprestimoProfile : Profile
{
    public EmprestimoProfile()
    {
        CreateMap<CriarEmprestimoDTO, Emprestimo>()
            .ForMember(dest => dest.AlunoId, opt => opt.MapFrom(src => src.IdAluno))
            .ForMember(dest => dest.LivroId, opt => opt.MapFrom(src => src.IdLivro))
            .ForMember(dest => dest.Aluno, opt => opt.Ignore())
            .ForMember(dest => dest.Livro, opt => opt.Ignore());

        CreateMap<Emprestimo, EmprestimoResponseDTO>();

        CreateMap<Emprestimo, EmprestimoResumoDTO>();
    }
}