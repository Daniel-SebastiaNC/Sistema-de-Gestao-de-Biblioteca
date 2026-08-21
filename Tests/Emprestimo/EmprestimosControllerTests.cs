using Controllers;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;
using Services;
using Xunit;

namespace BibliotecaApi.Tests.EmprestimoTests;

public class EmprestimosControllerTests
{
    private readonly Mock<IEmprestimoService> _mockService;
    private readonly EmprestimosController _controller;

    public EmprestimosControllerTests()
    {
        _mockService = new Mock<IEmprestimoService>();
        _controller = new EmprestimosController(_mockService.Object);
    }

    [Fact]
    public void AddEmprestimo_DeveRetornarCreatedResult_QuandoModeloValido()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var livroId = Guid.NewGuid();
        var dto = new CriarEmprestimoDTO(alunoId, livroId);
        var emprestimoId = Guid.NewGuid();

        var responseDto = new EmprestimoResponseDTO(
            emprestimoId,
            new LivroResponseDTO { Id = livroId, Titulo = "Livro 1" },
            new AlunoResumoDTO(alunoId, "Aluno 1", "123", "aluno@email.com"),
            DateTime.Now,
            DateTime.Now.AddDays(7),
            null,
            StatusEmprestimo.Ativo
        );

        _mockService.Setup(s => s.AddEmprestimo(dto)).Returns(responseDto);

        // Act
        var result = _controller.AddEmprestimo(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/api/emprestimos/{emprestimoId}", createdResult.Location);
        Assert.Equal(responseDto, createdResult.Value);
    }

    [Fact]
    public void AddEmprestimo_DeveRetornarBadRequest_QuandoModelStateInvalido()
    {
        // Arrange
        var dto = new CriarEmprestimoDTO(Guid.Empty, Guid.Empty);
        _controller.ModelState.AddModelError("IdAluno", "Campo obrigatório");

        // Act
        var result = _controller.AddEmprestimo(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockService.Verify(s => s.AddEmprestimo(It.IsAny<CriarEmprestimoDTO>()), Times.Never);
    }

    [Fact]
    public void ReturnEmprestimo_DeveRetornarOkResult_ComEmprestimoDevolvido()
    {
        // Arrange
        var id = Guid.NewGuid();
        var responseDto = new EmprestimoResponseDTO(
            id,
            new LivroResponseDTO { Id = Guid.NewGuid(), Titulo = "Livro 1" },
            new AlunoResumoDTO(Guid.NewGuid(), "Aluno 1", "123", "aluno@email.com"),
            DateTime.Now.AddDays(-7),
            DateTime.Now,
            DateTime.Now,
            StatusEmprestimo.Devolvido
        );

        _mockService.Setup(s => s.ReturnEmprestimo(id)).Returns(responseDto);

        // Act
        var result = _controller.ReturnEmprestimo(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responseDto, okResult.Value);
    }

    [Fact]
    public void GetAll_DeveRetornarOkResult_ComListaDeEmprestimos()
    {
        // Arrange
        var lista = new List<EmprestimoResponseDTO>
        {
            new(Guid.NewGuid(), null!, null!, DateTime.Now, DateTime.Now.AddDays(7), null, StatusEmprestimo.Ativo)
        };

        _mockService.Setup(s => s.GetAll()).Returns(lista);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(lista, okResult.Value);
    }
}
