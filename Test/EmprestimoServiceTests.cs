using Xunit;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
public class EmprestimoServiceTests
{
    [Fact]
    public void DeveIndicarQueLivroEstaDisponivel()
    {
        // Arrange: Instancia o serviço e define um cenário com estoque positivo
        var service = new EmprestimoService();
        int quantidadeDisponivel = 3;
        // Act: Executa a validação de disponibilidade
        var resultado = service.LivroDisponivel(quantidadeDisponivel);
        // Assert: Verifica se o sistema identifica corretamente a disponibilidade
        Assert.True(resultado);
    }
    [Fact]
    public void DeveIndicarQueLivroNaoEstaDisponivel()
    {
        // Arrange: Define um cenário onde o estoque está zerado
        var service = new EmprestimoService();
        int quantidadeEsgotada = 0;
        // Act: Executa a validação
        var resultado = service.LivroDisponivel(quantidadeEsgotada);
        // Assert: O retorno esperado deve ser falso
        Assert.False(resultado);
    }

    [Theory]
    [InlineData(1, 2)] // 1 dia de atraso -> R$ 2,00
    [InlineData(5, 10)] // 5 dias de atraso -> R$ 10,00
    [InlineData(10, 20)]// 10 dias de atraso -> R$ 20,00
    public void DeveCalcularMulta(int dias, decimal valorEsperado)
    {
        // Arrange: Setup do serviço de empréstimo
        var service = new EmprestimoService();
        // Act: Processamento do cálculo baseado nos inputs da teoria
        var resultado = service.CalcularMulta(dias);
        // Assert: Validação do valor calculado contra a expectativa do negócio
        Assert.Equal(valorEsperado, resultado);
    }
    
    [Fact]
    public void DeveLancarExcecaoQuandoLivroIndisponivel()
    {
    // Arrange: Configura cenário crítico de estoque zerado
    var service = new EmprestimoService();
    int quantidadeIndisponivel = 0;
    
    // Act & Assert: Verifica se a exceção RegraNegocioException é disparada
    Assert.Throws<RegraNegocioException>(
    () => service.ValidarDisponibilidade(quantidadeIndisponivel)
    );
    
    }
    

}

