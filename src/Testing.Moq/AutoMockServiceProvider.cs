using System;
using System.Reflection;
using Autofac.Core;
using Autofac.Extras.Moq;

namespace Rocket.Surgery.Extensions.Testing
{
    class AutoMockServiceProvider : IServiceProvider
    {
        private readonly AutoMock _autoMock;
        private static readonly MethodInfo CreateMethod = typeof(AutoMock).GetMethod(nameof(AutoMock.Create));
        private static readonly object[] _parameters = { Array.Empty<Parameter>() };

        public AutoMockServiceProvider(AutoMock autoMock) => _autoMock = autoMock;

        public object GetService(Type serviceType)
            => serviceType == typeof(IServiceProvider) ? this : CreateMethod.MakeGenericMethod(serviceType).Invoke(_autoMock, _parameters);
    }
}