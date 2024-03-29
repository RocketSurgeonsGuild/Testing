using FakeItEasy;
using NSubstitute;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class AutoFixtureGeneratorData : AutoFixtureSourceData
{
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[]
            {
                DefaultBuilder()
                   .AddReferences(typeof(Fake))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
                ClassSource,
                AttributedFixtureSource,
            },
            new object[]
            {
                DefaultBuilder()
                   .AddReferences(typeof(Substitute))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
                ClassSource,
                AttributedFixtureSource,
            },
        };

    private const string AttributedFixtureSource = @"using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;


namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Authenticator))]
    internal sealed partial class AuthenticatorFixture { }
}
";

    private const string ClassSource = @"using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Goony.Goo.Goo
{
    internal class Authenticator
    {
        public Authenticator(IAuthenticationClient authenticationClient,
            ISecureStorage secureStorage,
            ILogger<Authenticator> logger) {}
    }
    internal interface ISecureStorage {}
    internal interface IAuthenticationClient {}
}
";

    private const string AttributeOnClassSource = @"using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture]
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
