using EBL.FIG.Process.Identity.Infra.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace EBL.FIG.Process.Identity.Tests.Infra.Data.Interceptors;

public class TelemetryInterceptorTests
{
    private readonly Mock<ILogger<TelemetryInterceptor>> _loggerMock = new();

    private TelemetryInterceptor CreateSut() => new(_loggerMock.Object);

    #region Construtor

    [Fact(DisplayName = "TelemetryInterceptor - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "TelemetryInterceptor - Deve herdar de DbCommandInterceptor")]
    [Trait("Infra.Data", "")]
    public void TelemetryInterceptor_DeveHerdarDeDbCommandInterceptor()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<DbCommandInterceptor>(sut);
    }

    [Fact(DisplayName = "TelemetryInterceptor - Não deve lançar exceção ao receber logger nulo")]
    [Trait("Infra.Data", "")]
    public void Constructor_LoggerNulo_NaoDeveLancarExcecao()
    {
        var exception = Record.Exception(() => new TelemetryInterceptor(null!));

        Assert.Null(exception);
    }

    #endregion

    #region ReaderExecuting

    [Fact(DisplayName = "ReaderExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var result = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = sut.ReaderExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "NonQueryExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void NonQueryExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("UPDATE Tenants SET Name='x'");
        var result = InterceptionResult<int>.SuppressWithResult(0);

        var actual = sut.NonQueryExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ScalarExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM Tenants");
        var result = InterceptionResult<object>.SuppressWithResult(1);

        var actual = sut.ScalarExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ReaderExecuting - Não deve logar quando debug não está habilitado")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuting_DebugDesabilitado_NaoDeveLogar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");

        var actual = sut.ReaderExecuting(command, CreateCommandEventData(), default);

        _loggerMock.Verify(
            x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region CommandFailed

    [Fact(DisplayName = "CommandFailed - Deve registrar log de erro")]
    [Trait("Infra.Data", "")]
    public void CommandFailed_Sucesso_DeveLogarErro()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new InvalidOperationException("Erro de teste"));

        sut.CommandFailed(command, eventData);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "CommandFailedAsync - Deve registrar log de erro")]
    [Trait("Infra.Data", "")]
    public async Task CommandFailedAsync_Sucesso_DeveLogarErro()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new InvalidOperationException("Erro async"));

        await sut.CommandFailedAsync(command, eventData);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ReaderExecutingAsync

    [Fact(DisplayName = "ReaderExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var result = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "NonQueryExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("DELETE FROM Tenants WHERE Id=1");
        var result = InterceptionResult<int>.SuppressWithResult(0);

        var actual = await sut.NonQueryExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ScalarExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var result = InterceptionResult<object>.SuppressWithResult(42);

        var actual = await sut.ScalarExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    #endregion

    #region Helpers

    private static CommandEventData CreateCommandEventData()
    {
        return (CommandEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandEventData));
    }

    private static CommandErrorEventData CreateCommandErrorEventData(Exception exception)
    {
        var data = (CommandErrorEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandErrorEventData));

        var field = typeof(CommandErrorEventData)
            .GetField("<Exception>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(data, exception);

        return data;
    }

    #endregion
}
