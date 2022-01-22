using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests;

public class XUnitExtensionsTests : LoggerTest
{
    private readonly ITestOutputHelper _outputHelper;

    public XUnitExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void GetTestTest()
    {
        var test = _outputHelper.GetTest();
        test.Should().NotBeNull();
        test.DisplayName.Should().EndWith("GetTestTest");
    }
}
