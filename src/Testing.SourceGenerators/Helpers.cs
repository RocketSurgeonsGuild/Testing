using System.Text;
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

    /// <summary>
    ///     Keeps the diagnostics at or above <paramref name="minimumSeverity" />; <see langword="null" /> keeps them all.
    /// </summary>
    internal static IEnumerable<Diagnostic> AtOrAbove(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity? minimumSeverity) => minimumSeverity is { } minimum ? diagnostics.Where(z => z.Severity >= minimum) : diagnostics;

    /// <summary>
    ///     Builds an exception message that names every failing diagnostic, grouped by where it came from.
    /// </summary>
    internal static string FormatFailure(string summary, IEnumerable<(string Source, IEnumerable<Diagnostic> Diagnostics)> groups)
    {
        var builder = new StringBuilder(summary);
        foreach ((var source, var diagnostics) in groups)
        {
            var list = diagnostics.OrderDiagnosticResults().ToList();
            if (list.Count == 0) continue;
            builder.AppendLine().Append(source).Append(':');
            foreach (var diagnostic in list)
            {
                builder.AppendLine().Append("    ").Append(diagnostic);
            }
        }

        return builder.ToString();
    }
}
