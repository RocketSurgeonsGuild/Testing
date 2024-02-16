using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public record GeneratorTestContextBuilder
{
    public static GeneratorTestContextBuilder Create()
    {
        return new GeneratorTestContextBuilder().WithDefaultReferences();
    }

    public static GeneratorTestContextBuilder CreateWithoutDefaultReferences()
    {
        return new();
    }

    private ImmutableDictionary<string, ImmutableDictionary<string, string>> _options = ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;
    private ImmutableDictionary<string, string> _globalOptions = ImmutableDictionary<string, string>.Empty;

    private ILogger? _logger;
    private string? _projectName;
    private AssemblyLoadContext? _assemblyLoadContext;
    private CSharpParseOptions _parseOptions = new();

    private ImmutableHashSet<MetadataReference>
        _metadataReferences = ImmutableHashSet<MetadataReference>.Empty.WithComparer(ReferenceEqualityComparer.Instance);

    private ImmutableHashSet<Type> _generators = ImmutableHashSet<Type>.Empty;
    private ImmutableArray<SourceText> _sources = ImmutableArray<SourceText>.Empty;
    private ImmutableArray<AdditionalText> _additionalTexts = ImmutableArray<AdditionalText>.Empty;
    private ImmutableHashSet<string> _ignoredFilePaths = ImmutableHashSet<string>.Empty;

    private GeneratorTestContextBuilder() { }

    public GeneratorTestContextBuilder WithLogger(ILogger logger)
    {
        return this with { _logger = logger, };
    }

    public GeneratorTestContextBuilder WithProjectName(string projectName)
    {
        return this with { _projectName = projectName, };
    }

    public GeneratorTestContextBuilder WithAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        return this with { _assemblyLoadContext = assemblyLoadContext, };
    }

    public GeneratorTestContextBuilder WithLanguageVersion(LanguageVersion languageVersion)
    {
        return this with
        {
            _parseOptions = new(languageVersion, _parseOptions.DocumentationMode, _parseOptions.Kind, _parseOptions.PreprocessorSymbolNames.ToArray()),
        };
    }

    public GeneratorTestContextBuilder WithDocumentationMode(DocumentationMode documentationMode)
    {
        return this with
        {
            _parseOptions = new(_parseOptions.LanguageVersion, documentationMode, _parseOptions.Kind, _parseOptions.PreprocessorSymbolNames.ToArray()),
        };
    }

    public GeneratorTestContextBuilder AddPreprocessorSymbol(params string[] preprocessorSymbolNames)
    {
        return this with
        {
            _parseOptions = new(
                _parseOptions.LanguageVersion,
                _parseOptions.DocumentationMode,
                _parseOptions.Kind,
                _parseOptions.PreprocessorSymbolNames.Union(preprocessorSymbolNames).ToArray()
            ),
        };
    }

    public GeneratorTestContextBuilder WithSourceCodeKind(SourceCodeKind sourceCodeKind)
    {
        return this with
        {
            _parseOptions = new(
                _parseOptions.LanguageVersion,
                _parseOptions.DocumentationMode,
                sourceCodeKind,
                _parseOptions.PreprocessorSymbolNames.ToArray()
            ),
        };
    }

    public GeneratorTestContextBuilder WithFeature(string key, string? value = null)
    {
        return this with
        {
            _parseOptions = new CSharpParseOptions(
                    _parseOptions.LanguageVersion,
                    _parseOptions.DocumentationMode,
                    _parseOptions.Kind,
                    _parseOptions.PreprocessorSymbolNames.ToArray()
                )
               .WithFeatures(_parseOptions.Features.Concat(new KeyValuePair<string, string>[] { new(key, value ?? string.Empty), })),
        };
    }

    public GeneratorTestContextBuilder IgnoreOutputFile(string path)
    {
        return this with { _ignoredFilePaths = _ignoredFilePaths.Add(path), };
    }

    public GeneratorTestContextBuilder WithGenerator(Type type)
    {
        return this with { _generators = _generators.Add(type), };
    }

    public GeneratorTestContextBuilder WithGenerator<T>()
        where T : new()
    {
        return WithGenerator(typeof(T));
    }

    public GeneratorTestContextBuilder AddCompilationReferences(params CSharpCompilation[] additionalCompilations)
    {
        return AddReferences(additionalCompilations.Select(z => z.CreateMetadataReference()).ToArray());
    }

    public GeneratorTestContextBuilder AddReferences(params string[] assemblyNames)
    {
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return AddReferences(
            assemblyNames.Select(z => MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, z))).OfType<MetadataReference>().ToArray()
        );
    }

    public GeneratorTestContextBuilder AddReferences(params MetadataReference[] references)
    {
        var set = _metadataReferences.ToBuilder();
        foreach (var reference in references)
        {
            set.Add(reference);
        }

        return this with { _metadataReferences = set.ToImmutable(), };
    }

    public GeneratorTestContextBuilder AddReferences(params Type[] references)
    {
        return AddReferences(references.Select(z => MetadataReference.CreateFromFile(z.Assembly.Location)).OfType<MetadataReference>().ToArray());
    }

    public GeneratorTestContextBuilder AddReferences(params Assembly[] references)
    {
        return AddReferences(references.Select(z => MetadataReference.CreateFromFile(z.Location)).OfType<MetadataReference>().ToArray());
    }

    public GeneratorTestContextBuilder WithDefaultReferences()
    {
        return AddReferences(
            "mscorlib.dll",
            "netstandard.dll",
            "System.dll",
            "System.Core.dll",
            #if NETCOREAPP
            "System.Private.CoreLib.dll",
            #endif
            "System.Runtime.dll"
        );
    }

    public GeneratorTestContextBuilder AddSources(params SourceText[] additionalSources)
    {
        return this with { _sources = _sources.AddRange(additionalSources), };
    }

    public GeneratorTestContextBuilder AddSources(params string[] additionalSources)
    {
        return this with { _sources = _sources.AddRange(additionalSources.Select(s => SourceText.From(s, Encoding.UTF8))), };
    }

    public GeneratorTestContextBuilder AddAdditionalTexts(params AdditionalText[] additionalTexts)
    {
        return this with { _additionalTexts = _additionalTexts.AddRange(additionalTexts), };
    }

    public GeneratorTestContextBuilder AddAdditionalText(string path, SourceText source)
    {
        return this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, source)), };
    }

    public GeneratorTestContextBuilder AddAdditionalText(string path, string source)
    {
        return this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, SourceText.From(source, Encoding.UTF8))), };
    }

    public GeneratorTestContextBuilder AddOption(SyntaxTree tree, string key, string value)
    {
        return AddOption(tree.FilePath, key, value);
    }

    public GeneratorTestContextBuilder AddOption(AdditionalText tree, string key, string value)
    {
        return AddOption(tree.Path, key, value);
    }

    public GeneratorTestContextBuilder AddOption(string path, string key, string value)
    {
        var rootOptions = _options;
        if (rootOptions.ContainsKey(path))
        {
            var fileOptions = rootOptions[path];
            fileOptions = fileOptions.Add(key, value);
            rootOptions = rootOptions.SetItem(path, fileOptions);
        }
        else
        {
            rootOptions = rootOptions.SetItem(path, ImmutableDictionary<string, string>.Empty.Add(key, value));
        }

        return this with { _options = rootOptions, };
    }

    public GeneratorTestContextBuilder AddGlobalOption(string key, string value)
    {
        return this with { _globalOptions = _globalOptions.Add(key, value), };
    }

    public GeneratorTestContext Build()
    {
        return new(
            _projectName ?? "TestProject",
            _logger ?? NullLogger.Instance,
            _assemblyLoadContext ?? new CollectibleTestAssemblyLoadContext(),
            _metadataReferences,
            _generators,
            _sources,
            _ignoredFilePaths,
            _options,
            _globalOptions,
            _parseOptions,
            _additionalTexts
        );
    }
}