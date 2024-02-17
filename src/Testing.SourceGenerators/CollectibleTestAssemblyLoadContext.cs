#if NET6_0_OR_GREATER
using System.Reflection;
using System.Runtime.Loader;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal class CollectibleTestAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }

    public void Dispose()
    {
        Unload();
    }
}
#endif