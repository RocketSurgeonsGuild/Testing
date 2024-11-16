using System.Diagnostics.CodeAnalysis;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy;
using FakeItEasy.Creation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoFakeTest
    (Action<AutoFakeTestContext, LoggerConfiguration> configureLogger, Action<IFakeOptions>? fakeOptionsAction = null)
    : LoggerTest<AutoFakeTestContext>(new(configureLogger, fakeOptionsAction));

/// <summary>
///     A base class with AutoFake wired in for DryIoc
/// </summary>
public abstract class AutoFakeTest<TContext>(TContext context) : LoggerTest<TContext>(context)
    where TContext : class, IAutoFakeTestContext
{
    private static readonly IConfiguration _readOnlyConfiguration = new ConfigurationBuilder().Build();
    private AutoFake? _autoFake;
    private bool _building;

    /// <summary>
    ///     The Configuration if defined otherwise empty.
    /// </summary>
    protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.Resolve<IConfiguration>() : _readOnlyConfiguration;

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
        var autoFake = new AutoFake(configureAction: ConfigureContainer, fakeOptionsAction: TestContext.FakeOptionsAction, container: container);
        _building = false;
        return autoFake;
    }

    /// <summary>
    ///     The Service Provider
    /// </summary>
    protected IServiceProvider ServiceProvider => AutoFake.Container;

    private IContainer ConfigureContainer(IContainer container)
    {
        container.RegisterDelegate(
            context =>
            {
                var factory = new FakeItEasyLoggerFactory(
                    CreateLoggerFactory(context.Resolve<LoggerProviderCollection>(IfUnresolved.ReturnDefault) ?? new LoggerProviderCollection())
                );
                return A.Fake<ILoggerFactory>(l => l.Wrapping(factory));
            }
        );
        container.RegisterDelegate(context => context.Resolve<ILoggerFactory>().CreateLogger("Test"));
        container.RegisterInstance(Logger);
        return BuildContainer(container.WithDependencyInjectionAdapter());
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
}
