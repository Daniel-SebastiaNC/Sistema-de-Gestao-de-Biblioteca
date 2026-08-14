using AutoMapper;
using DTO;
using Models;

namespace Mapper;

public class AlunoProfile : Profile
{
    public AlunoProfile()
    {
        CreateMap<CriarAlunoDTO, Aluno>();

        CreateMap<Aluno, AlunoResponseDTO>();
    }
}