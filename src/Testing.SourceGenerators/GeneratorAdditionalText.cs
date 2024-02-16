using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal class GeneratorAdditionalText(string path, SourceText sourceText) : AdditionalText
{
    public override string Path { get; } = path;

    public override SourceText? GetText(CancellationToken cancellationToken = new())
    {
        return sourceText;
    }
}