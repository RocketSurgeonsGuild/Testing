using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Turns <c>dotnet_diagnostic.&lt;id&gt;.severity</c> and <c>generated_code</c> analyzer config options into compiler options,
///     as the real compiler does for .editorconfig (per file) and .globalconfig (global) files.
/// </summary>
internal sealed class AnalyzerConfigSyntaxTreeOptionsProvider : SyntaxTreeOptionsProvider
{
    private const string DiagnosticPrefix = "dotnet_diagnostic.";
    private const string SeveritySuffix = ".severity";
    private const string GeneratedCodeKey = "generated_code";

    /// <summary>
    ///     Creates the provider, or returns <see langword="null" /> when no option affects the compiler, so the compilation keeps its own.
    /// </summary>
    public static AnalyzerConfigSyntaxTreeOptionsProvider? Create(
        ImmutableDictionary<string, ImmutableDictionary<string, string>> fileOptions,
        ImmutableDictionary<string, string> globalOptions
    )
    {
        var global = ReadSeverities(globalOptions);
        var perFile = fileOptions.ToImmutableDictionary(z => z.Key, z => ReadSeverities(z.Value));
        var generated = fileOptions
                       .Where(z => z.Value.ContainsKey(GeneratedCodeKey))
                       .ToImmutableDictionary(z => z.Key, z => ReadGeneratedKind(z.Value[GeneratedCodeKey]));

        return global.Count == 0 && perFile.Values.All(z => z.Count == 0) && generated.Count == 0
            ? null
            : new(global, perFile, generated);
    }

    private readonly ImmutableDictionary<string, ReportDiagnostic> _global;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, ReportDiagnostic>> _perFile;
    private readonly ImmutableDictionary<string, GeneratedKind> _generated;

    private AnalyzerConfigSyntaxTreeOptionsProvider(
        ImmutableDictionary<string, ReportDiagnostic> global,
        ImmutableDictionary<string, ImmutableDictionary<string, ReportDiagnostic>> perFile,
        ImmutableDictionary<string, GeneratedKind> generated
    )
    {
        _global = global;
        _perFile = perFile;
        _generated = generated;
    }

    public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken) =>
        _generated.TryGetValue(tree.FilePath, out var kind) ? kind : GeneratedKind.Unknown;

    public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        severity = default;
        return _perFile.TryGetValue(tree.FilePath, out var values) && values.TryGetValue(diagnosticId, out severity);
    }

    public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity) =>
        _global.TryGetValue(diagnosticId, out severity);

    private static ImmutableDictionary<string, ReportDiagnostic> ReadSeverities(ImmutableDictionary<string, string> options)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in options)
        {
            if (!option.Key.StartsWith(DiagnosticPrefix, StringComparison.OrdinalIgnoreCase)
             || !option.Key.EndsWith(SeveritySuffix, StringComparison.OrdinalIgnoreCase)
             || option.Key.Length <= DiagnosticPrefix.Length + SeveritySuffix.Length)
            {
                continue;
            }

            var id = option.Key[DiagnosticPrefix.Length..^SeveritySuffix.Length];
            if (ReadSeverity(option.Value) is { } severity)
            {
                builder[id] = severity;
            }
        }

        return builder.ToImmutable();
    }

    // The same values the compiler accepts in .editorconfig files.
    private static ReportDiagnostic? ReadSeverity(string value) => value.Trim().ToLowerInvariant() switch
    {
        "default" => ReportDiagnostic.Default,
        "error" => ReportDiagnostic.Error,
        "warning" => ReportDiagnostic.Warn,
        "suggestion" => ReportDiagnostic.Info,
        "silent" or "refactoring" => ReportDiagnostic.Hidden,
        "none" => ReportDiagnostic.Suppress,
        _ => null,
    };

    private static GeneratedKind ReadGeneratedKind(string value) => value.Trim().ToLowerInvariant() switch
    {
        "true" => GeneratedKind.MarkedGenerated,
        "false" => GeneratedKind.NotGenerated,
        _ => GeneratedKind.Unknown,
    };
}
