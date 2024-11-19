﻿using FluentAssertions;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

public class XUnitExtensionsTests(ITestOutputHelper outputHelper) : LoggerTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{
    [Fact]
    public void GetTestTest()
    {
        var test = outputHelper.GetTest();
        test.Should().NotBeNull();
        test.DisplayName.Should().EndWith("GetTestTest");
    }

    [Fact]
    public void GetTestUniqueId()
    {
        XUnitExtensions.GetTestUniqueId(outputHelper).Should().Be("66d256d940db996ce53528d6d76407726a8eaa10");
    }

    [Fact]
    public void GetTestHashId()
    {
        XUnitExtensions.GetTestHashId(outputHelper).Should().Be(-485969091);
    }
}