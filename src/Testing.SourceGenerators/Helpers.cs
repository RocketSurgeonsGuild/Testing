using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class Helpers
{
    internal static IEnumerable<Diagnostic> OrderDiagnosticResults(this IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics
              .OrderBy(static z => z.Location.GetMappedLineSpan().ToString())
              .ThenBy(static z => z.Severity)
              .ThenBy(static z => z.Id);
    }
}
