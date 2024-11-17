using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoMockTest
    (Action<AutoMockTestContext, LoggerConfiguration> configureLogger, MockBehavior mockBehavior = MockBehavior.Default)
    : LoggerTest<AutoMockTestContext>(new(configureLogger, mockBehavior));

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoMockTest<TContext>(TContext context) : LoggerTest<TContext>(context)
    where TContext : class, IAutoMockTestContext
{
    private static readonly IConfiguration _readOnlyConfiguration = new ConfigurationBuilder().Build();
    private AutoMock? _autoMock;
    private bool _building;

    /// <summary>
    ///     The Configuration if defined otherwise empty.
    /// </summary>
    protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.Resolve<IConfiguration>() : _readOnlyConfiguration;

    /// <summary>
    ///     The AutoMock instance
    /// </summary>
    protected AutoMock AutoMock => _autoMock ??= Rebuild();

    /// <summary>
    ///     The DryIoc container
    /// </summary>
    protected IContainer Container
    {
        get => AutoMock.Container;
        private set => _autoMock = Rebuild(value);
    }

    /// <summary>
    ///     The Service Provider
    /// </summary>
    protected IServiceProvider ServiceProvider => AutoMock.Container;

    /// <summary>
    ///     Force the container to rebuild from scratch
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected AutoMock Rebuild(IContainer? container = null)
    {
        if (_building) throw new TestBootstrapException($"Unable to access {nameof(AutoMock)} while the container is being constructed!");
        _building = true;
        var autoFake = new AutoMock(new MockRepository(TestContext.MockBehavior), configureAction: ConfigureContainer, container: container);
        _building = false;
        return autoFake;
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
        return BuildContainer(container);
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
        return container.With(rules => rules.WithBaseMicrosoftDependencyInjectionRules(null));
    }
}
