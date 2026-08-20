using Controllers;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;
using Xunit;

namespace BibliotecaApi.Tests.AutorTests;

public class AutorControllerTests
{
    private readonly Mock<IAutorService> _mockService;
    private readonly AutorController _controller;

    public AutorControllerTests()
    {
        _mockService = new Mock<IAutorService>();
        _controller = new AutorController(_mockService.Object);
    }

    [Fact]
    public void GetAllAutores_DeveRetornarOkResult_ComListaDeAutores()
    {
        // Arrange
        var listaAutores = new List<AutorResponseDto>
        {
            new(Guid.NewGuid(), "Autor 1", DateTime.Now, "BR"),
            new(Guid.NewGuid(), "Autor 2", DateTime.Now, "PT")
        };
        _mockService.Setup(s => s.GetAllAutores()).Returns(listaAutores);

        // Act
        var result = _controller.GetAllAutores();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<AutorResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public void GetAutorById_DeveRetornarOkResult_ComAutor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var autorDto = new AutorResponseDto(id, "Machado de Assis", DateTime.Now, "BR");
        _mockService.Setup(s => s.GetAutorById(id)).Returns(autorDto);

        // Act
        var result = _controller.GetAutorById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<AutorResponseDto>(okResult.Value);
        Assert.Equal(id, returnedDto.Id);
    }

    [Fact]
    public void AddAutor_DeveRetornarCreatedAtActionResult_ComAutorCriado()
    {
        // Arrange
        var criarDto = new CriarAutorDto("Machado de Assis", DateTime.Now, "BR");
        var id = Guid.NewGuid();
        var autorCriado = new AutorResponseDto(id, criarDto.Nome, criarDto.DataNascimento, criarDto.Nacionalidade);
        _mockService.Setup(s => s.AddAutor(criarDto)).Returns(autorCriado);

        // Act
        var result = _controller.AddAutor(criarDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(AutorController.GetAutorById), createdAtResult.ActionName);
        Assert.Equal(id, createdAtResult.RouteValues?["id"]);
        Assert.Equal(autorCriado, createdAtResult.Value);
    }

    [Fact]
    public void UpdateAutor_DeveRetornarOkResult_ComAutorAtualizado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CriarAutorDto("Nome Atualizado", DateTime.Now, "BR");
        var autorAtualizado = new AutorResponseDto(id, dto.Nome, dto.DataNascimento, dto.Nacionalidade);
        _mockService.Setup(s => s.UpdateAutor(id, dto)).Returns(autorAtualizado);

        // Act
        var result = _controller.UpadateAutor(id, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(autorAtualizado, okResult.Value);
    }

    [Fact]
    public void DeleteAutor_DeveRetornarNoContentResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAutor(id));

        // Act
        var result = _controller.DeleteAutor(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAutor(id), Times.Once);
    }
}
