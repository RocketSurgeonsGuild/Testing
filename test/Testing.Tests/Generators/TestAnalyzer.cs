using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class TestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _descriptor = new(
        "TEST0001",
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray<DiagnosticDescriptor>.Empty.Add(_descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCompilationAction(
            c => { c.ReportDiagnostic(Diagnostic.Create(_descriptor, null)); }
        );
    }
}
