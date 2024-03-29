using System.Runtime.CompilerServices;
using DiffEngine;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using VerifyTests.DiffPlex;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyGeneratorTextContext.Initialize();
        VerifyDiffPlex.Initialize(OutputType.Minimal);
        DiffRunner.Disabled = true;
    }
}