//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/AutoFixture.cs
using NSubstitute;
using Goony.Goo.Goo;
using Microsoft.Extensions.Logging;

namespace Goony.Goo.Goo.Tests
{
    internal partial sealed class AuthenticatorFixture : ITestFixtureBuilder
    {
        public static implicit operator Authenticator(AuthenticatorFixture fixture) => fixture.Build();
        public AuthenticatorFixture WithClient(Goony.Goo.Goo.IAuthenticationClient authenticationClient) => this.With(ref _authenticationClient, authenticationClient);
        public AuthenticatorFixture WithStorage(Goony.Goo.Goo.ISecureStorage secureStorage) => this.With(ref _secureStorage, secureStorage);
        public AuthenticatorFixture WithLogger(Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> logger) => this.With(ref _logger, logger);
        private Goony.Goo.Goo.IAuthenticationClient _authenticationClient = Substitute.For<Goony.Goo.Goo.IAuthenticationClient>();
        private Goony.Goo.Goo.ISecureStorage _secureStorage = Substitute.For<Goony.Goo.Goo.ISecureStorage>();
        private Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator> _logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<Goony.Goo.Goo.Authenticator>>();
    }
}