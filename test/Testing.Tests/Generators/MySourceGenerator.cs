using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[Generator]
public class MySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(z => z.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }

    public void Execute(GeneratorExecutionContext context) { }
}

[Generator]
public class MyInputLikeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(z => z.AddSource("Input0_ouput.g.cs", "public class GeneratorTest { }"));
    }

    public void Execute(GeneratorExecutionContext context) { }
}
