using System;
using System.Diagnostics.CodeAnalysis;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
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
    /// A base class with AutoFake wired in for DryIoc
    /// </summary>
    public abstract class AutoMockTest : LoggerTest
    {
        private static readonly IConfiguration ReadOnlyConfiguration = new ConfigurationBuilder().Build();
        private readonly MockBehavior _mockBehavior;
        private AutoMock? _autoMock;
        private bool _building;

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration Configuration => Container.IsRegistered<IConfiguration>() ? Container.GetService<IConfiguration>() : ReadOnlyConfiguration;

        /// <summary>
        /// The AutoMock instance
        /// </summary>
        protected AutoMock AutoMock => _autoMock ??= Rebuild();

        /// <summary>
        /// The DryIoc container
        /// </summary>
        protected IContainer Container
        {
            get => AutoMock.Container;
            private set => _autoMock = Rebuild(value);
        }

        /// <summary>
        /// Force the container to rebuild from scratch
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected AutoMock Rebuild(IContainer? container = null)
        {
            if (_building) throw new ApplicationException($"Unable to access {nameof(AutoMock)} while the container is being constructed!");
            _building = true;
            var autoFake = new AutoMock(new MockRepository(_mockBehavior), configureAction: ConfigureContainer, container: container);
            _building = false;
            return autoFake;
        }

        /// <summary>
        /// The Service Provider
        /// </summary>
        protected IServiceProvider ServiceProvider => AutoMock.Container;

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
        protected AutoMockTest(
            ITestOutputHelper outputHelper,
            LogEventLevel minLevel,
            MockBehavior mockBehavior = MockBehavior.Default,
            string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
            Action<LoggerConfiguration>? configureLogger = null
        )
            : base(outputHelper, minLevel, logFormat, configureLogger)
        {
            _mockBehavior = mockBehavior;
        }
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

        private IContainer ConfigureContainer(IContainer container)
        {
            container.RegisterInstance(LoggerFactory);
            container.RegisterInstance(Logger);
            container.RegisterInstance(SerilogLogger);
            return BuildContainer(container).WithDependencyInjectionAdapter();
        }

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        [ExcludeFromCodeCoverage]
        [Obsolete("This method is obsolete you can use the overload with IServiceCollection or IContainer instead.")]
        protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context) =>
            Populate(context.configuration, context.serviceCollection);

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        [ExcludeFromCodeCoverage]
        [Obsolete("This method is obsolete you can use the overload with IServiceCollection or IContainer instead.")]
        protected void Populate(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            Container.Populate(serviceCollection);
            Container.UseInstance(configuration);
        }

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate(IServiceCollection serviceCollection)
        {
            Container.Populate(serviceCollection);
        }

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate(IContainer container) => Container = container;

        /// <summary>
        /// A method that allows you to override and update the behavior of building the container
        /// </summary>
        protected virtual IContainer BuildContainer(IContainer container) => container;
    }
}
