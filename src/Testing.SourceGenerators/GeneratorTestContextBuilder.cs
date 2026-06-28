using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using Serilog.Core;

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
    public static GeneratorTestContextBuilder Create() => new GeneratorTestContextBuilder().WithDefaultReferences();

    /// <summary>
    ///     Create a new instance of the <see cref="GeneratorTestContextBuilder" /> without the default references
    /// </summary>
    /// <returns></returns>
    public static GeneratorTestContextBuilder CreateWithoutDefaultReferences() => new();

    private ImmutableDictionary<string, ImmutableDictionary<string, string>> _options = [];
    private ImmutableDictionary<string, string> _globalOptions = [];

    private ILogger? _logger;
    private string? _projectName;
    private AssemblyLoadContext? _assemblyLoadContext;
    private CSharpParseOptions _parseOptions = new();
    private DiagnosticSeverity? _diagnosticSeverity = VerifyGeneratorTextContext.diagnosticSeverityFilter;

    private ImmutableHashSet<MetadataReference>
        _metadataReferences = ImmutableHashSet<MetadataReference>.Empty.WithComparer(ReferenceEqualityComparer.Instance);

    private ImmutableHashSet<string> _referenceNames = ImmutableHashSet<string>.Empty.WithComparer(StringComparer.OrdinalIgnoreCase);

    private ImmutableHashSet<Type> _relatedTypes = [];
    private ImmutableArray<NamedSourceText> _sources = [];
    private ImmutableArray<AdditionalText> _additionalTexts = [];
    private ImmutableArray<GeneratorTestResultsCustomizer> _customizers = [];
    private ImmutableHashSet<string> _ignoredFilePaths = [];
    private ImmutableDictionary<string, MarkedLocation> _markedLocations = [];

    private GeneratorTestContextBuilder() { }

    /// <summary>
    ///     Attach a logger to the builder
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithLogger(ILogger logger) => this with { _logger = logger, };

    /// <summary>
    ///     Configure the diagnostic severity to be returned
    /// </summary>
    /// <param name="diagnosticSeverity"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithDiagnosticSeverity(DiagnosticSeverity? diagnosticSeverity) => this with { _diagnosticSeverity = diagnosticSeverity, };

    /// <summary>
    ///     Add a customizer to custom the data returned through verify
    /// </summary>
    /// <param name="customizer"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCustomizer(GeneratorTestResultsCustomizer customizer) => this with { _customizers = _customizers.Add(customizer), };

    /// <summary>
    ///     Set the project name for the compilation
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithProjectName(string projectName) => this with { _projectName = projectName, };

    /// <summary>
    ///     Set the assembly load context for the compilation
    /// </summary>
    /// <param name="assemblyLoadContext"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext) => this with { _assemblyLoadContext = assemblyLoadContext, };

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
    [OverloadResolutionPriority(-1)]
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
    ///     Add a preprocessor symbol to the compilation
    /// </summary>
    /// <param name="preprocessorSymbolNames"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddPreprocessorSymbol(params IEnumerable<string> preprocessorSymbolNames)
    {
        var preprocessorSymbolNameList = preprocessorSymbolNames.ToList();
        return this with
        {
            _parseOptions = new(
                _parseOptions.LanguageVersion,
                _parseOptions.DocumentationMode,
                _parseOptions.Kind,
                _parseOptions.PreprocessorSymbolNames.Union(preprocessorSymbolNameList).ToArray()
            ),
        };
    }

    /// <summary>
    ///     Add a preprocessor symbol to the compilation
    /// </summary>
    /// <param name="preprocessorSymbolNames"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddPreprocessorSymbol(params IReadOnlyCollection<string> preprocessorSymbolNames)
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
    public GeneratorTestContextBuilder IgnoreOutputFile(string path) => this with { _ignoredFilePaths = _ignoredFilePaths.Add(path), };

    /// <summary>
    ///     Add a generator to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithGenerator(Type type) => this with { _relatedTypes = _relatedTypes.Add(type), };

    /// <summary>
    ///     Add a generator to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithGenerator<T>()
        where T : new() => WithGenerator(typeof(T));

    /// <summary>
    ///     Add a analyzer to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAnalyzer(Type type) => this with { _relatedTypes = _relatedTypes.Add(type), };

    /// <summary>
    ///     Add a analyzer to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithAnalyzer<T>()
        where T : DiagnosticAnalyzer, new() => WithAnalyzer(typeof(T));

    /// <summary>
    ///     Add a codefix provider to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeFix(Type type) => this with { _relatedTypes = _relatedTypes.Add(type), };

    /// <summary>
    ///     Add a codefix provider to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeFix<T>()
        where T : CodeFixProvider, new() => WithCodeFix(typeof(T));

    /// <summary>
    ///     Add a code refactoring provider to the compilation
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeRefactoring(Type type) => this with { _relatedTypes = _relatedTypes.Add(type), };

    /// <summary>
    ///     Add a code refactoring provider to the compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GeneratorTestContextBuilder WithCodeRefactoring<T>()
        where T : CodeRefactoringProvider, new() => WithCodeFix(typeof(T));

    /// <summary>
    ///     Add an in memory compiled assembly to the compilation
    /// </summary>
    /// <param name="additionalCompilations"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddCompilationReferences(params GeneratorTestResults[] additionalCompilations)
    {
        return additionalCompilations.Any(z => z.MetadataReference is null)
            ? throw new ArgumentException("All additional compilations must have a metadata reference", nameof(additionalCompilations))
            : ( AddReferencesInternal(additionalCompilations.Select(z => z.MetadataReference!).ToArray()) with
            {
                _referenceNames = _referenceNames.Union(additionalCompilations.Select(z => z.Assembly!.GetName().Name)),
            } );
    }

    /// <summary>
    ///     Add an in memory compiled assembly to the compilation
    /// </summary>
    /// <param name="additionalCompilations"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddCompilationReferences(params IEnumerable<GeneratorTestResults> additionalCompilations)
    {
        var generatorTestResultsEnumerable = additionalCompilations.ToList();
        return generatorTestResultsEnumerable.Any(z => z.MetadataReference is null)
            ? throw new ArgumentException("All additional compilations must have a metadata reference", nameof(additionalCompilations))
            : AddReferencesInternal(generatorTestResultsEnumerable.Select(z => z.MetadataReference!).ToArray()) with
            {
                _referenceNames = _referenceNames.Union(generatorTestResultsEnumerable.Select(z => z.Assembly!.GetName().Name)),
            };
    }

    /// <summary>
    ///     Add an in memory compiled assembly to the compilation
    /// </summary>
    /// <param name="additionalCompilations"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddCompilationReferences(params IReadOnlyCollection<GeneratorTestResults> additionalCompilations)
    {
        return additionalCompilations.Any(z => z.MetadataReference is null)
            ? throw new ArgumentException("All additional compilations must have a metadata reference", nameof(additionalCompilations))
            : ( AddReferencesInternal(additionalCompilations.Select(z => z.MetadataReference!).ToArray()) with
            {
                _referenceNames = _referenceNames.Union(additionalCompilations.Select(z => z.Assembly!.GetName().Name)),
            } );
    }

    /// <summary>
    ///     Add references to the given assembly names
    /// </summary>
    /// <param name="assemblyNames"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddReferences(params string[] assemblyNames)
    {
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return AddReferencesInternal(
                assemblyNames.Select(z => MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, z))).OfType<MetadataReference>().ToArray()
            ) with
        {
            _referenceNames = _referenceNames.Union(assemblyNames),
        };
    }

    /// <summary>
    ///     Add references to the given assembly names
    /// </summary>
    /// <param name="assemblyNames"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddReferences(params IEnumerable<string> assemblyNames)
    {
        var assemblyNameList = assemblyNames.ToList();
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return AddReferencesInternal(
                assemblyNameList.Select(z => MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, z))).OfType<MetadataReference>().ToArray()
            ) with
        {
            _referenceNames = _referenceNames.Union(assemblyNameList),
        };
    }

    /// <summary>
    ///     Add references to the given assembly names
    /// </summary>
    /// <param name="assemblyNames"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddReferences(params IReadOnlyCollection<string> assemblyNames)
    {
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return AddReferencesInternal(
                assemblyNames.Select(z => MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, z))).OfType<MetadataReference>().ToArray()
            ) with
        {
            _referenceNames = _referenceNames.Union(assemblyNames),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddReferences(params MetadataReference[] references)
    {
        var names = _referenceNames.ToBuilder();
        foreach (var reference in references)
        {
            names.Add(Path.GetFileName(reference.Display ?? ""));
        }

        return AddReferencesInternal(references) with
        {
            _referenceNames = names.ToImmutable(),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddReferences(params IEnumerable<MetadataReference> references)
    {
        var referenceList = references.ToList();
        var names = _referenceNames.ToBuilder();
        foreach (var reference in referenceList)
        {
            names.Add(Path.GetFileName(reference.Display ?? ""));
        }

        return AddReferencesInternal(referenceList) with
        {
            _referenceNames = names.ToImmutable(),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddReferences(params IReadOnlyCollection<MetadataReference> references)
    {
        var names = _referenceNames.ToBuilder();
        foreach (var reference in references)
        {
            names.Add(Path.GetFileName(reference.Display ?? ""));
        }

        return AddReferencesInternal(references) with
        {
            _referenceNames = names.ToImmutable(),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime type to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddReferences(params Type[] references) => AddReferences(references.Select(z => z.Assembly).ToArray());

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime type to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddReferences(params IEnumerable<Type> references)
    {
        var referenceList = references.ToList();
        return AddReferences(referenceList.Select(z => z.Assembly).ToArray());
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime type to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddReferences(params IReadOnlyCollection<Type> references) => AddReferences(references.Select(z => z.Assembly).ToArray());

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime assembly to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddReferences(params Assembly[] references)
    {
        return AddReferencesInternal(references.Select(z => MetadataReference.CreateFromFile(z.Location)).OfType<MetadataReference>().ToArray()) with
        {
            _referenceNames = _referenceNames.Union(references.Select(z => z.GetName().Name ?? "")),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime assembly to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddReferences(params IEnumerable<Assembly> references)
    {
        var referenceList = references.ToList();
        return AddReferencesInternal(referenceList.Select(z => MetadataReference.CreateFromFile(z.Location)).OfType<MetadataReference>().ToArray()) with
        {
            _referenceNames = _referenceNames.Union(referenceList.Select(z => z.GetName().Name ?? "")),
        };
    }

    /// <summary>
    ///     Add a set of metadata references to the compilation using the runtime assembly to identify the assembly
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddReferences(params IReadOnlyCollection<Assembly> references)
    {
        return AddReferencesInternal(references.Select(z => MetadataReference.CreateFromFile(z.Location)).OfType<MetadataReference>().ToArray()) with
        {
            _referenceNames = _referenceNames.Union(references.Select(z => z.GetName().Name ?? "")),
        };
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
    ///     Add a set of metadata references to the compilation
    /// </summary>
    /// <param name="references"></param>
    /// <returns></returns>
    private GeneratorTestContextBuilder AddReferencesInternal(IEnumerable<MetadataReference> references)
    {
        var set = _metadataReferences.ToBuilder();
        foreach (var reference in references)
        {
            set.Add(reference);
        }

        return this with
        {
            _metadataReferences = set.ToImmutable(),
        };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddSources(params SourceText[] additionalSources) => this with { _sources = _sources.AddRange(additionalSources.Select(z => new NamedSourceText(z))), };

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddSources(params IEnumerable<SourceText> additionalSources)
    {
        var sourceList = additionalSources.ToList();
        return this with { _sources = _sources.AddRange(sourceList.Select(z => new NamedSourceText(z))), };
    }

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddSources(params IReadOnlyCollection<SourceText> additionalSources) => this with { _sources = _sources.AddRange(additionalSources.Select(z => new NamedSourceText(z))), };

    /// <summary>
    ///     Add the given source text to the compilation as a string
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddSources(params string[] additionalSources)
    {
        return this with
        {
            _sources = _sources.AddRange(additionalSources.Select(s => SourceText.From(s, Encoding.UTF8)).Select(z => new NamedSourceText(z))),
        };
    }

    /// <summary>
    ///     Add the given source text to the compilation as a string
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddSources(params IEnumerable<string> additionalSources)
    {
        var sourceList = additionalSources.ToList();
        return this with
        {
            _sources = _sources.AddRange(sourceList.Select(s => SourceText.From(s, Encoding.UTF8)).Select(z => new NamedSourceText(z))),
        };
    }

    /// <summary>
    ///     Add the given source text to the compilation as a string
    /// </summary>
    /// <param name="additionalSources"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddSources(params IReadOnlyCollection<string> additionalSources)
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
    public GeneratorTestContextBuilder AddSource(string name, SourceText source) => this with { _sources = _sources.Add(new(source) { Name = name, }), };

    /// <summary>
    ///     Add the given source text to the compilation
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddSource(string name, string source) => this with { _sources = _sources.Add(new(SourceText.From(source, Encoding.UTF8)) { Name = name, }), };

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
    public GeneratorTestContextBuilder AddMarkup(string name, string source) => AddMarkup(name, new CodeMarkup(source));

    /// <summary>
    ///     Add the given additional text to the compilation
    /// </summary>
    /// <param name="additionalTexts"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(-1)]
    public GeneratorTestContextBuilder AddAdditionalTexts(params AdditionalText[] additionalTexts) => this with { _additionalTexts = _additionalTexts.AddRange(additionalTexts), };

    /// <summary>
    ///     Add the given additional text to the compilation
    /// </summary>
    /// <param name="additionalTexts"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(0)]
    public GeneratorTestContextBuilder AddAdditionalTexts(params IEnumerable<AdditionalText> additionalTexts)
    {
        var additionalTextList = additionalTexts.ToList();
        return this with { _additionalTexts = _additionalTexts.AddRange(additionalTextList), };
    }

    /// <summary>
    ///     Add the given additional text to the compilation
    /// </summary>
    /// <param name="additionalTexts"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public GeneratorTestContextBuilder AddAdditionalTexts(params IReadOnlyCollection<AdditionalText> additionalTexts) => this with { _additionalTexts = _additionalTexts.AddRange(additionalTexts), };

    /// <summary>
    ///     Add the given additional text to the compilation for the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddAdditionalText(string path, SourceText source) => this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, source)), };

    /// <summary>
    ///     Add the given additional text to the compilation for the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddAdditionalText(string path, string source) => this with { _additionalTexts = _additionalTexts.Add(new GeneratorAdditionalText(path, SourceText.From(source, Encoding.UTF8))), };

    /// <summary>
    ///     Set the options for a given file to the compilation to be used by generators
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddOption(SyntaxTree tree, string key, string value) => AddOption(tree.FilePath, key, value);

    /// <summary>
    ///     Set the options for a given file to the compilation to be used by generators
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public GeneratorTestContextBuilder AddOption(AdditionalText tree, string key, string value) => AddOption(tree.Path, key, value);

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
        if (rootOptions.TryGetValue(path, out var fileOptions))
        {
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
    public GeneratorTestContextBuilder AddGlobalOption(string key, string value) => this with { _globalOptions = _globalOptions.Add(key, value), };

    /// <summary>
    ///     Create the <see cref="GeneratorTestContext" /> from the builder
    /// </summary>
    /// <returns></returns>
    public GeneratorTestContext Build()
    {
        return new(
            _projectName ?? "TestProject",
            _logger ?? Logger.None,
            _assemblyLoadContext ?? new CollectibleTestAssemblyLoadContext(),
            _metadataReferences,
            _referenceNames,
            _relatedTypes,
            _sources,
            _ignoredFilePaths,
            _options,
            _globalOptions,
            _parseOptions,
            _additionalTexts,
            _markedLocations,
            _diagnosticSeverity,
            _customizers
        );
    }

    /// <summary>
    ///     Generate and return the results of the generators
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<GeneratorTestResults> GenerateAsync(CancellationToken cancellationToken = default) => Build().GenerateAsync(cancellationToken);
}
