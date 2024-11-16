using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoSubstituteTest
    (Action<AutoSubstituteTestContext, LoggerConfiguration> configureLogger)
    : LoggerTest<AutoSubstituteTestContext>(new(configureLogger));

/// <summary>
///     A test using NSubstitute
/// </summary>
public abstract class AutoSubstituteTest<TContext>(TContext context) : LoggerTest<TContext>(context)
    where TContext : class, IAutoSubstituteTestContext
{
    private static readonly IConfiguration _readOnlyConfiguration = new ConfigurationBuilder().Build();
    private AutoSubstitute? _autoSubstitute;
    private bool _building;

    /// <summary>
    ///     The Configuration if defined otherwise empty.
    /// </summary>
    protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.Resolve<IConfiguration>() : _readOnlyConfiguration;

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
        container.RegisterDelegate(
            context => CreateLoggerFactory(
                context.Resolve<LoggerProviderCollection>(IfUnresolved.ReturnDefault) ?? new LoggerProviderCollection()
            )
        );
        container.RegisterDelegate(context => context.Resolve<ILoggerFactory>().CreateLogger("Test"));
        container.RegisterInstance(Logger);
        return BuildContainer(container.WithDependencyInjectionAdapter());
    }
}
