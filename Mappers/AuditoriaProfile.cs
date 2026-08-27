using AutoMapper;
using DTO;
using Models;

namespace Mapper;

public class AuditoriaProfile : Profile
{
    public AuditoriaProfile()
    {
        CreateMap<Auditoria, AuditoriaResponseDTO>();
    }
}
