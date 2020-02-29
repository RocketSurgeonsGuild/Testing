using System;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy;
using FakeItEasy.Creation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using IContainer = DryIoc.IContainer;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A base class with AutoFake wired in for DryIoc
    /// </summary>
    public abstract class AutoFakeTest : LoggerTest
    {
        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoFake AutoFake { get; }

        /// <summary>
        /// The DryIoc container
        /// </summary>
        protected IContainer Container => AutoFake.Container;

        /// <summary>
        /// The Service Provider
        /// </summary>
        protected IServiceProvider ServiceProvider => AutoFake.Container;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <param name="fakeOptionsAction"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null,
            Action<IFakeOptions> fakeOptionsAction = null
        )
            : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger, fakeOptionsAction) { }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <param name="fakeOptionsAction"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            LogLevel minLevel,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null,
            Action<IFakeOptions> fakeOptionsAction = null
        )
            : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger, fakeOptionsAction) { }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <param name="fakeOptionsAction"></param>
        protected AutoFakeTest(
            ITestOutputHelper outputHelper,
            LogEventLevel minLevel,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null,
            Action<IFakeOptions> fakeOptionsAction = null
        )
            : base(outputHelper, minLevel, logFormat, configureLogger) => AutoFake = new AutoFake(configureAction: ConfigureContainer, fakeOptionsAction: fakeOptionsAction);
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

        private IContainer ConfigureContainer(IContainer container)
        {
            container.RegisterInstance(LoggerFactory);
            container.RegisterInstance(Logger);
            container.RegisterInstance(SerilogLogger);
            return BuildContainer(container.WithDependencyInjectionAdapter());
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
            Configuration = new ConfigurationBuilder().AddConfiguration(Configuration).AddConfiguration(configuration).Build();
            Container.Populate(serviceCollection);
        }

        /// <summary>
        /// A method that allows you to override and update the behavior of building the container
        /// </summary>
        protected virtual IContainer BuildContainer(IContainer container) => container;

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