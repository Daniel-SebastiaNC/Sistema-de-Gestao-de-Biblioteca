using Controllers;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;
using Xunit;

namespace BibliotecaApi.Tests.AlunoTests;

public class AlunoControllerTests
{
    private readonly Mock<IAlunoService> _mockService;
    private readonly AlunoController _controller;

    public AlunoControllerTests()
    {
        _mockService = new Mock<IAlunoService>();
        _controller = new AlunoController(_mockService.Object);
    }

    [Fact]
    public void GetAllAlunos_DeveRetornarOkResult_ComListaDeAlunos()
    {
        // Arrange
        var alunos = new List<AlunoResponseDTO>
        {
            new(Guid.NewGuid(), "Daniel", "123", "daniel@email.com", new List<EmprestimoResumoDTO>()),
            new(Guid.NewGuid(), "Maria", "456", "maria@email.com", new List<EmprestimoResumoDTO>())
        };

        _mockService.Setup(s => s.GetAllAlunos()).Returns(alunos);

        // Act
        var result = _controller.GetAllAlunos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<AlunoResponseDTO>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public void GetAlunoById_DeveRetornarOkResult_ComAluno()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aluno = new AlunoResponseDTO(id, "Daniel", "123", "daniel@email.com", new List<EmprestimoResumoDTO>());

        _mockService.Setup(s => s.GetAlunoById(id)).Returns(aluno);

        // Act
        var result = _controller.GetAlunoById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(aluno, okResult.Value);
    }

    [Fact]
    public void AddAluno_DeveRetornarCreatedAtActionResult_ComAlunoCriado()
    {
        // Arrange
        var dto = new CriarAlunoDTO("Daniel", "123", "daniel@email.com");
        var id = Guid.NewGuid();
        var alunoCriado = new AlunoResponseDTO(id, dto.Nome, dto.Matricula, dto.Email, new List<EmprestimoResumoDTO>());

        _mockService.Setup(s => s.AddAluno(dto)).Returns(alunoCriado);

        // Act
        var result = _controller.AddAluno(dto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(AlunoController.GetAlunoById), createdAtResult.ActionName);
        Assert.Equal(id, createdAtResult.RouteValues?["id"]);
        Assert.Equal(alunoCriado, createdAtResult.Value);
    }

    [Fact]
    public void DeleteAluno_DeveRetornarNoContentResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = _controller.DeleteAluno(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAluno(id), Times.Once);
    }
}
