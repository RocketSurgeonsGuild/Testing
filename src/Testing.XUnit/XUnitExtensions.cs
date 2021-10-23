using System.Reflection;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace xunit;

/// <summary>
///     Helpful XUnit Extensions
/// </summary>
public static class XUnitExtensions
{
    /// <summary>
    ///     Gets the test from the ITestOutputHelper
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public static ITest GetTest(this ITestOutputHelper output)
    {
        var type = output.GetType();
        var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (ITest)testMember.GetValue(output)!;
    }
}