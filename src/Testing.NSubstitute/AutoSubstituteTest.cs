using System.Diagnostics.CodeAnalysis;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A test using NSubstitute
/// </summary>
public abstract class AutoSubstituteTest : LoggerTest
{
    private static readonly IConfiguration ReadOnlyConfiguration = new ConfigurationBuilder().Build();
    private AutoSubstitute? _autoSubstitute;
    private bool _building;

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    protected AutoSubstituteTest(
        ITestOutputHelper outputHelper,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null
    )
        : this(outputHelper, LogLevel.Information, logFormat, configureLogger)
    {
    }

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    protected AutoSubstituteTest(
        ITestOutputHelper outputHelper,
        LogLevel minLevel,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null
    )
        : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger)
    {
    }

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    protected AutoSubstituteTest(
        ITestOutputHelper outputHelper,
        LogEventLevel minLevel,
        string? logFormat = null,
        Action<LoggerConfiguration>? configureLogger = null
    ) : base(outputHelper, minLevel, logFormat, configureLogger)
    {
    }

    /// <summary>
    ///     The Configuration if defined otherwise empty.
    /// </summary>
    protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.Resolve<IConfiguration>() : ReadOnlyConfiguration;

    /// <summary>
    ///     The AutoFake instance
    /// </summary>
    protected AutoSubstitute AutoSubstitute => _autoSubstitute ??= Rebuild();

    /// <summary>
    ///     The DryIoc container
    /// </summary>
    protected IContainer Container
    {
        get => AutoSubstitute.Container;
        private set => _autoSubstitute = Rebuild(value);
    }

    /// <summary>
    ///     Force the container to rebuild from scratch
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected AutoSubstitute Rebuild(IContainer? container = null)
    {
        if (_building) throw new TestBootstrapException($"Unable to access {nameof(AutoSubstitute)} while the container is being constructed!");
        _building = true;
        var autoFake = new AutoSubstitute(configureAction: ConfigureContainer, container: container);
        _building = false;
        return autoFake;
    }

    /// <summary>
    ///     The Service Provider
    /// </summary>
    protected IServiceProvider ServiceProvider => AutoSubstitute.Container;

    /// <summary>
    ///     A method that allows you to override and update the behavior of building the container
    /// </summary>
    protected virtual IContainer BuildContainer(IContainer container)
    {
        return container;
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

    private IContainer ConfigureContainer(IContainer container)
    {
        container.RegisterInstance(LoggerFactory);
        container.RegisterInstance(Logger);
        container.RegisterInstance(SerilogLogger);
        return BuildContainer(container.WithDependencyInjectionAdapter());
    }
}
