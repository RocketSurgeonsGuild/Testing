using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class Helpers
{
    internal static IEnumerable<Diagnostic> OrderDiagnosticResults(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
    {
        return diagnostics
              .Where(s => s.Severity >= severity)
              .OrderBy(static z => z.Location.GetMappedLineSpan().ToString())
              .ThenBy(static z => z.Severity)
              .ThenBy(static z => z.Id);
    }
}

