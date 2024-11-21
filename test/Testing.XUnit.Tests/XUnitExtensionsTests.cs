using FluentAssertions;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

public class XUnitExtensionsTests(ITestOutputHelper outputHelper) : LoggerTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
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
        outputHelper.GetTestUniqueId().Should().Be("66d256d940db996ce53528d6d76407726a8eaa10");
    }

    [Fact]
    public void GetTestHashId()
    {
        outputHelper.GetTestHashId().Should().Be(-485969091);
    }
}
