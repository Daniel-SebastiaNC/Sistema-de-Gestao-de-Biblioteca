using DTO;
using Repository;
using AutoMapper;

namespace Services;
    public interface IEmprestimoService
    {
        EmprestimoResponseDTO AddEmprestimo(CriarEmprestimoDTO dto);
        EmprestimoResponseDTO ReturnEmprestimo(Guid id);

        List<EmprestimoResponseDTO> GetAll();
    }