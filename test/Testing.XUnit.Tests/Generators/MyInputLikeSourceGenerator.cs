using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Generators;

[Generator]
public class MyInputLikeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(z => z.AddSource("Input0_ouput.g.cs", "public class GeneratorTest { }"));
    }

    public void Execute(GeneratorExecutionContext context) { }
}