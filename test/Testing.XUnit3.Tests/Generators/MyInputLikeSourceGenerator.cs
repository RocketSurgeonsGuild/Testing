using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Generators;

[Generator]
public class MyInputLikeSourceGenerator : IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context) { }
    public void Initialize(IncrementalGeneratorInitializationContext context) => context.RegisterPostInitializationOutput(z => z.AddSource("Input0_ouput.g.cs", "public class GeneratorTest { }"));
}
