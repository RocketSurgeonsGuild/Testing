using System.Reflection;
using System.Runtime.Loader;
#if NET11_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Loads emitted assemblies from memory. On .NET 11+ the assembly also gets a stable <see cref="Assembly.Location" />:
///     a copy of the image under the temp directory, keyed by the <see cref="GeneratorTestContext.Id" /> and written the first time
///     something reads the location.
/// </summary>
internal static class GeneratedAssemblyLocation
{
    public static Assembly LoadFromStream(AssemblyLoadContext assemblyLoadContext, string contextId, byte[] image)
    {
        var assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(image));
#if NET11_0_OR_GREATER
        if (Registered.Value)
        {
            Locations.AddOrUpdate(assembly, new(() => WriteImage(assembly, contextId, image)));
        }
#endif
        return assembly;
    }

#if NET11_0_OR_GREATER
    private static readonly ConditionalWeakTable<Assembly, Lazy<string>> Locations = [];
    private static readonly Lazy<bool> Registered = new(Register);

    private static bool Register()
    {
        try
        {
            AssemblyLoadContext.SetAssemblyLocationOverride(
                static (assembly, location) => location is { Length: 0, } && Locations.TryGetValue(assembly, out var path) ? path.Value : location
            );
            return true;
        }
        catch (InvalidOperationException)
        {
            // The override is process-wide and can only be set once; if the host already owns it, keep the default empty location.
            return false;
        }
    }

    private static string WriteImage(Assembly assembly, string contextId, byte[] image)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "Rocket.Surgery.Extensions.Testing",
            Convert.ToHexStringLower(Convert.FromBase64String(contextId))
        );
        var path = Path.Combine(directory, $"{assembly.GetName().Name}.dll");
        Directory.CreateDirectory(directory);
        // Write to a unique file and move it into place so parallel test runs never see a partial image.
        var temporaryPath = Path.Combine(directory, Path.GetRandomFileName());
        File.WriteAllBytes(temporaryPath, image);
        File.Move(temporaryPath, path, true);
        return path;
    }
#endif
}
