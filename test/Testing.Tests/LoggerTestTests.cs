using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using Microsoft.Extensions.Logging;
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
    }
}
