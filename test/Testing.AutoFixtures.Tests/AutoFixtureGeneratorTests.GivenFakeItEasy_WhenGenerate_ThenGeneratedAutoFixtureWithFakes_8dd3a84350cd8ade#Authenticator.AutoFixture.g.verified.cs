//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Authenticator.AutoFixture.g.cs
using FakeItEasy;
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using Microsoft.Extensions.Logging;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class AuthenticatorFixture : AutoFixtureBase
    {
        public static implicit operator Authenticator(AuthenticatorFixture fixture) => fixture.Build();
        public AuthenticatorFixture WithClient(Goony.Goo.Goo.IAuthenticationClient authenticationClient) => this.With(ref _authenticationClient, authenticationClient);
        public AuthenticatorFixture WithStorage(Goony.Goo.Goo.ISecureStorage secureStorage) => this.With(ref _secureStorage, secureStorage);
        public AuthenticatorFixture WithLogger(Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> logger) => this.With(ref _logger, logger);
        private Authenticator Build() => new Authenticator(_authenticationClient, _secureStorage, _logger);
        private Goony.Goo.Goo.IAuthenticationClient _authenticationClient = A.Fake<Goony.Goo.Goo.IAuthenticationClient>();
        private Goony.Goo.Goo.ISecureStorage _secureStorage = A.Fake<Goony.Goo.Goo.ISecureStorage>();
        private Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> _logger = A.Fake<Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator>>();
    }
}