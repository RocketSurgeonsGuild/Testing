using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal sealed class OptionsProvider
(
    ImmutableDictionary<string, ImmutableDictionary<string, string>> options,
    ImmutableDictionary<string, string> globalOptions
) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new OptionsObject(globalOptions);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        if (!options.TryGetValue(tree.FilePath, out var value)) return GlobalOptions;
        foreach (var v in globalOptions.Where(v => !value.ContainsKey(v.Key)))
        {
            value = value.SetItem(v.Key, v.Value);
        }

        return new OptionsObject(value);
    }

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        if (!options.TryGetValue(textFile.Path, out var value)) return GlobalOptions;
        foreach (var v in globalOptions.Where(v => !value.ContainsKey(v.Key)))
        {
            value = value.SetItem(v.Key, v.Value);
        }

        return new OptionsObject(value);
    }

    private sealed class OptionsObject(ImmutableDictionary<string, string> properties) : AnalyzerConfigOptions
    {
        private readonly ImmutableDictionary<string, string> _properties = properties;

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _properties.TryGetValue(key, out value);
    }
}
