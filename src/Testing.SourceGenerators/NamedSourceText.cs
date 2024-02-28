using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal record NamedSourceText(SourceText SourceText)
{
    public string? Name { get; init; }
}