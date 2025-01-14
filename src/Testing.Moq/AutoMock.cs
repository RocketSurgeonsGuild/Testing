using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Automatically creates fakes for requested services that haven't been registered
/// </summary>
[PublicAPI]
public sealed class AutoMock : IDisposable
{
    /// <summary>
    ///     Create a container that automatically fakes unknown types
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="container"></param>
    /// <param name="configureAction"></param>
    public AutoMock(
        MockBehavior behavior,
        IContainer? container = null,
        Func<IContainer, IContainer>? configureAction = null
    )
        : this(new MockRepository(behavior), container, configureAction) { }

    /// <summary>
    ///     Create a container that automatically fakes unknown types
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="container"></param>
    /// <param name="configureAction"></param>
    public AutoMock(
        MockRepository repository,
        IContainer? container = null,
        Func<IContainer, IContainer>? configureAction = null
    )
    {
        container ??= new Container();

        container = container.With(
            rules =>
            {
                var createMethod = typeof(MockRepository).GetMethod(nameof(MockRepository.Create), Array.Empty<Type>())!;
                return rules
                      .WithTestLoggerResolver(
                           (request, loggerType) => ActivatorUtilities.CreateInstance(request.Container, loggerType)
                       )
                      .WithUndefinedTestDependenciesResolver(
                           request => ( (Mock)createMethod
                                             .MakeGenericMethod(request.ServiceType)
                                             .Invoke(repository, Array.Empty<object>())!
                               ).Object
                       )
                      .WithConcreteTypeDynamicRegistrations((_, _) => true, Reuse.Transient);
            }
        );

        if (configureAction != null)
            container = configureAction.Invoke(container);
        ServiceProvider = container.WithDependencyInjectionAdapter();
    }

    /// <summary>
    ///     Gets the <see cref="IContainer" /> that handles the component resolution.
    /// </summary>
    public IContainer Container => ServiceProvider.Container;

    /// <summary>
    /// The dryioc service provider
    /// </summary>
    public DryIocServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     Resolve the specified type in the container (register it if needed).
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service.</returns>
    public T Resolve<T>() => Container.Resolve<T>();

    /// <summary>
    ///     Finds (creating if needed) the actual mock for the provided type.
    /// </summary>
    /// <typeparam name="T">Type to mock.</typeparam>
    /// <returns>A mock of type <typeparamref name="T" />.</returns>
    public Mock<T> Mock<T>()
        where T : class
    {
        var obj = (IMocked<T>)Container.Resolve<T>();
        return obj.Mock;
    }

    /// <summary>
    ///     Resolve the specified type in the container (register it if needed).
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
    /// <returns>The service.</returns>
    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The container responsible for registration disposal."
    )]
    public TService Provide<TService, TImplementation>()
        where TImplementation : TService
    {
        Container.Register<TService, TImplementation>(Reuse.Singleton);
        return Container.Resolve<TService>();
    }

    /// <summary>
    ///     Resolve the specified type in the container (register specified instance if needed).
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="instance">The instance to register if needed.</param>
    /// <returns>The instance resolved from container.</returns>
    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The component registry is responsible for registration disposal."
    )]
    public TService Provide<TService>(TService instance)
        where TService : class
    {
        Container.RegisterInstance(instance);
        return instance;
    }

    #pragma warning disable CA1063
    void IDisposable.Dispose()
        #pragma warning restore CA1063
    {
        Container.Dispose();
    }
}
