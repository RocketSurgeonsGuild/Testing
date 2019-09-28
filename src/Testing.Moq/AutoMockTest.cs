using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A base class with AutoFake wired in for Autofac
    /// </summary>
    public abstract class AutoMockTest : LoggerTest
    {
        private readonly Lazy<AutoMock> _autoMoq;
        private IServiceCollection? _serviceCollection;

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration? Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoMock AutoMock => _autoMoq.Value;

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoMock Moq => _autoMoq.Value;

        /// <summary>
        /// Create the auto test class
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="mockBehavior"></param>
        /// <param name="minLevel"></param>
        protected AutoMockTest(ITestOutputHelper outputHelper, MockBehavior mockBehavior = MockBehavior.Default, LogLevel minLevel = LogLevel.Information) : base(outputHelper, minLevel, new LoggerFactory())
        {
            _autoMoq = new Lazy<AutoMock>(() =>
            {
                var af = AutoMock.GetFromRepository(new MockRepository(mockBehavior), SetupContainer);
                af.Container.ComponentRegistry.AddRegistrationSource(new LoggingRegistrationSource(LoggerFactory, Logger));
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
