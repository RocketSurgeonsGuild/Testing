using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A base class with AutoFake wired in for Autofac
    /// </summary>
    public abstract class AutoMockTest : LoggerTest
    {
        private readonly Lazy<(AutoMock autoMock, IContainer container, AutofacServiceProvider serviceProvider)> _autoMoq;
        private IServiceCollection? _serviceCollection = new ServiceCollection();

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoMock instance
        /// </summary>
        protected AutoMock AutoMock => _autoMoq.Value.autoMock;

        /// <summary>
        /// The AutoMock instance
        /// </summary>
        protected AutoMock Moq => _autoMoq.Value.autoMock;

        /// <summary>
        /// The Autofac container
        /// </summary>
        protected IContainer Container => _autoMoq.Value.container;

        /// <summary>
        /// The Service Provider
        /// </summary>
        protected AutofacServiceProvider ServiceProvider => _autoMoq.Value.serviceProvider;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="mockBehavior"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoMockTest(ITestOutputHelper outputHelper, MockBehavior mockBehavior = MockBehavior.Default, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LogEventLevel.Information, mockBehavior, logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="mockBehavior"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoMockTest(ITestOutputHelper outputHelper, LogLevel minLevel, MockBehavior mockBehavior = MockBehavior.Default, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), mockBehavior, logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="mockBehavior"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        protected AutoMockTest(ITestOutputHelper outputHelper, LogEventLevel minLevel, MockBehavior mockBehavior = MockBehavior.Default, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : base(outputHelper, minLevel, logFormat, configureLogger)
            => _autoMoq = new Lazy<(AutoMock autoMock, IContainer container, AutofacServiceProvider serviceProvider)>(() =>
                {
                    var af = AutoMock.GetFromRepository(new MockRepository(mockBehavior), SetupContainer);
                    return (af, af.Container, new AutofacServiceProvider(af.Container));
                });
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context) =>
            Populate(context.configuration, context.serviceCollection);

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
        protected virtual void BuildContainer(ContainerBuilder cb)
        {
        }

        private void SetupContainer(ContainerBuilder cb)
        {
            cb.Populate(_serviceCollection);
            cb.RegisterSource(new LoggingRegistrationSource(LoggerFactory, Logger, SerilogLogger));
            BuildContainer(cb);
            cb.RegisterSource(new RemoveMockFromEnumerableRegistrationSource());
        }
    }
}
