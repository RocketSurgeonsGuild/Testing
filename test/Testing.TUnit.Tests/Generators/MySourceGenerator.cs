using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

[Generator]
public class MySourceGenerator : IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context) { }
    public void Initialize(IncrementalGeneratorInitializationContext context) => context.RegisterPostInitializationOutput(z => z.AddSource("test.g.cs", "public class GeneratorTest { }"));
}
