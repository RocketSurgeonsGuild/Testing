//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/UserAccount.AutoFixture.g.cs
using System;
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class UserAccountFixture : AutoFixtureBase<UserAccountFixture>
    {
        public static implicit operator UserAccount(UserAccountFixture fixture) => fixture.Build();
        public UserAccountFixture WithUserId(System.String UserId) => With(ref _UserId, UserId);
        public UserAccountFixture WithLastLogin(System.String LastLogin) => With(ref _LastLogin, LastLogin);
        public UserAccountFixture WithExpiringRefresh(System.String ExpiringRefresh) => With(ref _ExpiringRefresh, ExpiringRefresh);
        private UserAccount Build() => new UserAccount(_UserId, _LastLogin, _ExpiringRefresh);
        private System.String _UserId = default;
        private System.String _LastLogin = default;
        private System.String _ExpiringRefresh = default;
    }
}