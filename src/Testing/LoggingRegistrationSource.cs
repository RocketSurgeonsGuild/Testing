using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    class LoggingRegistrationSource : IRegistrationSource
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _defaultLogger;

        public LoggingRegistrationSource(ILoggerFactory loggerFactory, ILogger defaultLogger)
        {
            _loggerFactory = loggerFactory;
            _defaultLogger = defaultLogger;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (!(service is TypedService typedService)) yield break;
            if (typedService.ServiceType.IsClosedTypeOf(typeof(ILogger<>)))
            {
                var registration = RegistrationBuilder.ForType(
                        typeof(Logger<>).MakeGenericType(typedService.ServiceType.GetGenericArguments()[0]))
                    .As(typedService.ServiceType);
                yield return registration.CreateRegistration();
            }

            if (typedService.ServiceType == typeof(ILogger))
            {
                yield return new ComponentRegistration(
                    Guid.NewGuid(),
                    new ProvidedInstanceActivator(_defaultLogger),
                    new RootScopeLifetime(),
                    InstanceSharing.Shared,
                    InstanceOwnership.OwnedByLifetimeScope,
                    new[] { service },
                    new Dictionary<string, object>());
            }

            if (typedService.ServiceType == typeof(ILoggerFactory))
            {
                yield return new ComponentRegistration(
                    Guid.NewGuid(),
                    new ProvidedInstanceActivator(A.Fake<ILoggerFactory>(x => x.Wrapping(_loggerFactory))),
                    new RootScopeLifetime(),
                    InstanceSharing.Shared,
                    InstanceOwnership.OwnedByLifetimeScope,
                    new[] { service },
                    new Dictionary<string, object>());
            }
        }

        public bool IsAdapterForIndividualComponents { get; } = false;
    }
}
