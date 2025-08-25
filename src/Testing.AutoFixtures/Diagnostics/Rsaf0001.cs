using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Rsaf0001 : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override void Initialize(AnalysisContext analysisContext)
    {
        analysisContext.EnableConcurrentExecution();
        analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        analysisContext.RegisterSyntaxNodeAction(
            context =>
            {
                if (context.Node is not ClassDeclarationSyntax classDeclaration)
                {
                    return;
                }

                if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)))
                {
                    return;
                }

                if (classDeclaration.IsAutoFixture())
                {
                    return;
                }

                if (classDeclaration.AttributeLists.Any(listSyntax => listSyntax.Attributes.Any(syntax => syntax.HasAutoFixtureAttribute())))
                {
                    return;
                }

                if (classDeclaration.Identifier.Text.EndsWith("Fixture"))
                {
                    return;
                }

                var constructors = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().ToImmutableList();

                if (!constructors.All(constructorDeclarationSyntax => constructorDeclarationSyntax.ParameterList.Parameters.Count == 0))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.GetLocation()));
            },
            SyntaxKind.ClassDeclaration
        );
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    /// <summary>
    ///     Diagnostic for unsupported classes with no constructors.
    /// </summary>
    public static DiagnosticDescriptor Descriptor = new(
        nameof(Rsaf0001),
        "classes without constructors are currently not supported",
        "",
        "Support",
        DiagnosticSeverity.Info,
        true
    );
}
