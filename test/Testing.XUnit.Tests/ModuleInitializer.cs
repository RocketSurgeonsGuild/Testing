using System.Runtime.CompilerServices;
using DiffEngine;
using FakeItEasy.Core;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyGeneratorTextContext.Initialize(
            DiagnosticSeverity.Warning,
            Customizers.IncludeInputs,
            Customizers.IncludeParseOptions,
            Customizers.IncludeGlobalOptions,
            Customizers.IncludeFileOptions,
            Customizers.IncludeReferences
        );

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
