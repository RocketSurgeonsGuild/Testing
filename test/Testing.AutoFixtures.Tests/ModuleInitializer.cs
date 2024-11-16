using System.Runtime.CompilerServices;
using DiffEngine;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using VerifyTests.DiffPlex;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyGeneratorTextContext.Initialize();
        VerifyDiffPlex.Initialize(OutputType.Minimal);
        DiffRunner.Disabled = true;

        DerivePathInfo(
            (sourceFile, projectDirectory, type, method) =>
            {
                static string GetTypeName(Type type)
                {
                    return type.IsNested ? $"{type.ReflectedType!.Name}.{type.Name}" : type.Name;
                }

                var typeName = GetTypeName(type);

                var path = Path.Combine(Path.GetDirectoryName(sourceFile)!, "snapshots");
                return new(path, typeName, method.Name);
            }
        );
    }
}
