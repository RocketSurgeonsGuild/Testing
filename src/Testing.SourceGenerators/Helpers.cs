using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class Helpers
{
    public static MetadataReference CreateMetadataReference(this CSharpCompilation compilation)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream, options: new(outputNameOverride: compilation.AssemblyName));
        if (!emitResult.Success) throw new InvalidOperationException("Failed to compile");

        var data = stream.ToArray();

        using var assemblyStream = new MemoryStream(data);
        return MetadataReference.CreateFromStream(assemblyStream, MetadataReferenceProperties.Assembly);
    }

    internal static IEnumerable<Diagnostic> OrderDiagnosticResults(this IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics
              .OrderBy(static z => z.Id)
              .ThenBy(static z => z.Severity)
              .ThenBy(static z => z.Location.SourceSpan.Start)
              .ThenBy(static z => z.Location.SourceSpan.End);
    }
}