using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

public class XUnitExtensionsTests(ITestOutputHelper outputHelper) : LoggerTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [Fact]
    public void GetTestTest()
    {
        var test = outputHelper.GetTest();
        test.ShouldNotBeNull();
        test.DisplayName.ShouldEndWith("GetTestTest");
    }

    [Fact]
    public void GetTestUniqueId()
    {
        outputHelper.GetTestUniqueId().ShouldBe("66d256d940db996ce53528d6d76407726a8eaa10");
    }

    [Fact]
    public void GetTestHashId()
    {
        outputHelper.GetTestHashId().ShouldBe(-485969091);
    }
}
