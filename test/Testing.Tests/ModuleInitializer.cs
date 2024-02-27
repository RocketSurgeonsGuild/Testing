using System.Runtime.CompilerServices;
using DiffEngine;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyGeneratorTextContext.Initialize(true);

        DiffRunner.Disabled = true;
    }
}
