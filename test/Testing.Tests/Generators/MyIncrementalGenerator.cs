using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

public class MyIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(initializationContext => initializationContext.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }
}