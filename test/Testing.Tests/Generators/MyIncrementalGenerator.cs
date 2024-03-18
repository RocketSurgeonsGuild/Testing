using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[Generator]
public class MyIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(initializationContext => initializationContext.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }
}
[Generator]
public class MyDiagnosticGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.ParseOptionsProvider,
            (productionContext, options) =>
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor2, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor3, null));
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor4, null));
            });
    }

    private DiagnosticDescriptor DiagnosticDescriptor { get; } = new DiagnosticDescriptor("TEST001", "Test", "Test", "Test", DiagnosticSeverity.Error, true);
    private DiagnosticDescriptor DiagnosticDescriptor2 { get; } = new DiagnosticDescriptor("TEST002", "Test", "Test", "Test", DiagnosticSeverity.Warning, true);
    private DiagnosticDescriptor DiagnosticDescriptor3 { get; } = new DiagnosticDescriptor("TEST003", "Test", "Test", "Test", DiagnosticSeverity.Info, true);
    private DiagnosticDescriptor DiagnosticDescriptor4 { get; } = new DiagnosticDescriptor("TEST004", "Test", "Test", "Test", DiagnosticSeverity.Hidden, true);
}
