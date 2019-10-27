using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
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
        private readonly Lazy<AutoFake> _autoFake;
        private IServiceCollection? _serviceCollection;

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration? Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoFake AutoFake => _autoFake.Value;

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoFake Fake => _autoFake.Value;

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(ITestOutputHelper outputHelper, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null) : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(ITestOutputHelper outputHelper, LogLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null) : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoFakeTest(ITestOutputHelper outputHelper, LogEventLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null) : base(outputHelper, minLevel, logFormat, configureLogger)
        {
            _autoFake = new Lazy<AutoFake>(() =>
            {
                var cb = new ContainerBuilder();
                SetupContainer(cb);
                var af = new AutoFake(builder: cb);
                af.Container.ComponentRegistry.AddRegistrationSource(new LoggingRegistrationSource(LoggerFactory, Logger, SerilogLogger));
                return af;
            });
        }

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context)
        {
            Configuration = context.configuration;
            _serviceCollection = context.serviceCollection;
        }

        /// <summary>
        /// A method that allows you to override and update the behavior of building the container
        /// </summary>
        protected virtual void BuildContainer(ContainerBuilder cb)
        {
        }

        /// <summary>
        /// Control the way that the serilog logger factory is created.
        /// </summary>
        protected override ILoggerFactory CreateLoggerFactory(Serilog.ILogger logger, LoggerProviderCollection loggerProviderCollection)
        {
            var factory = new FakeItEasyLoggerFactory(new SerilogLoggerFactory(logger, false, loggerProviderCollection));
            return A.Fake<ILoggerFactory>(l => l.Wrapping(factory));
        }

        private void SetupContainer(ContainerBuilder cb)
        {
            if (_serviceCollection != null)
            {
                cb.Populate(_serviceCollection);
            }
            BuildContainer(cb);
        }
    }
}
