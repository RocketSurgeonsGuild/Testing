using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     An immutable builder for creating a <see cref="GeneratorTestContext" />
/// </summary>
public record GeneratorTestContextBuilder
{
    /// <summary>
    ///     Create a new instance of the <see cref="GeneratorTestContextBuilder" /> with the default references
    /// </summary>
    /// <remarks>This method adds the default references to the compilation (things that are needed for most code to compile)</remarks>
    /// <returns></returns>
    public static GeneratorTestContextBuilder Create()
    {
        return new GeneratorTestContextBuilder().WithDefaultReferences();
    }

    /// <summary>
    ///     Create a new instance of the <see cref="GeneratorTestContextBuilder" /> without the default references
    /// </summary>
    /// <returns></returns>
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

    private ImmutableHashSet<Type> _relatedTypes = ImmutableHashSet<Type>.Empty;
    private ImmutableArray<NamedSourceText> _sources = ImmutableArray<NamedSourceText>.Empty;
    private ImmutableArray<AdditionalText> _additionalTexts = ImmutableArray<AdditionalText>.Empty;
    private ImmutableHashSet<string> _ignoredFilePaths = ImmutableHashSet<string>.Empty;
    private ImmutableDictionary<string, MarkedLocation> _markedLocations = ImmutableDictionary<string, MarkedLocation>.Empty;

    private GeneratorTestContextBuilder() { }

    /// <summary>
    ///     Attach a logger to the builder
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithLogger(ILogger logger)
    {
        return this with { _logger = logger, };
    }

    /// <summary>
    ///     Set the project name for the compilation
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithProjectName(string projectName)
    {
        return this with { _projectName = projectName, };
    }

    /// <summary>
    ///     Set the assembly load context for the compilation
    /// </summary>
    /// <param name="assemblyLoadContext"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        return this with { _assemblyLoadContext = assemblyLoadContext, };
    }

    /// <summary>
    ///     Set the C# language version for the compilation
    /// </summary>
    /// <param name="languageVersion"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithLanguageVersion(LanguageVersion languageVersion)
    {
        return this with
        {
            _parseOptions = new(languageVersion, _parseOptions.DocumentationMode, _parseOptions.Kind, _parseOptions.PreprocessorSymbolNames.ToArray()),
        };
    }

    /// <summary>
    ///     Set the documentation mode for the compilation
    /// </summary>
    /// <param name="documentationMode"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithDocumentationMode(DocumentationMode documentationMode)
    {
        return this with
        {
            _parseOptions = new(_parseOptions.LanguageVersion, documentationMode, _parseOptions.Kind, _parseOptions.PreprocessorSymbolNames.ToArray()),
        };
    }

    /// <summary>
    ///     Add a preprocessor symbol to the compilation
    /// </summary>
    /// <param name="preprocessorSymbolNames"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     What kind of source code is being compiled
    /// </summary>
    /// <param name="sourceCodeKind"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Set features for the compilation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Ignore a given output file, if you don't want to see it's results.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder IgnoreOutputFile(string path)
    {
        return this with { _ignoredFilePaths = _ignoredFilePaths.Add(path), };
    }

    /// <summary>
    ///     Add a generator to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithGenerator(Type type)
    {
        return this with { _relatedTypes = _relatedTypes.Add(type), };
    }

    /// <summary>
    ///     Add a generator to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithGenerator<T>()
        where T : new()
    {
        return WithGenerator(typeof(T));
    }

    /// <summary>
    ///     Add a analyzer to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAnalyzer(Type type)
    {
        return this with { _relatedTypes = _relatedTypes.Add(type), };
    }

    /// <summary>
    ///     Add a analyzer to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAnalyzer<T>()
        where T : DiagnosticAnalyzer, new()
    {
        return WithAnalyzer(typeof(T));
    }

    /// <summary>
    ///     Add a codefix provider to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeFix(Type type)
    {
        return this with { _relatedTypes = _relatedTypes.Add(type), };
    }

    /// <summary>
    ///     Add a codefix provider to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeFix<T>()
        where T : CodeFixProvider, new()
    {
        return WithCodeFix(typeof(T));
    }

    /// <summary>
    ///     Add a code refactoring provider to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeRefactoring(Type type)
    {
        return this with { _relatedTypes = _relatedTypes.Add(type), };
    }

    /// <summary>
    ///     Add a code refactoring provider to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeRefactoring<T>()
        where T : CodeRefactoringProvider, new()
    {
        return WithCodeFix(typeof(T));
    }

    /// <summary>
    ///     Add an in memory compiled assembly to the compilation
    /// </summary>
    /// <param name="additionalCompilations"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddCompilationReferences(params GeneratorTestResults[] additionalCompilations)
    {
        return AddReferences(additionalCompilations.Select(z => z.MetadataReference!).ToArray());
    }

    /// <summary>
    ///     Add references to the given assembly names
    /// </summary>
    /// <param name="assemblyNames"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddReferences(params string[] assemblyNames)
    {
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return AddReferences(
            assemblyNames.Select(z => MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, z))).OfType<MetadataReference>().ToArray()
        );
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddReferences(params MetadataReference[] references)
    {
        var set = _metadataReferences.ToBuilder();
        foreach (var reference in references)
        {
            set.Add(reference);
        }

        return this with { _metadataReferences = set.ToImmutable(), };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime type to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddReferences(params Type[] references)
    {
        return AddReferences(references.Select(z => MetadataReference.CreateFromFile(z.Assembly.Location)).OfType<MetadataReference>().ToArray());
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime assembly to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddReferences(params Assembly[] references)
    {
        return AddReferences(references.Select(z => MetadataReference.CreateFromFile(z.Location)).OfType<MetadataReference>().ToArray());
    }

    /// <summary>
    ///     Set the default references for the compilation
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddSources(params SourceText[] additionalSources)
    {
        return this with { _sources = _sources.AddRange(additionalSources.Select(z => new NamedSourceText(z))), };
    }

    /// <summary>
    ///     Add the given source text to the compilation as a string
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddSources(params string[] additionalSources)
    {
        return this with
        {
            _sources = _sources.AddRange(additionalSources.Select(s => SourceText.From(s, Encoding.UTF8)).Select(z => new NamedSourceText(z))),
        };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddSource(string name, SourceText source)
    {
        return this with { _sources = _sources.Add(new(source) { Name = name, }), };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddSource(string name, string source)
    {
        return this with { _sources = _sources.Add(new(SourceText.From(source, Encoding.UTF8)) { Name = name, }), };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddMarkup(string name, CodeMarkup source)
    {
        return this with
        {
            _markedLocations = _markedLocations.Add(name, new(source.Location, source.Trigger)),
            _sources = _sources.Add(new(SourceText.From(source.Code, Encoding.UTF8)) { Name = name, }),
        };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddMarkup(string name, string source)
    {
        return AddMarkup(name, new CodeMarkup(source));
    }

    /// <summary>
    ///     Add the given additional text to the compilation
    /// </summary>
    /// <param name="additionalTexts"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddAdditionalTexts(params AdditionalText[] additionalTexts)
    {
        return this with { _additionalTexts = _additionalTexts.AddRange(additionalTexts), };
    }

    /// <summary>
    ///     Add the given additional text to the compilation for the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddAdditionalText(string path, SourceText source)
    {
        return this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, source)), };
    }

    /// <summary>
    ///     Add the given additional text to the compilation for the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddAdditionalText(string path, string source)
    {
        return this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, SourceText.From(source, Encoding.UTF8))), };
    }

    /// <summary>
    ///     Set the options for a given file to the compilation to be used by generators
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddOption(SyntaxTree tree, string key, string value)
    {
        return AddOption(tree.FilePath, key, value);
    }

    /// <summary>
    ///     Set the options for a given file to the compilation to be used by generators
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddOption(AdditionalText tree, string key, string value)
    {
        return AddOption(tree.Path, key, value);
    }

    /// <summary>
    ///     Set the options for a given file to the compilation to be used by generators
    /// </summary>
    /// <param name="path"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Set the global options for the compilation to be used by generators
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddGlobalOption(string key, string value)
    {
        return this with { _globalOptions = _globalOptions.Add(key, value), };
    }

    /// <summary>
    ///     Create the <see cref="GeneratorTestContext" /> from the builder
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContext Build()
    {
        return new(
            _projectName ?? "TestProject",
            _logger ?? NullLogger.Instance,
            _assemblyLoadContext ?? new CollectibleTestAssemblyLoadContext(),
            _metadataReferences,
            _relatedTypes,
            _sources,
            _ignoredFilePaths,
            _options,
            _globalOptions,
            _parseOptions,
            _additionalTexts,
            _markedLocations
        );
    }

    /// <summary>
    ///     Generate and return the results of the generators
    /// </summary>
    /// <returns></returns>
    public Task<GeneratorTestResults> GenerateAsync()
    {
        return Build().GenerateAsync();
    }
}