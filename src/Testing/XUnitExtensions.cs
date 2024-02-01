using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Rocket.Surgery.Extensions.Testing;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Xunit;

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

    /// <summary>
    ///     Gets the test from the ITestOutputHelper
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public static ITest GetTest(this IHasTestOutputHelper output)
    {
        return output.TestOutputHelper.GetTest();
    }

    /// <summary>
    ///     Gets a hashed id for the test
    /// </summary>
    /// <remarks>may be unstable depending on your test runner</remarks>
    /// <param name="output"></param>
    /// <returns></returns>
    public static int GetTestHashId(this ITestOutputHelper output)
    {
        // convert the md5 of the test unique id to an int without using GetHashCode()
        using var haser = SHA256.Create();
        haser.ComputeHash(Encoding.UTF8.GetBytes(output.GetTestUniqueId()));
        return BitConverter.ToInt32(haser.Hash, 0);
    }

    /// <summary>
    ///     Gets a hashed id for the test
    /// </summary>
    /// <remarks>may be unstable depending on your test runner</remarks>
    /// <param name="output"></param>
    /// <returns></returns>
    public static int GetTestHashId(this IHasTestOutputHelper output)
    {
        return output.TestOutputHelper.GetTestHashId();
    }

    /// <summary>
    ///     Gets unique id for the test
    /// </summary>
    /// <remarks>may be unstable depending on your test runner</remarks>
    /// <param name="output"></param>
    /// <returns></returns>
    public static string GetTestUniqueId(this ITestOutputHelper output)
    {
        return output.GetTest().TestCase.UniqueID;
    }

    /// <summary>
    ///     Gets unique id for the test
    /// </summary>
    /// <remarks>may be unstable depending on your test runner</remarks>
    /// <param name="output"></param>
    /// <returns></returns>
    public static string GetTestUniqueId(this IHasTestOutputHelper output)
    {
        return output.TestOutputHelper.GetTestUniqueId();
    }
}
