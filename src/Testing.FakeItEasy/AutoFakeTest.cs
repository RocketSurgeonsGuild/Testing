using System.Diagnostics.CodeAnalysis;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy;
using FakeItEasy.Creation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using ILogger = Serilog.ILogger;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoFakeTest : LoggerTest
{
    private static readonly IConfiguration ReadOnlyConfiguration = new ConfigurationBuilder().Build();
    private readonly Action<IFakeOptions>? _fakeOptionsAction;
    private AutoFake? _autoFake;
    private bool _building;

    /// <summary>
    ///     The Configuration if defined otherwise empty.
    /// </summary>
    protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.Resolve<IConfiguration>() : ReadOnlyConfiguration;

    /// <summary>
    ///     The AutoFake instance
    /// </summary>
    protected AutoFake AutoFake => _autoFake ??= Rebuild();

    /// <summary>
    ///     The DryIoc container
    /// </summary>
    protected IContainer Container
    {
        get => AutoFake.Container;
        private set => _autoFake = Rebuild(value);
    }

    /// <summary>
    ///     Force the container to rebuild from scratch
    /// </summary>
    /// <exception cref="ApplicationException"></exception>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    protected AutoFake Rebuild(IContainer? container = null)
    {
        if (_building) throw new TestBootstrapException($"Unable to access {nameof(AutoFake)} while the container is being constructed!");
        _building = true;
        var autoFake = new AutoFake(configureAction: ConfigureContainer, fakeOptionsAction: _fakeOptionsAction, container: container);
        _building = false;
        return autoFake;
    }

    /// <summary>
    ///     The Service Provider
    /// </summary>
    protected IServiceProvider ServiceProvider => AutoFake.Container;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <param name="fakeOptionsAction"></param>
    protected AutoFakeTest(
        ITestOutputHelper outputHelper,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null,
        Action<IFakeOptions>? fakeOptionsAction = null
    )
        : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger, fakeOptionsAction)
    {
    }

    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <param name="fakeOptionsAction"></param>
    protected AutoFakeTest(
        ITestOutputHelper outputHelper,
        LogLevel minLevel,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null,
        Action<IFakeOptions>? fakeOptionsAction = null
    )
        : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger, fakeOptionsAction)
    {
    }

    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <param name="fakeOptionsAction"></param>
    protected AutoFakeTest(
        ITestOutputHelper outputHelper,
        LogEventLevel minLevel,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null,
        Action<IFakeOptions>? fakeOptionsAction = null
    )
        : base(outputHelper, minLevel, logFormat, configureLogger)
    {
        _fakeOptionsAction = fakeOptionsAction;
    }
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

    private IContainer ConfigureContainer(IContainer container)
    {
        container.RegisterInstance(LoggerFactory);
        container.RegisterInstance(Logger);
        container.RegisterInstance(SerilogLogger);
        return BuildContainer(container.WithDependencyInjectionAdapter());
    }

    /// <summary>
    ///     Populate the test class with the given configuration and services
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method is obsolete you can use the overload with IServiceCollection or IContainer instead.")]
    protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context)
    {
        Populate(context.configuration, context.serviceCollection);
    }

    /// <summary>
    ///     Populate the test class with the given configuration and services
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method is obsolete you can use the overload with IServiceCollection or IContainer instead.")]
    protected void Populate(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        Container.Populate(serviceCollection);
        Container.RegisterInstance(configuration);
    }

    /// <summary>
    ///     Populate the test class with the given configuration and services
    /// </summary>
    protected void Populate(IServiceCollection serviceCollection)
    {
        Container.Populate(serviceCollection);
    }

    /// <summary>
    ///     Populate the test class with the given configuration and services
    /// </summary>
    protected void Populate(IContainer container)
    {
        Container = container;
    }

    /// <summary>
    ///     A method that allows you to override and update the behavior of building the container
    /// </summary>
    protected virtual IContainer BuildContainer(IContainer container)
    {
        return container;
    }

    /// <summary>
    ///     Control the way that the serilog logger factory is created.
    /// </summary>
    protected override ILoggerFactory CreateLoggerFactory(
        ILogger logger,
        LoggerProviderCollection loggerProviderCollection
    )
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var factory =
            new FakeItEasyLoggerFactory(new SerilogLoggerFactory(logger, false, loggerProviderCollection));
#pragma warning restore CA2000 // Dispose objects before losing scope
        return A.Fake<ILoggerFactory>(l => l.Wrapping(factory));
    }
}
