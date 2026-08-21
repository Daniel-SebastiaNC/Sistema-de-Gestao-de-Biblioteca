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

namespace BibliotecaApi.Tests.LivroTests;

public class LivroServiceTests
{
    private readonly Mock<ILivroRepository> _mockLivroRepository;
    private readonly Mock<IAutorRepository> _mockAutorRepository;
    private readonly IMapper _mapper;
    private readonly LivroService _service;

    public LivroServiceTests()
    {
        _mockLivroRepository = new Mock<ILivroRepository>();
        _mockAutorRepository = new Mock<IAutorRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<LivroProfile>();
            cfg.AddProfile<AutorProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();

        _service = new LivroService(_mockLivroRepository.Object, _mockAutorRepository.Object, _mapper);
    }

    [Fact]
    public void AddLivro_DeveRetornarLivroResponseDTO_QuandoAutorExiste()
    {
        // Arrange
        var autorId = Guid.NewGuid();
        var dto = new CriarLivroDto("978-3-16-148410-0", "Dom Casmurro", 1899, autorId, 5);

        var autorFake = new Autor
        {
            Id = autorId,
            Nome = "Machado de Assis",
            DataNascimento = new DateTime(1839, 6, 21),
            Nacionalidade = "Brasileira"
        };

        var livroSalvo = new Livro
        {
            Id = Guid.NewGuid(),
            ISBN = dto.ISBN,
            Titulo = dto.Titulo,
            AnoPublicacao = dto.AnoPublicacao,
            Quantidade = dto.QuantidadeDisponivel,
            AutorId = autorId,
            Autor = autorFake
        };

        _mockAutorRepository.Setup(r => r.GetAutorById(autorId)).Returns(autorFake);
        _mockLivroRepository.Setup(r => r.AddLivro(It.IsAny<Livro>())).Returns(livroSalvo);

        // Act
        var resultado = _service.AddLivro(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(dto.Titulo, resultado.Titulo);
        Assert.Equal(dto.ISBN, resultado.ISBN);
        Assert.Equal(dto.QuantidadeDisponivel, resultado.Quantidade);
        Assert.Equal("Machado de Assis", resultado.Autor.Nome);
        _mockLivroRepository.Verify(r => r.AddLivro(It.IsAny<Livro>()), Times.Once);
    }

    [Fact]
    public void AddLivro_DeveLancarNotFoundException_QuandoAutorNaoExiste()
    {
        // Arrange
        var autorId = Guid.NewGuid();
        var dto = new CriarLivroDto("978-3-16-148410-0", "Dom Casmurro", 1899, autorId, 5);

        _mockAutorRepository.Setup(r => r.GetAutorById(autorId)).Returns((Autor)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.AddLivro(dto));
        Assert.Contains(autorId.ToString(), exception.Message);
        _mockLivroRepository.Verify(r => r.AddLivro(It.IsAny<Livro>()), Times.Never);
    }

    [Fact]
    public void GetLivrosByAutorOrTitle_DeveRetornarListaDeLivros()
    {
        // Arrange
        var livros = new List<Livro>
        {
            new Livro
            {
                Id = Guid.NewGuid(),
                Titulo = "Dom Casmurro",
                ISBN = "123",
                Autor = new Autor { Id = Guid.NewGuid(), Nome = "Machado de Assis" }
            }
        };

        _mockLivroRepository.Setup(r => r.GetLivrosByAutorOrTitle("Dom", null)).Returns(livros);

        // Act
        var resultado = _service.GetLivrosByAutorOrTitle("Dom", null);

        // Assert
        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Dom Casmurro", resultado[0].Titulo);
    }

    [Fact]
    public void GetLivrosById_DeveRetornarLivroResponseDTO_QuandoLivroExiste()
    {
        // Arrange
        var livroId = Guid.NewGuid();
        var livroFake = new Livro
        {
            Id = livroId,
            Titulo = "Memórias Póstumas de Brás Cubas",
            ISBN = "456",
            Autor = new Autor { Id = Guid.NewGuid(), Nome = "Machado de Assis" }
        };

        _mockLivroRepository.Setup(r => r.GetLivroById(livroId)).Returns(livroFake);

        // Act
        var resultado = _service.GetLivrosById(livroId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(livroId, resultado.Id);
        Assert.Equal("Memórias Póstumas de Brás Cubas", resultado.Titulo);
    }

    [Fact]
    public void GetLivrosById_DeveLancarNotFoundException_QuandoLivroNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockLivroRepository.Setup(r => r.GetLivroById(idInexistente)).Returns((Livro)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.GetLivrosById(idInexistente));
        Assert.Contains(idInexistente.ToString(), exception.Message);
    }

    [Fact]
    public void GetAll_DeveRetornarListaDeTodosOsLivros()
    {
        // Arrange
        var livros = new List<Livro>
        {
            new Livro { Id = Guid.NewGuid(), Titulo = "Livro 1", Autor = new Autor { Nome = "Autor 1" } },
            new Livro { Id = Guid.NewGuid(), Titulo = "Livro 2", Autor = new Autor { Nome = "Autor 2" } }
        };

        _mockLivroRepository.Setup(r => r.GetAllLivros()).Returns(livros);

        // Act
        var resultado = _service.GetAll();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
    }
}
