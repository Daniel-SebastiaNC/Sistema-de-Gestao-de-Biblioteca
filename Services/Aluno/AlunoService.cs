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

    public AlunoResponseDTO AddAluno(CriarAlunoDTO dto)
    {
        bool isExists = _repository.ExistsAlunoByMatricula(dto.Matricula);

        if (isExists)
        {
            throw new BadRequestException($"Já existe um Aluno com Matrícula {dto.Matricula}");
        }

        Aluno aluno = _repository.AddAluno(
            _mapper.Map<Aluno>(dto)
        );

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }

    public void DeleteAluno(Guid id)
    {
        Aluno aluno = _repository.GetAlunoById(id) ?? throw new NotFoundException($"Aluno com id {id} não encontrado.");

        _repository.DeleteAluno(aluno);
    }

    public List<AlunoResponseDTO> GetAllAlunos()
    {
        List<Aluno> alunos = _repository.GetAllAlunos();

        return _mapper.Map<List<AlunoResponseDTO>>(alunos);
    }

    public AlunoResponseDTO GetAlunoById(Guid id)
    {
        Aluno aluno = _repository.GetAlunoById(id) ?? throw new NotFoundException($"Aluno com id {id} não encontrado.");

        return _mapper.Map<AlunoResponseDTO>(aluno);
    }
}