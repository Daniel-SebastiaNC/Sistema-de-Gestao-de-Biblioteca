using Controllers;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;
using Xunit;

namespace BibliotecaApi.Tests.LivroTests;

public class LivrosControllerTests
{
    private readonly Mock<ILivroService> _mockService;
    private readonly LivrosController _controller;

    public LivrosControllerTests()
    {
        _mockService = new Mock<ILivroService>();
        _controller = new LivrosController(_mockService.Object);
    }

    [Fact]
    public void CriarLivro_DeveRetornarCreatedAtActionResult_ComLivroCriado()
    {
        // Arrange
        var autorId = Guid.NewGuid();
        var dto = new CriarLivroDto("12345", "Dom Casmurro", 1899, autorId, 3);
        var livroId = Guid.NewGuid();
        var livroResponse = new LivroResponseDTO
        {
            Id = livroId,
            ISBN = dto.ISBN,
            Titulo = dto.Titulo,
            AnoPublicacao = dto.AnoPublicacao,
            Quantidade = dto.QuantidadeDisponivel,
            Autor = new AutorResponseDto(autorId, "Machado de Assis", DateTime.Now, "BR")
        };

        _mockService.Setup(s => s.AddLivro(dto)).Returns(livroResponse);

        // Act
        var result = _controller.CriarLivro(dto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(LivrosController.GetLivroById), createdAtResult.ActionName);
        Assert.Equal(livroId, createdAtResult.RouteValues?["id"]);
        Assert.Equal(livroResponse, createdAtResult.Value);
    }

    [Fact]
    public void ObterLivros_DeveRetornarOkObjectResult_ComListaDeLivrosFiltrados()
    {
        // Arrange
        var listaLivros = new List<LivroResponseDTO>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Dom Casmurro", ISBN = "123" }
        };

        _mockService.Setup(s => s.GetLivrosByAutorOrTitle("Dom", null)).Returns(listaLivros);

        // Act
        var result = _controller.ObterLivros("Dom", null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<List<LivroResponseDTO>>(okResult.Value);
        Assert.Single(returnedList);
    }

    [Fact]
    public void GetLivroById_DeveRetornarOkObjectResult_ComLivro()
    {
        // Arrange
        var id = Guid.NewGuid();
        var livroResponse = new LivroResponseDTO { Id = id, Titulo = "Dom Casmurro", ISBN = "123" };

        _mockService.Setup(s => s.GetLivrosById(id)).Returns(livroResponse);

        // Act
        var result = _controller.GetLivroById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(livroResponse, okResult.Value);
    }

    [Fact]
    public void GetAll_DeveRetornarOkObjectResult_ComTodosOsLivros()
    {
        // Arrange
        var listaLivros = new List<LivroResponseDTO>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Livro 1" },
            new() { Id = Guid.NewGuid(), Titulo = "Livro 2" }
        };

        _mockService.Setup(s => s.GetAll()).Returns(listaLivros);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<List<LivroResponseDTO>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }
}
