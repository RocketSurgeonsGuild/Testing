using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        /// Create the auto test class
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        protected AutoFakeTest(ITestOutputHelper outputHelper, LogLevel minLevel = LogLevel.Information) : base(outputHelper, minLevel, A.Fake<ILoggerFactory>(x => x.Wrapping(new FakeItEasyLoggerFactory())))
        {
            _autoFake = new Lazy<AutoFake>(() =>
            {
                var cb = new ContainerBuilder();
                SetupContainer(cb);
                var af = new AutoFake(builder: cb);
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
