﻿using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

public class MySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(z => z.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }

    public void Execute(GeneratorExecutionContext context) { }
}