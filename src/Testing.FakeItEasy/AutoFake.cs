using System.Diagnostics.CodeAnalysis;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy.Creation;
using FakeItEasy.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Automatically creates fakes for requested services that haven't been registered
/// </summary>
public sealed class AutoFake : IDisposable
{
    /// <summary>
    ///     Create a container that automatically fakes unknown types
    /// </summary>
    /// <param name="fakeOptionsAction"></param>
    /// <param name="container"></param>
    /// <param name="configureAction"></param>
    public AutoFake(
        IContainer? container = null,
        Func<IContainer, IContainer>? configureAction = null,
        Action<IFakeOptions>? fakeOptionsAction = null
    )
    {
        container ??= new Container();
        fakeOptionsAction ??= _ => { };
        container = container
           .With(
                rules => rules
                        .WithTestLoggerResolver(
                             (request, loggerType) => Create.Fake(
                                 request.ServiceType,
                                 x
                                     => x.Wrapping(ActivatorUtilities.CreateInstance(request.Container, loggerType))
                             )
                         )
                        .WithUndefinedTestDependenciesResolver(request => Create.Fake(request.ServiceType, fakeOptionsAction))
                        .WithConcreteTypeDynamicRegistrations((_, _) => true, Reuse.Transient)
            );
        if (configureAction != null)
            DryIoc = configureAction.Invoke(container).WithDependencyInjectionAdapter();
    }

    /// <summary>
    ///     Gets the <see cref="IContainer" /> that handles the component resolution.
    /// </summary>
    public DryIocServiceProvider DryIoc { get; }

    /// <summary>
    ///     Resolve the specified type in the container (register it if needed).
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service.</returns>
    public T Resolve<T>()
    {
        return DryIoc.Container.Resolve<T>();
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
        DryIoc.Container.Register<TService, TImplementation>(Reuse.Singleton);
        return DryIoc.Container.Resolve<TService>();
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
        DryIoc.Container.RegisterInstance(instance);
        return instance;
    }

#pragma warning disable CA1063
    void IDisposable.Dispose()
#pragma warning restore CA1063
    {
        DryIoc.Dispose();
    }
}
