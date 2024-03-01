using FluentAssertions;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests;

public class XUnitExtensionsTests : LoggerTest
{
    [Fact]
    public void GetTestTest()
    {
        var test = _outputHelper.GetTest();
        test.Should().NotBeNull();
        test.DisplayName.Should().EndWith("GetTestTest");
    }

    [Fact]
    public void GetTestUniqueId()
    {
        XUnitExtensions.GetTestUniqueId(this).Should().Be("7e06173264149bca938e4d763000308e6fbf9940");
    }

    [Fact]
    public void GetTestHashId()
    {
        XUnitExtensions.GetTestHashId(this).Should().Be(-213276095);
    }

    public XUnitExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private readonly ITestOutputHelper _outputHelper;
}