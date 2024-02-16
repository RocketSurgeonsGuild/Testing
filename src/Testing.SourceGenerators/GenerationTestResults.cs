using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

[PublicAPI]
public record GenerationTestResults
(
    CSharpCompilation InputCompilation,
    ImmutableArray<Diagnostic> InputDiagnostics,
    ImmutableArray<SyntaxTree> InputSyntaxTrees,
    ImmutableArray<AdditionalText> InputAdditionalTexts,
    CSharpParseOptions ParseOptions,
    ImmutableDictionary<string, string> GlobalOptions,
    ImmutableDictionary<string, ImmutableDictionary<string, string>> FileOptions,
    ImmutableDictionary<Type, GenerationTestResult> Results,
    CSharpCompilation FinalCompilation,
    ImmutableArray<Diagnostic> FinalDiagnostics,
    Assembly? Assembly)
{
    public static implicit operator CSharpCompilation(GenerationTestResults results)
    {
        return results.FinalCompilation;
    }

    public static implicit operator Assembly?(GenerationTestResults results)
    {
        return results.Assembly;
    }

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
        if (FinalDiagnostics.Any(x => x.Severity >= severity))
        {
            throw new InvalidOperationException("Compilation failed");
        }

        foreach (var result in Results.Values)
        {
            result.EnsureDiagnosticSeverity(severity);
        }
    }

    public void AssertCompilationWasSuccessful()
    {
        if (FinalDiagnostics.Any(x => x.Severity >= DiagnosticSeverity.Warning))
        {
            throw new InvalidOperationException("Compilation failed");
        }

        foreach (var result in Results.Values)
        {
            result.EnsureDiagnosticSeverity();
        }
    }

    public void AssertGenerationWasSuccessful()
    {
        foreach (var item in Results.Values)
        {
            if (item.Compilation is null)
            {
                throw new InvalidOperationException("Compilation must not be null");
            }

            item.EnsureDiagnosticSeverity();
        }
    }
}