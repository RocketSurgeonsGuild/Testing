namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public static class AutoFixtureGeneratorData
{
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { BasicSource }
        };

    private const string BasicSource = @"using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixture;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Authenticator))]
    internal sealed partial class AuthenticatorFixture : ITestFixtureBuilder { }
    internal class Authenticator 
    {
        public Authenticator(IAuthenticationClient authenticationClient,
            ISecureStorage secureStorage,
            ILogger<Authenticator> logger) {}
    }
    internal interface ISecureStorage {}
    internal interface IAuthenticationClient {}
}";
}