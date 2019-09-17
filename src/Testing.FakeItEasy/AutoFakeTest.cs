using System;
using Autofac;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
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
                BuildContainer(cb);
                var af = new AutoFake(builder: cb);
                af.Container.ComponentRegistry.AddRegistrationSource(new LoggingRegistrationSource(LoggerFactory, Logger));
                return af;
            });
        }

        protected virtual void BuildContainer(ContainerBuilder cb)
        {
        }
    }
}
