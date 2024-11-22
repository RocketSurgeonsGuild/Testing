using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Automatically creates substitute for requested services that haven't been registered
/// </summary>
public sealed class AutoSubstitute : IDisposable
{
    /// <summary>
    ///     Create a container that automatically substitutes unknown types.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="configureAction"></param>
    public AutoSubstitute(
        IContainer? container = null,
        Func<IContainer, IContainer>? configureAction = null
    )
    {
        container ??= new Container();

        container = container
           .With(
                rules => rules
                        .WithTestLoggerResolver(
                             (request, loggerType) => ActivatorUtilities.CreateInstance(request.Container, loggerType)
                         )
                        .WithUndefinedTestDependenciesResolver(
                             request =>
                                 Substitute.For(new[] { request.ServiceType, }, Array.Empty<object>())
                         )
                        .WithConcreteTypeDynamicRegistrations((_, _) => true, Reuse.Transient)
            );

        if (configureAction != null)
            container = configureAction.Invoke(container);
        Container = container.WithDependencyInjectionAdapter().Container;
    }

    /// <summary>
    ///     Gets the <see cref="IContainer" /> that handles the component resolution.
    /// </summary>
    public IContainer Container { get; }

    /// <summary>
    ///     Resolve the specified type in the container (register it if needed).
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service.</returns>
    public T Resolve<T>() => Container.Resolve<T>();

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

    /// <summary>
    ///     Resolve the specified type in the container (register specified instance if needed).
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The instance resolved from container.</returns>
    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The component registry is responsible for registration disposal."
    )]
    public TService Provide<TService, TImplementation>() where TImplementation : TService
    {
        Container.Register<TService, TImplementation>();
        return Container.Resolve<TService>();
    }

    #pragma warning disable CA1063
    void IDisposable.Dispose()
    {
        Container.Dispose();
    }
    #pragma warning restore CA1063
}
