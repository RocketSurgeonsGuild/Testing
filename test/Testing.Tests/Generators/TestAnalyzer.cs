using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

class TestAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCompilationAction(
            c =>
            {
                c.ReportDiagnostic(Diagnostic.Create(_descriptor, null));
            });
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray<DiagnosticDescriptor>.Empty.Add(_descriptor);

    private static DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
        "TEST0001",
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true
    );
}