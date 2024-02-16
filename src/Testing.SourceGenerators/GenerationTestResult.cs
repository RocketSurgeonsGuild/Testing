using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

[PublicAPI]
public record GenerationTestResult
(
    CSharpCompilation Compilation,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<SyntaxTree> SyntaxTrees
)
{
    public static string NormalizeToLf(string input)
    {
        return input.Replace(GenerationHelpers.CrLf, GenerationHelpers.Lf);
    }

    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        if (Diagnostics.Any(x => x.Severity >= severity))
        {
            throw new InvalidOperationException("Compilation failed");
        }
    }
}