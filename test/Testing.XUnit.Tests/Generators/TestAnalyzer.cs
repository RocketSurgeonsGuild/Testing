using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class TestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _descriptor = new(
        #pragma warning disable RS2008
        "TEST0001",
        #pragma warning restore RS2008
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true,
        customTags: []
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray<DiagnosticDescriptor>.Empty.Add(_descriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationAction(
            c => { c.ReportDiagnostic(Diagnostic.Create(_descriptor, null)); }
        );
    }
}