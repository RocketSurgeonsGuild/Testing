#pragma warning disable CA2255
using System.Runtime.CompilerServices;

namespace Rocket.Surgery.Extensions.Testing;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyNSubstitute.Initialize();
    }
}
