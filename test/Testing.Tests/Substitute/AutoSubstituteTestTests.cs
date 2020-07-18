using System;
using DryIoc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Xunit;
using Xunit.Abstractions;
using Arg = NSubstitute.Arg;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoSubstituteTestTests : AutoSubstituteTest
    {
        public AutoSubstituteTestTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        class Impl : AutoSubstituteTest
        {
            public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
                Logger.LogError("abcd");
                Logger.LogError("abcd {something}", "somevalue");
            }
        }

        class DoubleAccess : AutoSubstituteTest
        {
            public DoubleAccess(ITestOutputHelper outputHelper) : base(outputHelper)
            {
            }

            protected override IContainer BuildContainer(IContainer container)
            {
                // invalid do not touch ServiceProvider or Container while constructing the container....
                return Container.GetRequiredService<IContainer>();
            }

            public IContainer Self => Container;
        }

        [Fact]
        public void Should_Create_Usable_Logger()
        {
            var test = AutoSubstitute.Resolve<Impl>();
            AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
        }

        class LoggerImpl : AutoSubstituteTest
        {
            public LoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper) { }

            public void Write()
            {
                AutoSubstitute.Resolve<ILogger>().LogError("abcd");
                AutoSubstitute.Resolve<ILogger>().LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Inject_Logger()
        {
            var test = AutoSubstitute.Resolve<LoggerImpl>();
            test.Write();
            AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
        }

        class LoggerFactoryImpl : AutoSubstituteTest
        {
            public LoggerFactoryImpl(ITestOutputHelper outputHelper) : base(outputHelper) { }

            public void Write()
            {
                AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
                AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Inject_LoggerFactory()
        {
            var test = AutoSubstitute.Resolve<LoggerFactoryImpl>();
            test.Write();
            AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
        }

        public class GenericLoggerImpl : AutoSubstituteTest
        {
            private ITestOutputHelper _otherHelper;

            public GenericLoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
                this._otherHelper = outputHelper;
            }

            public void Write()
            {
                AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
                AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Inject_GenericLogger()
        {
            var test = AutoSubstitute.Resolve<GenericLoggerImpl>();
            test.Write();
            var testOutputHelper = AutoSubstitute.Resolve<ITestOutputHelper>();
            testOutputHelper.Received().WriteLine(Arg.Any<string>());
        }

        [Fact]
        public void Should_Provide_Values()
        {
            var item = AutoSubstitute.Provide(new MyItem());
            ServiceProvider.GetRequiredService<MyItem>().Should().BeSameAs(item);
        }

        [Fact]
        public void Should_Return_Self_For_ServiceProvider()
        {
            ServiceProvider.GetRequiredService<IServiceProvider>().Should().Be(ServiceProvider);
        }

        [Fact]
        public void Should_Not_Fake_Optional_Parameters()
        {
            AutoSubstitute.Resolve<Optional>().Item.Should().BeNull();
        }

        [Fact]
        public void Should_Populate_Optional_Parameters_When_Provided()
        {
            AutoSubstitute.Provide<IItem>(new MyItem());
            AutoSubstitute.
                Resolve<Optional>()
               .Item
               .Should()
               .NotBeNull();
        }

        [Fact]
        public void Should_Fail_If_Container_Is_Touched_When_Building()
        {
            var access = AutoSubstitute.Resolve<DoubleAccess>();
            Action a = () => access.Self.Resolve<IContainer>();
            a.Should().Throw<ApplicationException>();
        }

        class MyItem : IItem { }

        public interface IItem
        {
        }

        class Optional
        {
            public IItem? Item { get; }

            public Optional(IItem? item = null) => Item = item;
        }
    }
}