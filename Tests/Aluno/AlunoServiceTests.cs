using AutoMapper;
using DTO;
using Exceptions;
using Mapper;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Moq;
using Repository;
using Services;
using Xunit;

namespace BibliotecaApi.Tests;

public class AlunoServiceTests
{
    private readonly Mock<IAlunoRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly AlunoService _service;

    public AlunoServiceTests()
    {
        _mockRepository = new Mock<IAlunoRepository>();

        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<AlunoProfile>(),
            NullLoggerFactory.Instance
        );
        _mapper = config.CreateMapper();

        _service = new AlunoService(_mockRepository.Object, _mapper);
    }

    [Fact]
    public void AddAluno_DeveRetornarAlunoResponseDTO_QuandoMatriculaNaoExiste()
    {
        // Arrange
        var dto = new CriarAlunoDTO("Daniel", "123", "daniel@email.com");

        var alunoSalvo = new Aluno
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Matricula = dto.Matricula,
            Email = dto.Email
        };

        _mockRepository.Setup(r => r.ExistsAlunoByMatricula(dto.Matricula)).Returns(false);
        _mockRepository.Setup(r => r.AddAluno(It.IsAny<Aluno>())).Returns(alunoSalvo);

        // Act
        var resultado = _service.AddAluno(dto);

        // Assert
        Assert.Equal(dto.Nome, resultado.Nome);
        Assert.Equal(dto.Matricula, resultado.Matricula);
        Assert.Equal(dto.Email, resultado.Email);
        _mockRepository.Verify(r => r.AddAluno(It.IsAny<Aluno>()), Times.Once);
    }

    [Fact]
    public void AddAluno_DeveLancarBadRequestException_QuandoMatriculaJaExiste()
    {
        // Arrange
        var dto = new CriarAlunoDTO("Daniel", "123", "daniel@email.com");

        _mockRepository.Setup(r => r.ExistsAlunoByMatricula(dto.Matricula)).Returns(true);

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _service.AddAluno(dto));

        Assert.Contains(dto.Matricula, exception.Message);
        _mockRepository.Verify(r => r.AddAluno(It.IsAny<Aluno>()), Times.Never);
    }

    [Fact]
    public void GetAlunoById_DeveRetornarAlunoResponseDTO_QuandoAlunoExiste()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var alunoFake = new Aluno
        {
            Id = alunoId,
            Nome = "Daniel",
            Matricula = "123",
            Email = "daniel@email.com"
        };

        _mockRepository.Setup(r => r.GetAlunoById(alunoId)).Returns(alunoFake);

        // Act
        var resultado = _service.GetAlunoById(alunoId);

        // Assert
        Assert.Equal(alunoId, resultado.Id);
        Assert.Equal("Daniel", resultado.Nome);
    }

    [Fact]
    public void GetAlunoById_DeveLancarNotFoundException_QuandoAlunoNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAlunoById(idInexistente)).Returns((Aluno)null);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.GetAlunoById(idInexistente));

        Assert.Contains(idInexistente.ToString(), exception.Message);
    }

    [Fact]
    public void GetAllAlunos_DeveRetornarListaDeAlunoResponseDTO_QuandoExistemAlunos()
    {
        // Arrange
        var alunos = new List<Aluno>
        {
            new Aluno { Id = Guid.NewGuid(), Nome = "Daniel", Matricula = "123", Email = "daniel@email.com" },
            new Aluno { Id = Guid.NewGuid(), Nome = "Maria", Matricula = "456", Email = "maria@email.com" }
        };

        _mockRepository.Setup(r => r.GetAllAlunos()).Returns(alunos);

        // Act
        var resultado = _service.GetAllAlunos();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Equal("Daniel", resultado[0].Nome);
        Assert.Equal("Maria", resultado[1].Nome);
    }

    [Fact]
    public void GetAllAlunos_DeveRetornarListaVazia_QuandoNaoExistemAlunos()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAlunos()).Returns(new List<Aluno>());

        // Act
        var resultado = _service.GetAllAlunos();

        // Assert
        Assert.Empty(resultado);
    }

    // ===== DeleteAluno =====

    [Fact]
    public void DeleteAluno_DeveChamarRepository_QuandoAlunoExiste()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var alunoFake = new Aluno
        {
            Id = alunoId,
            Nome = "Daniel",
            Matricula = "123",
            Email = "daniel@email.com"
        };

        _mockRepository.Setup(r => r.GetAlunoById(alunoId)).Returns(alunoFake);

        // Act
        _service.DeleteAluno(alunoId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAluno(alunoFake), Times.Once);
    }

    [Fact]
    public void DeleteAluno_DeveLancarNotFoundException_QuandoAlunoNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAlunoById(idInexistente)).Returns((Aluno)null);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.DeleteAluno(idInexistente));

        Assert.Contains(idInexistente.ToString(), exception.Message);
        _mockRepository.Verify(r => r.DeleteAluno(It.IsAny<Aluno>()), Times.Never);
    }
}