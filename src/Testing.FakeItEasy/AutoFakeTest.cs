using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using FakeItEasy.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A base class with AutoFake wired in for Autofac
    /// </summary>
    public abstract class AutoFakeTest : LoggerTest
    {
        private readonly Lazy<(AutoFake autoFake, IContainer container, IServiceProvider serviceProvider)> _autoFake;
        private IServiceCollection? _serviceCollection = new ServiceCollection();

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoFake AutoFake => _autoFake.Value.autoFake;

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoFake Fake => _autoFake.Value.autoFake;

        /// <summary>
        /// The Autofac container
        /// </summary>
        protected IContainer Container => _autoFake.Value.container;

        /// <summary>
        /// The Service Provider
        /// </summary>
        protected IServiceProvider ServiceProvider => _autoFake.Value.serviceProvider;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null
        )
            : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger) { }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            LogLevel minLevel,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null
        )
            : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger) { }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            LogEventLevel minLevel,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null
        )
            : base(outputHelper, minLevel, logFormat, configureLogger) => _autoFake =
            new Lazy<(AutoFake autoFake, IContainer container, IServiceProvider serviceProvider)>(
                () =>
                {
                    var af = new AutoFake(configureAction: ConfigureContainerBuilder);
                    return (
                        af,
                        A.Fake<IContainer>(x => x.Wrapping(af.Container)),
                        A.Fake<IServiceProvider>(x => x.Wrapping(new AutofacServiceProvider(af.Container)))
                    );
                }
            );
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

        private void ConfigureContainerBuilder(ContainerBuilder cb)
        {
            cb.RegisterSource<RemoveProxyFromEnumerableRegistrationSource>();
            cb.Populate(_serviceCollection);
            cb.RegisterSource(new LoggingRegistrationSource(LoggerFactory, Logger, SerilogLogger));
            BuildContainer(cb);
        }

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context)
            => Populate(context.configuration, context.serviceCollection);

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            Configuration = configuration;
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// A method that allows you to override and update the behavior of building the container
        /// </summary>
        protected virtual void BuildContainer(ContainerBuilder cb) { }

        /// <summary>
        /// Control the way that the serilog logger factory is created.
        /// </summary>
        protected override ILoggerFactory CreateLoggerFactory(
            Serilog.ILogger logger,
            LoggerProviderCollection loggerProviderCollection
        )
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var factory =
                new FakeItEasyLoggerFactory(new SerilogLoggerFactory(logger, false, loggerProviderCollection));
#pragma warning restore CA2000 // Dispose objects before losing scope
            return A.Fake<ILoggerFactory>(l => l.Wrapping(factory));
        }
    }
}