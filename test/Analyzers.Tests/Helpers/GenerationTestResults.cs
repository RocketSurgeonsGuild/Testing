using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Analyzers.Tests.Helpers
{
    public record GenerationTestResults(
        CSharpCompilation InputCompilation,
        ImmutableArray<Diagnostic> InputDiagnostics,
        ImmutableArray<SyntaxTree> InputSyntaxTrees,
        ImmutableDictionary<Type, GenerationTestResult> Results
    )
    {
        public bool TryGetResult(Type type, [NotNullWhen(true)] out GenerationTestResult? result)
        {
            return Results.TryGetValue(type, out result);
        }

        public bool TryGetResult<T>([NotNullWhen(true)] out GenerationTestResult? result)
            where T : new()
        {
            return Results.TryGetValue(typeof(T), out result);
        }

        public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            Assert.Empty(InputDiagnostics.Where(x => x.Severity >= severity));
            foreach (var result in Results.Values)
            {
                result.EnsureDiagnostics(severity);
            }
        }

        public void AssertGeneratedAsExpected<T>(string expectedValue, params string[] expectedValues)
            where T : new()
        {
            if (!TryGetResult<T>(out var result))
            {
                Assert.NotNull(result);
                return;
            }

            result.AssertGeneratedAsExpected(expectedValue);
        }
    }
}
