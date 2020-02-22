using System;
using System.Reflection;
using Autofac.Core;
using Autofac.Extras.FakeItEasy;

namespace Rocket.Surgery.Extensions.Testing
{
    class AutoFakeServiceProvider : IServiceProvider
    {
        private readonly AutoFake _autoFake;
        private static readonly MethodInfo ResolveMethod = typeof(AutoFake).GetMethod(nameof(AutoFake.Resolve));
        private static readonly object[] _parameters = { Array.Empty<Parameter>() };

        public AutoFakeServiceProvider(AutoFake autoFake) => _autoFake = autoFake;

        public object GetService(Type serviceType)
            => serviceType == typeof(IServiceProvider) ? this : ResolveMethod.MakeGenericMethod(serviceType).Invoke(_autoFake, _parameters);
    }
}