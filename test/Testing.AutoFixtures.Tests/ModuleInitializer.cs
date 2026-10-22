using System.Runtime.CompilerServices;
using DiffEngine;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyGeneratorTextContext.Initialize(
            Customizers.ExcludeInputs,
            Customizers.IncludeParseOptions,
            Customizers.IncludeGlobalOptions,
            Customizers.IncludeFileOptions,
            Customizers.IncludeReferences
        );
        VerifierSettings.UseTextDiffFormat(TextDiffFormat.Minimal);
        DiffRunner.Disabled = true;

        DerivePathInfo(
            (sourceFile, projectDirectory, type, method) =>
            {
                static string GetTypeName(Type type) => type.IsNested ? $"{type.ReflectedType!.Name}.{type.Name}" : type.Name;

                var typeName = GetTypeName(type);

                var path = Path.Combine(Path.GetDirectoryName(sourceFile)!, "snapshots");
                return new(path, typeName, method.Name);
            }
        );
    }
}
