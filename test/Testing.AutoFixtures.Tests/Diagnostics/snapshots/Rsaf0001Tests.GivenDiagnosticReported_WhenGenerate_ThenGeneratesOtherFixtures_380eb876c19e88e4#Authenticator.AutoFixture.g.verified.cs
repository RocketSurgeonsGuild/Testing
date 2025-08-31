//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Authenticator.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class AuthenticatorFixture : AutoFixtureBase<AuthenticatorFixture>
    {
        public static implicit operator Authenticator(AuthenticatorFixture fixture) => fixture.Build();
        public AuthenticatorFixture WithClient(Goony.Goo.Goo.IAuthenticationClient authenticationClient) => With(ref _authenticationClient, authenticationClient);
        public AuthenticatorFixture WithStorage(Goony.Goo.Goo.ISecureStorage secureStorage) => With(ref _secureStorage, secureStorage);
        public AuthenticatorFixture WithLogger(Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> logger) => With(ref _logger, logger);
        private Authenticator Build() => new Authenticator(_authenticationClient, _secureStorage, _logger);
        private Goony.Goo.Goo.IAuthenticationClient _authenticationClient = Substitute.For<Goony.Goo.Goo.IAuthenticationClient>();
        private Goony.Goo.Goo.ISecureStorage _secureStorage = Substitute.For<Goony.Goo.Goo.ISecureStorage>();
        private Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> _logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator>>();
    }
}