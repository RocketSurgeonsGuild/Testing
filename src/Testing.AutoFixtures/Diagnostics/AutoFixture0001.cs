using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoFixture0001 : DiagnosticAnalyzer
{
    /// <summary>
    ///     Diagnostic for unsupported classes with no constructors.
    /// </summary>
    public static DiagnosticDescriptor Descriptor = new(
        nameof(AutoFixture0001),
        "classes without constructors are currently not supported.",
        "",
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
                        if (context.Node is not ClassDeclarationSyntax classDeclaration)
                        {
                            return;
                        }
                        if (!classDeclaration.AttributeLists.Any(listSyntax => listSyntax.Attributes.Any(syntax => syntax.IsAutoFixtureAttribute())))
                        {
                            return;
                        }

                        var constructors = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().ToImmutableList();

                        if (constructors.All(constructorDeclarationSyntax =>
                                                 constructorDeclarationSyntax.ParameterList.Parameters.All(parameterSyntax =>
                                                     parameterSyntax.AttributeLists.Count == 0
                                                 )
                            ))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.GetLocation()));
                        }
                    },
            syntaxKinds: SyntaxKind.ClassDeclaration
        );
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];
}
