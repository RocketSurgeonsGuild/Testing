using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

/// <summary>
///     Writes the names of every class in a comment, through a step tracked as "ClassNames".
/// </summary>
[Generator]
public class ClassNamesGenerator : IIncrementalGenerator
{
    public const string StepName = "ClassNames";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var names = context
                   .SyntaxProvider.CreateSyntaxProvider(
                        static (node, _) => node is ClassDeclarationSyntax,
                        static (syntaxContext, _) => ( (ClassDeclarationSyntax)syntaxContext.Node ).Identifier.Text
                    )
                   .WithTrackingName(StepName);
        context.RegisterSourceOutput(
            names.Collect(),
            static (productionContext, list) => productionContext.AddSource("ClassNames.g.cs", "// " + string.Join(",", list.OrderBy(z => z, StringComparer.Ordinal)))
        );
    }
}

/// <summary>
///     Writes the names of the referenced assemblies that start with "Library" in a comment, through a step tracked as "ReferenceNames".
/// </summary>
[Generator]
public class ReferenceNamesGenerator : IIncrementalGenerator
{
    public const string StepName = "ReferenceNames";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var names = context
                   .CompilationProvider.Select(
                        static (compilation, _) => string.Join(
                            ",",
                            compilation
                               .ReferencedAssemblyNames.Select(z => z.Name)
                               .Where(z => z.StartsWith("Library", StringComparison.Ordinal))
                               .OrderBy(z => z, StringComparer.Ordinal)
                        )
                    )
                   .WithTrackingName(StepName);
        context.RegisterSourceOutput(names, static (productionContext, value) => productionContext.AddSource("References.g.cs", "// " + value));
    }
}
