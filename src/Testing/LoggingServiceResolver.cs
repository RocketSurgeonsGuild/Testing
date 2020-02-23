using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DryIoc;
using Microsoft.Extensions.Logging;
using IMsftLogger = Microsoft.Extensions.Logging.ILogger;
using ISeriLogger = Serilog.ILogger;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace Rocket.Surgery.Extensions.Testing
{
    internal static class LoggingServiceResolver
    {
        public static IContainer RegisterLoggers(
            this IContainer container,
            ILoggerFactory loggerFactory,
            IMsftLogger defaultLogger,
            ISeriLogger serilogLogger
        )
        {
            container.RegisterInstance(loggerFactory);
            container.RegisterInstance(defaultLogger);
            container.RegisterInstance(serilogLogger);
            return container;
        }

        // private readonly ILoggerFactory _loggerFactory;
        // private readonly ISeriLogger _serilogLogger;
        // private readonly IMsftLogger _defaultLogger;
        //
        // public LoggingRegistrationSource(ILoggerFactory loggerFactory, IMsftLogger defaultLogger, ISeriLogger serilogLogger)
        // {
        //     _loggerFactory = loggerFactory;
        //     _serilogLogger = serilogLogger;
        //     _defaultLogger = defaultLogger;
        // }
        //
        // public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        // {
        //     if (!(service is TypedService typedService)) yield break;
        //     if (typedService.ServiceType.IsClosedTypeOf(typeof(ILogger<>)))
        //     {
        //         var registration = RegistrationBuilder.ForType(
        //                 typeof(Logger<>).MakeGenericType(typedService.ServiceType.GetGenericArguments()[0]))
        //             .As(typedService.ServiceType);
        //         yield return registration.CreateRegistration();
        //     }
        //
        //     if (typedService.ServiceType == typeof(IMsftLogger))
        //     {
        //         yield return new ComponentRegistration(
        //             Guid.NewGuid(),
        //             new ProvidedInstanceActivator(_defaultLogger),
        //             new RootScopeLifetime(),
        //             InstanceSharing.Shared,
        //             InstanceOwnership.OwnedByLifetimeScope,
        //             new[] { service },
        //             new Dictionary<string, object>());
        //     }
        //
        //     if (typedService.ServiceType == typeof(ISeriLogger))
        //     {
        //         yield return new ComponentRegistration(
        //             Guid.NewGuid(),
        //             new ProvidedInstanceActivator(_serilogLogger),
        //             new RootScopeLifetime(),
        //             InstanceSharing.Shared,
        //             InstanceOwnership.OwnedByLifetimeScope,
        //             new[] { service },
        //             new Dictionary<string, object>());
        //     }
        //
        //     if (typedService.ServiceType == typeof(ILoggerFactory))
        //     {
        //         yield return new ComponentRegistration(
        //             Guid.NewGuid(),
        //             new ProvidedInstanceActivator(_loggerFactory),
        //             new RootScopeLifetime(),
        //             InstanceSharing.Shared,
        //             InstanceOwnership.OwnedByLifetimeScope,
        //             new[] { service },
        //             new Dictionary<string, object>());
        //     }
        // }
        //
        // public bool IsAdapterForIndividualComponents { get; } = false;
    }
}