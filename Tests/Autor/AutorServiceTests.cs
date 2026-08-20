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

namespace BibliotecaApi.Tests.AutorTests;

public class AutorServiceTests
{
    private readonly Mock<IAutorRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly AutorService _service;

    public AutorServiceTests()
    {
        _mockRepository = new Mock<IAutorRepository>();

        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<AutorProfile>(),
            NullLoggerFactory.Instance
        );
        _mapper = config.CreateMapper();

        _service = new AutorService(_mockRepository.Object, _mapper);
    }

    [Fact]
    public void AddAutor_DeveRetornarAutorResponseDto_QuandoDadosValidos()
    {
        // Arrange
        var dto = new CriarAutorDto("Machado de Assis", new DateTime(1839, 6, 21), "Brasileira");
        var autorSalvo = new Autor
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            DataNascimento = dto.DataNascimento,
            Nacionalidade = dto.Nacionalidade
        };

        _mockRepository.Setup(r => r.AddAutor(It.IsAny<Autor>())).Returns(autorSalvo);

        // Act
        var resultado = _service.AddAutor(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(dto.Nome, resultado.Nome);
        Assert.Equal(dto.DataNascimento, resultado.DataNascimento);
        Assert.Equal(dto.Nacionalidade, resultado.Nacionalidade);
        _mockRepository.Verify(r => r.AddAutor(It.IsAny<Autor>()), Times.Once);
    }

    [Fact]
    public void GetAutorById_DeveRetornarAutorResponseDto_QuandoAutorExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var autorFake = new Autor
        {
            Id = id,
            Nome = "Clarice Lispector",
            DataNascimento = new DateTime(1920, 12, 10),
            Nacionalidade = "Brasileira"
        };

        _mockRepository.Setup(r => r.GetAutorById(id)).Returns(autorFake);

        // Act
        var resultado = _service.GetAutorById(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);
        Assert.Equal("Clarice Lispector", resultado.Nome);
    }

    [Fact]
    public void GetAutorById_DeveLancarNotFoundException_QuandoAutorNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAutorById(id)).Returns((Autor)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.GetAutorById(id));
        Assert.Contains(id.ToString(), exception.Message);
    }

    [Fact]
    public void GetAllAutores_DeveRetornarListaDeAutores_QuandoExistemAutores()
    {
        // Arrange
        var autores = new List<Autor>
        {
            new Autor { Id = Guid.NewGuid(), Nome = "Autor 1", DataNascimento = DateTime.Now, Nacionalidade = "BR" },
            new Autor { Id = Guid.NewGuid(), Nome = "Autor 2", DataNascimento = DateTime.Now, Nacionalidade = "PT" }
        };

        _mockRepository.Setup(r => r.GetAllAutores()).Returns(autores);

        // Act
        var resultado = _service.GetAllAutores();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public void UpdateAutor_DeveAtualizarERetornarAutorResponseDto_QuandoAutorExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var autorExistente = new Autor
        {
            Id = id,
            Nome = "Nome Antigo",
            DataNascimento = DateTime.Now.AddYears(-30),
            Nacionalidade = "BR"
        };

        var dtoAtualizacao = new CriarAutorDto("Nome Novo", new DateTime(1990, 1, 1), "Brasileira");

        _mockRepository.Setup(r => r.GetAutorById(id)).Returns(autorExistente);

        // Act
        var resultado = _service.UpdateAutor(id, dtoAtualizacao);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Nome Novo", resultado.Nome);
        Assert.Equal("Brasileira", resultado.Nacionalidade);
        _mockRepository.Verify(r => r.UpdateAutor(It.Is<Autor>(a => a.Nome == "Nome Novo")), Times.Once);
    }

    [Fact]
    public void UpdateAutor_DeveLancarNotFoundException_QuandoAutorNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CriarAutorDto("Nome", DateTime.Now, "BR");
        _mockRepository.Setup(r => r.GetAutorById(id)).Returns((Autor)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.UpdateAutor(id, dto));
        Assert.Contains(id.ToString(), exception.Message);
        _mockRepository.Verify(r => r.UpdateAutor(It.IsAny<Autor>()), Times.Never);
    }

    [Fact]
    public void DeleteAutor_DeveDeletarAutor_QuandoAutorExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var autorExistente = new Autor { Id = id, Nome = "Autor Teste" };

        _mockRepository.Setup(r => r.GetAutorById(id)).Returns(autorExistente);

        // Act
        _service.DeleteAutor(id);

        // Assert
        _mockRepository.Verify(r => r.DeleteAutor(autorExistente), Times.Once);
    }

    [Fact]
    public void DeleteAutor_DeveLancarNotFoundException_QuandoAutorNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetAutorById(id)).Returns((Autor)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.DeleteAutor(id));
        Assert.Contains(id.ToString(), exception.Message);
        _mockRepository.Verify(r => r.DeleteAutor(It.IsAny<Autor>()), Times.Never);
    }
}
