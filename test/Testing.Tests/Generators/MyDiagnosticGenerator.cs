using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[Generator]
public class MyDiagnosticGenerator : IIncrementalGenerator
{
    private DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
        "TEST001",
        "Test",
        "Test",
        "Test",
        DiagnosticSeverity.Error,
        true
    );

    private DiagnosticDescriptor DiagnosticDescriptor2 { get; } = new(
        "TEST002",
        "Test",
        "Test",
        "Test",
        DiagnosticSeverity.Warning,
        true
    );

    private DiagnosticDescriptor DiagnosticDescriptor3 { get; } = new(
        "TEST003",
        "Test",
        "Test",
        "Test",
        DiagnosticSeverity.Info,
        true
    );

    private DiagnosticDescriptor DiagnosticDescriptor4 { get; } = new(
        "TEST004",
        "Test",
        "Test",
        "Test",
        DiagnosticSeverity.Hidden,
        true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.ParseOptionsProvider,
            (productionContext, options) =>
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor2, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor3, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor4, null));
            }
        );
    }
}