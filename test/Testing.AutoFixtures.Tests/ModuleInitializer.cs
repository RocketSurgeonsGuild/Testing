using System.Runtime.CompilerServices;
using DiffEngine;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyGeneratorTextContext.Initialize(
            includeInputs: false,
            includeOptions: true,
            diagnosticSeverityFilter: DiagnosticSeverity.Warning
        );
        DiffRunner.Disabled = true;
    }
}