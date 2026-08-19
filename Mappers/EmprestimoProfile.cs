using Models;
using DTO;
using AutoMapper;

namespace Mapper;

public class EmprestimoProfile : Profile
{
    public EmprestimoProfile()
    {
        CreateMap<CriarEmprestimoDTO, Emprestimo>();

        CreateMap<Emprestimo, EmprestimoResponseDTO>();

        CreateMap<Emprestimo, EmprestimoResumoDTO>();
    }
}