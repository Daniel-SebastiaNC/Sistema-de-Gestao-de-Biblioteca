using DTO;
using Models;
using Repository;
using Exceptions;
using AutoMapper;

namespace Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _repository;
    private readonly IMapper _mapper;

    public AlunoService(IAlunoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AlunoResponseDTO> AddAlunoAsync(CriarAlunoDTO dto)
    {
        bool isExists = await _repository.ExistsAlunoByMatriculaAsync(dto.Matricula);

        if (isExists)
        {
            throw new BadRequestException($"Já existe um Aluno com Matrícula {dto.Matricula}");
        }

        Aluno aluno = await _repository.AddAlunoAsync(
            _mapper.Map<Aluno>(dto)
        );

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }

    public async Task DeleteAlunoAsync(Guid id)
    {
        Aluno aluno = await _repository.GetAlunoByIdAsync(id) ?? throw new NotFoundException($"Aluno com id {id} não encontrado.");

        await _repository.DeleteAlunoAsync(aluno);
    }

    public async Task<List<AlunoResponseDTO>> GetAllAlunosAsync()
    {
        List<Aluno> alunos = await _repository.GetAllAlunosAsync();

        return _mapper.Map<List<AlunoResponseDTO>>(alunos);
    }

    public async Task<AlunoResponseDTO> GetAlunoByIdAsync(Guid id)
    {
        Aluno aluno = await _repository.GetAlunoByIdAsync(id) ?? throw new NotFoundException($"Aluno com id {id} não encontrado.");

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }
}