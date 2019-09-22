using System;
using Autofac;
using Autofac.Extras.Moq;
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
                var af = AutoMock.GetFromRepository(new MockRepository(mockBehavior), BuildContainer);
                af.Container.ComponentRegistry.AddRegistrationSource(new LoggingRegistrationSource(LoggerFactory, Logger));
                return af;
            });
        }

        protected virtual void BuildContainer(ContainerBuilder cb)
        {
        }
    }
}
