//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/AutoFixture.cs
using Goony.Goo.Goo.Tests;
using Goony.Goo.Goo.Tests;
using NSubstitute;

namespace Goony.Goo.Goo.Tests
{
    internal partial sealed class AuthenticatorFixture : ITestFixtureBuilder
    {
        public static implicit operator Authenticator(AuthenticatorFixture fixture) => fixture.Build();
        public AuthenticatorFixture WithClient(IAuthenticationClient authenticationClient) => this.With(ref _authenticationClient, authenticationClient);
        public AuthenticatorFixture WithStorage(ISecureStorage secureStorage) => this.With(ref _secureStorage, secureStorage);
        public AuthenticatorFixture WithLogger(ILogger logger) => this.With(ref _logger, logger);
        private IAuthenticationClient _authenticationClient = Substitute.For<IAuthenticationClient>();
        private ISecureStorage _secureStorage = Substitute.For<ISecureStorage>();
        private ILogger _logger = Substitute.For<ILogger>();
    }
}