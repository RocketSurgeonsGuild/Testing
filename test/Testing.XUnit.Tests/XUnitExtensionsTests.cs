using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class XUnitExtensionsTests(ITestOutputHelper outputHelper) : LoggerTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Fact]
    public void GetTestTest()
    {
        var test = outputHelper.GetTest();
        _ = test.ShouldNotBeNull();
        test.DisplayName.ShouldEndWith("GetTestTest");
    }

    [Fact]
    public void GetTestUniqueId() => outputHelper.GetTestUniqueId().ShouldBe("66d256d940db996ce53528d6d76407726a8eaa10");

    [Fact]
    public void GetTestHashId() => outputHelper.GetTestHashId().ShouldBe(-485969091);
}
