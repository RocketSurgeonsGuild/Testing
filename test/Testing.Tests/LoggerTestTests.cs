using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class LoggerTestTests : LoggerTest
    {
        public LoggerTestTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        class Impl : LoggerTest
        {
            public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
                Logger.LogError("abcd");
                Logger.LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Create_Usable_Logger()
        {
            var helper = A.Fake<ITestOutputHelper>();
            var test = new Impl(helper);
            A.CallTo(() => helper.WriteLine(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_Create_A_Log_Stream()
        {
            Logger.LogInformation("this is a test 1");

            using var listener = CaptureLogs(out var logs);
            using (listener)
            {
                Logger.LogInformation("this is a test 2");
            }

            Logger.LogInformation("this is a test 3");
            logs.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Create_A_Log_Filter_Stream()
        {
            Logger.LogInformation("this is a test 1");

            using var listener = CaptureLogs(log => log.Level >= LogEventLevel.Warning, out var logs);
            using (listener)
            {
                Logger.LogInformation("this is a test 2");
                Logger.LogWarning("this is a test 2");
                Logger.LogError("this is a test 2");
                Logger.LogCritical("this is a test 2");
            }

            Logger.LogInformation("this is a test 3");
            logs.Should().HaveCount(3);
        }

        [Theory]
        [ClassData(typeof(LoggerTheoryData))]
        public void Should_Support_Theory_Data(IEnumerable<string> messages, int count)
        {
            using var listener = CaptureLogs(out var logs);
            foreach (var item in messages)
            Logger.LogInformation(item);
            logs.Should().HaveCount(count);
        }

        public class LoggerTheoryData : TheoryData<(IEnumerable<string>, int)>
        {
            protected override IEnumerable<(IEnumerable<string>, int)> GetData()
            {
                yield return (new[] { "1", "2", "3" }, 3);
                yield return (new[] { "1", "2" }, 2);
                yield return (new[] { "1" }, 1);
            }
        }
    }
}
