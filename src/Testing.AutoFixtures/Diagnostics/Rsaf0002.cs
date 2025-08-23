using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Rsaf0002 : DiagnosticAnalyzer
{
    /// <summary>
    ///     Diagnostic for unsupported parameter arrays as constructors.
    /// </summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        nameof(Rsaf0002),
        "params arrays are not currently supported with AutoFixture.",
        "Consider using IEnumerable<T>",
        "Support",
        DiagnosticSeverity.Info,
        true
    );

    /// <inheritdoc />
    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.EnableConcurrentExecution();
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        analysisContext.RegisterSyntaxNodeAction(
            action: context =>
                    {
                        var namedTypeSymbol = GetNamedTypeSymbolDirectly(context);

                        if (namedTypeSymbol == null)
                        {
                            return;
                        }

                        foreach (var location in namedTypeSymbol
                                                .Constructors
                                                .SelectMany(methodSymbol => methodSymbol.Parameters)
                                                .Distinct(ParameterReductionComparer.Default)
                                                .Select(parameterSymbol => new { parameterSymbol, isArrayType = parameterSymbol.Type is IArrayTypeSymbol })
                                                .Select(tuple => new
                                                     {
                                                         tuple.isArrayType,
                                                         tuple.parameterSymbol,
                                                         hasParamsKeyWord = tuple.parameterSymbol.ToDisplayString().Contains("params"),
                                                     }
                                                 )
                                                .Where(tuple => tuple.isArrayType && tuple.hasParamsKeyWord)
                                                .SelectMany(tuple => tuple.parameterSymbol.Locations))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                        }
                    },
            syntaxKinds: SyntaxKind.ClassDeclaration
        );

        INamedTypeSymbol? GetNamedTypeSymbolDirectly(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is TypeDeclarationSyntax typeDeclaration)
            {
                return context
                      .Compilation
                      .GetSemanticModel(typeDeclaration.SyntaxTree)
                      .GetDeclaredSymbol(typeDeclaration);
            }

            // If the containing symbol is a named type, return it
            if (context.ContainingSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return namedTypeSymbol;
            }

            return context.ContainingSymbol?.ContainingType;
        }
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];
}
