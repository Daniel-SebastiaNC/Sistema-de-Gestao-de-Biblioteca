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

namespace BibliotecaApi.Tests.EmprestimoTests;

public class EmprestimoServiceTests
{
    private readonly Mock<IEmprestimoRepository> _mockEmprestimoRepository;
    private readonly Mock<ILivroRepository> _mockLivroRepository;
    private readonly IMapper _mapper;
    private readonly EmprestimoService _service;

    public EmprestimoServiceTests()
    {
        _mockEmprestimoRepository = new Mock<IEmprestimoRepository>();
        _mockLivroRepository = new Mock<ILivroRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EmprestimoProfile>();
            cfg.AddProfile<LivroProfile>();
            cfg.AddProfile<AutorProfile>();
            cfg.AddProfile<AlunoProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();

        _service = new EmprestimoService(
            _mockEmprestimoRepository.Object,
            _mockLivroRepository.Object,
            _mapper
        );
    }

    [Fact]
    public void AddEmprestimo_DeveCriarEmprestimoEDecrementarEstoque_QuandoValido()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var livroId = Guid.NewGuid();
        var dto = new CriarEmprestimoDTO(alunoId, livroId);

        var livro = new Livro
        {
            Id = livroId,
            Titulo = "Livro Teste",
            Quantidade = 3,
            ISBN = "123",
            Autor = new Autor { Id = Guid.NewGuid(), Nome = "Autor Teste" }
        };

        _mockEmprestimoRepository.Setup(r => r.ExistsEmpresitimoAtivo(alunoId, livroId)).Returns(false);
        _mockLivroRepository.Setup(r => r.GetLivroById(livroId)).Returns(livro);
        _mockEmprestimoRepository.Setup(r => r.AddEmprestimo(It.IsAny<Emprestimo>()))
            .Returns((Emprestimo e) => e);

        // Act
        var resultado = _service.AddEmprestimo(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusEmprestimo.Ativo, resultado.Status);
        Assert.Equal(2, livro.Quantidade);
        _mockLivroRepository.Verify(r => r.UpdateLivro(It.Is<Livro>(l => l.Quantidade == 2)), Times.Once);
        _mockEmprestimoRepository.Verify(r => r.AddEmprestimo(It.IsAny<Emprestimo>()), Times.Once);
    }

    [Fact]
    public void AddEmprestimo_DeveLancarConflictException_QuandoAlunoJaPossuiEmprestimoAtivoDoMesmoLivro()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var livroId = Guid.NewGuid();
        var dto = new CriarEmprestimoDTO(alunoId, livroId);

        _mockEmprestimoRepository.Setup(r => r.ExistsEmpresitimoAtivo(alunoId, livroId)).Returns(true);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.AddEmprestimo(dto));
        Assert.Equal("O aluno já possui um empréstimo ativo deste mesmo livro.", exception.Message);
        _mockLivroRepository.Verify(r => r.UpdateLivro(It.IsAny<Livro>()), Times.Never);
        _mockEmprestimoRepository.Verify(r => r.AddEmprestimo(It.IsAny<Emprestimo>()), Times.Never);
    }

    [Fact]
    public void AddEmprestimo_DeveLancarConflictException_QuandoLivroNaoExiste()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var livroId = Guid.NewGuid();
        var dto = new CriarEmprestimoDTO(alunoId, livroId);

        _mockEmprestimoRepository.Setup(r => r.ExistsEmpresitimoAtivo(alunoId, livroId)).Returns(false);
        _mockLivroRepository.Setup(r => r.GetLivroById(livroId)).Returns((Livro)null!);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.AddEmprestimo(dto));
        Assert.Equal("Livro não encontrado.", exception.Message);
    }

    [Fact]
    public void AddEmprestimo_DeveLancarConflictException_QuandoEstoqueZerado()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var livroId = Guid.NewGuid();
        var dto = new CriarEmprestimoDTO(alunoId, livroId);

        var livroSemEstoque = new Livro
        {
            Id = livroId,
            Titulo = "Livro Esgotado",
            Quantidade = 0
        };

        _mockEmprestimoRepository.Setup(r => r.ExistsEmpresitimoAtivo(alunoId, livroId)).Returns(false);
        _mockLivroRepository.Setup(r => r.GetLivroById(livroId)).Returns(livroSemEstoque);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.AddEmprestimo(dto));
        Assert.Equal("Livro indisponível no estoque.", exception.Message);
        _mockLivroRepository.Verify(r => r.UpdateLivro(It.IsAny<Livro>()), Times.Never);
    }

    [Fact]
    public void ReturnEmprestimo_DeveAtualizarStatusEIncrementarEstoque_QuandoEmprestimoAtivo()
    {
        // Arrange
        var emprestimoId = Guid.NewGuid();
        var livro = new Livro { Id = Guid.NewGuid(), Quantidade = 1, Titulo = "Livro Teste" };
        var emprestimo = new Emprestimo
        {
            Id = emprestimoId,
            Status = StatusEmprestimo.Ativo,
            DataEmprestimo = DateTime.Now.AddDays(-3),
            DataPrevistaDevolucao = DateTime.Now.AddDays(4),
            Livro = livro
        };

        _mockEmprestimoRepository.Setup(r => r.GetEmprestimoById(emprestimoId)).Returns(emprestimo);

        // Act
        var resultado = _service.ReturnEmprestimo(emprestimoId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusEmprestimo.Devolvido, emprestimo.Status);
        Assert.NotNull(emprestimo.DataDevolucao);
        Assert.Equal(2, livro.Quantidade);
        _mockEmprestimoRepository.Verify(r => r.UpdateEmprestimo(emprestimo), Times.Once);
    }

    [Fact]
    public void ReturnEmprestimo_DeveLancarNotFoundException_QuandoEmprestimoNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockEmprestimoRepository.Setup(r => r.GetEmprestimoById(idInexistente)).Returns((Emprestimo)null!);

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _service.ReturnEmprestimo(idInexistente));
        Assert.Equal("Empréstimo não encontrado.", exception.Message);
    }

    [Fact]
    public void ReturnEmprestimo_DeveLancarConflictException_QuandoEmprestimoJaDevolvido()
    {
        // Arrange
        var emprestimoId = Guid.NewGuid();
        var emprestimoDevolvido = new Emprestimo
        {
            Id = emprestimoId,
            Status = StatusEmprestimo.Devolvido
        };

        _mockEmprestimoRepository.Setup(r => r.GetEmprestimoById(emprestimoId)).Returns(emprestimoDevolvido);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.ReturnEmprestimo(emprestimoId));
        Assert.Equal("Este empréstimo já foi devolvido.", exception.Message);
        _mockEmprestimoRepository.Verify(r => r.UpdateEmprestimo(It.IsAny<Emprestimo>()), Times.Never);
    }

    [Fact]
    public void GetAll_DeveRetornarListaDeEmprestimos()
    {
        // Arrange
        var lista = new List<Emprestimo>
        {
            new Emprestimo { Id = Guid.NewGuid(), Status = StatusEmprestimo.Ativo },
            new Emprestimo { Id = Guid.NewGuid(), Status = StatusEmprestimo.Devolvido }
        };

        _mockEmprestimoRepository.Setup(r => r.GetAll()).Returns(lista);

        // Act
        var resultado = _service.GetAll();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
    }
}
