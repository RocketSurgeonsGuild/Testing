using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// Automatically creates fakes for requested services that haven't been registered
    /// </summary>
    public sealed class AutoMock : IDisposable
    {
        /// <summary>
        /// Create a container that automatically fakes unknown types
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="container"></param>
        /// <param name="configureAction"></param>
        public AutoMock(
            MockBehavior behavior,
            IContainer container = null,
            Func<IContainer, IContainer> configureAction = null
        )
            : this(new MockRepository(behavior), container, configureAction) { }

        /// <summary>
        /// Create a container that automatically fakes unknown types
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="container"></param>
        /// <param name="configureAction"></param>
        public AutoMock(
            MockRepository repository,
            IContainer container = null,
            Func<IContainer, IContainer> configureAction = null
        )
        {
            Container = container ?? new Container();

            Container = Container.With(
                rules =>
                {
                    var createMethod = typeof(MockRepository).GetMethod(nameof(MockRepository.Create), Array.Empty<Type>());
                    return rules
                       .WithTestLoggerResolver(
                            (request, loggerType) => ActivatorUtilities.CreateInstance(request.Container, loggerType)
                        )
                       .WithUndefinedTestDependenciesResolver(
                            request => ( (Mock)createMethod
                                   .MakeGenericMethod(request.ServiceType)
                                   .Invoke(repository, Array.Empty<object>())
                                ).Object
                        )
                       .WithConcreteTypeDynamicRegistrations((type, o) => true, Reuse.Transient);
                }
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
        /// Finds (creating if needed) the actual mock for the provided type.
        /// </summary>
        /// <typeparam name="T">Type to mock.</typeparam>
        /// <returns>A mock of type <typeparamref name="T"/>.</returns>
        public Mock<T> Mock<T>()
            where T : class
        {
            var obj = (IMocked<T>)Container.Resolve<T>();
            return obj.Mock;
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters.</param>
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            Container.RegisterInstance(instance);
            return instance;
        }

        void IDisposable.Dispose() => Container.Dispose();
    }
}