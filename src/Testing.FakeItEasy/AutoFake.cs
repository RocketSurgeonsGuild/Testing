using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DryIoc;
using FakeItEasy.Creation;
using FakeItEasy.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// Automatically creates fakes for requested services that haven't been registered
    /// </summary>
    public sealed class AutoFake : IDisposable
    {
        /// <summary>
        /// Create a container that automatically fakes unknown types
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
            Container = container ?? new Container();
            if (fakeOptionsAction == null)
                fakeOptionsAction = options => { };
            Container = Container

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
                       .WithConcreteTypeDynamicRegistrations((type, o) => true, Reuse.Transient)
                );
            if (configureAction != null)
                Container = configureAction.Invoke(Container);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service.</returns>
        public T Resolve<T>() => Container.Resolve<T>();

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
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
        /// Resolve the specified type in the container (register specified instance if needed).
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

        void IDisposable.Dispose() => Container.Dispose();
    }
}