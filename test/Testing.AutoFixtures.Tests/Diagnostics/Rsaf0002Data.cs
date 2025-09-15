using FakeItEasy;

using NSubstitute;

using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests.Diagnostics;

internal class Rsaf0002Data : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> DiagnosticReported =>
        [
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(ValidSource, ValidAttributedFixtureSource, InvalidSource, InvalidAttributedFixtureSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(ValidSource, ValidAttributedFixtureSource, InvalidSource, InvalidAttributedFixtureSource)
               .Build(),
        ];

    private const string ValidAttributedFixtureSource = @"using System;
using System.Diagnostics;
using Goony.Goo.Goo;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Authenticator))]
    internal sealed partial class AuthenticatorFixture { }
}
";

    private const string ValidSource = @"using System;
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

    private const string InvalidSource =
        @"namespace Goony.Goo.Goo
{
    public class Stuff
    {
        public string ThingOne { get; set; }
        public string ThingTwo { get; set; }
    }
}";

    private const string InvalidAttributedFixtureSource = @"using System;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Stuff))]
    internal partial class StufFixture
    {
    }
}";
}
