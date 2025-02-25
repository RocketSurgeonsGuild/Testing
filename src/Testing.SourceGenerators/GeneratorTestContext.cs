using System.Collections.Immutable;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Serilog;
using Serilog.Events;
// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The context for running a set of generators against a set of sources
/// </summary>
[PublicAPI]
public record GeneratorTestContext
{
    private readonly ImmutableDictionary<string, MarkedLocation> _markedLocations;
    private readonly ImmutableArray<GeneratorTestResultsCustomizer> _customizers;

    internal GeneratorTestContext(
        string projectName,
        ILogger logger,
        AssemblyLoadContext assemblyLoadContext,
        ImmutableHashSet<MetadataReference> metadataReferences,
        ImmutableHashSet<string> referenceNames,
        ImmutableHashSet<Type> relatedTypes,
        ImmutableArray<NamedSourceText> sources,
        ImmutableHashSet<string> ignoredFilePaths,
        ImmutableDictionary<string, ImmutableDictionary<string, string>> fileOptions,
        ImmutableDictionary<string, string> globalOptions,
        CSharpParseOptions parseOptions,
        ImmutableArray<AdditionalText> additionalTexts,
        ImmutableDictionary<string, MarkedLocation> markedLocations,
        DiagnosticSeverity? diagnosticSeverity,
        ImmutableArray<GeneratorTestResultsCustomizer> customizers
    )
    {
        _markedLocations = markedLocations;
        _customizers = customizers;
        _logger = logger;
        _metadataReferences = metadataReferences;
        _relatedTypes = relatedTypes;
        _sources = sources;
        _ignoredFilePaths = ignoredFilePaths;
        _fileOptions = fileOptions;
        _globalOptions = globalOptions;
        _parseOptions = parseOptions;
        _additionalTexts = additionalTexts;
        _projectName = projectName;
        _diagnosticSeverity = diagnosticSeverity;
        AssemblyLoadContext = assemblyLoadContext;

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hasher.AppendData(Encoding.UTF8.GetBytes(projectName));
        foreach (var reference in referenceNames.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference));
        }

        foreach (var reference in relatedTypes.OrderBy(z => z.FullName))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.FullName ?? ""));
        }

        foreach (var reference in sources.OrderBy(z => z.Name))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.Name ?? ""));
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            reference.SourceText.Write(writer);
            hasher.AppendData(Encoding.UTF8.GetBytes(builder.Replace("\r\n", "\n").ToString()));
        }

        foreach (var reference in ignoredFilePaths.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference));
        }

        foreach (var reference in fileOptions.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.Key));
            foreach (var item in reference.Value.OrderBy(z => z.Key))
            {
                hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
                hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
            }
        }

        foreach (var item in globalOptions.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
        }

        hasher.AppendData(Encoding.UTF8.GetBytes(parseOptions.Language));
        hasher.AppendData(Encoding.UTF8.GetBytes(parseOptions.LanguageVersion.ToString()));
        foreach (var item in parseOptions.Features.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
        }

        foreach (var item in parseOptions.PreprocessorSymbolNames.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item));
        }

        foreach (var item in additionalTexts.OrderBy(z => z.Path))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Path));
            var text = item.GetText();
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            text?.Write(writer);
            hasher.AppendData(Encoding.UTF8.GetBytes(builder.Replace("\r\n", "\n").ToString()));
        }

        foreach (var item in markedLocations.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value.Location.ToString()));
            if (item.Value.Trigger is { } trigger)
            {
                hasher.AppendData(Encoding.UTF8.GetBytes(trigger.ToString() ?? ""));
            }
        }

        Id = Convert.ToBase64String(hasher.GetCurrentHash());
    }

    /// <summary>
    ///     The related assembly load context
    /// </summary>
    public AssemblyLoadContext AssemblyLoadContext { get; }

    /// <summary>
    ///     Get the id of the context
    /// </summary>
    public string Id { get; }

    internal ILogger _logger { get; init; }
    internal ImmutableHashSet<MetadataReference> _metadataReferences { get; init; }
    internal ImmutableHashSet<Type> _relatedTypes { get; init; }
    internal ImmutableArray<NamedSourceText> _sources { get; init; }
    internal ImmutableHashSet<string> _ignoredFilePaths { get; init; }
    internal ImmutableDictionary<string, ImmutableDictionary<string, string>> _fileOptions { get; init; }
    internal ImmutableDictionary<string, string> _globalOptions { get; init; }
    internal string _projectName { get; init; }
    internal CSharpParseOptions _parseOptions { get; init; }
    internal ImmutableArray<AdditionalText> _additionalTexts { get; init; }
    internal DiagnosticSeverity? _diagnosticSeverity { get; init; }

    /// <summary>
    ///     Generate and return the results of the generators
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<GeneratorTestResults> GenerateAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting Generation for {SourceCount}", _sources.Length);
        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("--- References --- {Count}", _metadataReferences.Count);
            foreach (var reference in _metadataReferences)
            {
                _logger.Verbose("    Reference: {Name}", reference.Display);
            }
        }

        var project = GenerationHelpers.CreateProject(_projectName, _metadataReferences, _parseOptions, _sources, _additionalTexts);

        var compilation = (CSharpCompilation?)await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation is null) throw new InvalidOperationException("Could not compile the sources");

        var diagnostics = compilation.GetDiagnostics();
        if (_logger.IsEnabled(LogEventLevel.Verbose) && diagnostics is { Length: > 0, })
        {
            _logger.Verbose("--- Input Diagnostics --- {Count}", diagnostics.Length);
            foreach (var d in diagnostics)
            {
                _logger.Verbose("    Reference: {Name}", d.ToString());
            }
        }

        var builder = ImmutableDictionary<Type, GeneratorTestResult>.Empty.ToBuilder();
        var analyzerBuilder = ImmutableDictionary<Type, AnalyzerTestResult>.Empty.ToBuilder();

        var inputCompilation = compilation;
        var inputDiagnostics = diagnostics;

        TestProjectInformation projectInformation;

        {
            var relatedInstances = _relatedTypes
                                  .Select(Activator.CreateInstance)
                                  .Where(z => z != null)
                                  .Select(z => z!)
                                  .ToImmutableArray()
                ;
            projectInformation = new(
                _logger,
                project,
                relatedInstances.OfType<DiagnosticAnalyzer>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<ISourceGenerator>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<IIncrementalGenerator>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<CodeFixProvider>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<CodeRefactoringProvider>().ToImmutableDictionary(z => z.GetType())
            );
        }

        var finalDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var instance in projectInformation.IncrementalGenerators.Values.Concat(projectInformation.SourceGenerators.Values.Cast<object>()))
        {
            _logger.Information("--- {Generator} ---", instance.GetType().FullName);
            var generators = new List<ISourceGenerator>
            {
                instance switch { IIncrementalGenerator i => i.AsSourceGenerator(), ISourceGenerator s => s, _ => throw new ArgumentOutOfRangeException(), },
            };
            var driver = CSharpGeneratorDriver.Create(generators, _additionalTexts, _parseOptions, new OptionsProvider(_fileOptions, _globalOptions), default);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);

            if (_logger.IsEnabled(LogEventLevel.Verbose) && diagnostics is { Length: > 0, })
            {
                _logger.Verbose("--- Diagnostics --- {Count}", diagnostics.Length);
                foreach (var d in diagnostics)
                {
                    _logger.Verbose("    Reference: {Name}", d.ToString());
                }
            }

            var trees = outputCompilation
                       .SyntaxTrees
                       .Except(compilation.SyntaxTrees)
                       .ToImmutableArray();
            if (_logger.IsEnabled(LogEventLevel.Verbose) && trees is { Length: > 0, })
            {
                _logger.Verbose("--- Syntax Trees --- {Count}", trees.Length);
                foreach (var t in trees)
                {
                    _logger.Verbose("    FilePath: {Name}", t.FilePath);
                    _logger.Verbose("    Source:\n{Name}", ( await t.GetTextAsync(cancellationToken).ConfigureAwait(false) ).ToString());
                }
            }

            inputCompilation = inputCompilation.AddSyntaxTrees(trees);
            finalDiagnostics.AddRange(diagnostics);

            builder.Add(
                instance.GetType(),
                new(
                    ( outputCompilation as CSharpCompilation )!,
                    diagnostics,
                    trees.Where(z => !_ignoredFilePaths.Any(x => z.FilePath.Contains(x))).ToImmutableArray()
                )
            );
        }

        var analyzers = projectInformation.Analyzers;
        finalDiagnostics.AddRange(inputCompilation.GetDiagnostics());

        AnalysisResult? analysisResult = null;
        if (analyzers.Count > 0)
        {
            _logger.Information("--- Analyzers ---");
            foreach (var analyzer in analyzers)
            {
                _logger.Information("    {Analyzer}", analyzer.Key.FullName);
            }

            var compilationWithAnalyzers = inputCompilation.WithAnalyzers(
                analyzers.Values.ToImmutableArray(),
                new AnalyzerOptions(_additionalTexts, new OptionsProvider(_fileOptions, _globalOptions))
            );

            analysisResult = await compilationWithAnalyzers.GetAnalysisResultAsync(cancellationToken);
            foreach (var analyzer in analyzers)
            {
                var analyzerResults = analysisResult.GetAllDiagnostics(analyzer.Value);
                analyzerBuilder.Add(analyzer.Key, new(analyzerResults));
            }

            finalDiagnostics.AddRange(analysisResult.GetAllDiagnostics());
        }

        var results = new GeneratorTestResults(
            Id,
            projectInformation,
            compilation,
            inputDiagnostics,
            compilation.SyntaxTrees,
            _additionalTexts,
            _customizers,
            _diagnosticSeverity,
            _parseOptions,
            _globalOptions,
            _fileOptions,
            builder.ToImmutable(),
            analyzerBuilder.ToImmutable(),
            ImmutableDictionary<Type, CodeFixTestResult>.Empty,
            ImmutableDictionary<Type, CodeRefactoringTestResult>.Empty,
            _markedLocations,
            inputCompilation,
            finalDiagnostics.ToImmutable(),
            null!,
            null!
        );

        if (projectInformation.CodeFixProviders.Count > 0)
        {
            _logger.Information("--- Code Fix Providers ---");
            foreach (var provider in projectInformation.CodeFixProviders.Values)
            {
                results = await results.AddCodeFix(provider, cancellationToken);
            }
        }

        if (projectInformation.CodeRefactoringProviders.Count > 0)
        {
            _logger.Information("--- Code Refactoring Providers ---");
            foreach (var provider in projectInformation.CodeRefactoringProviders.Values)
            {
                results = await results.AddCodeRefactoring(provider, cancellationToken);
            }
        }

        var assemblyStream = Emit(inputCompilation);
        if (assemblyStream is { Length: > 0, })
            results = results with
            {
                MetadataReference = MetadataReference.CreateFromStream(new MemoryStream(assemblyStream)),
                Assembly = AssemblyLoadContext.LoadFromStream(new MemoryStream(assemblyStream)),
            };

        return results;
    }

    private byte[] Emit(CSharpCompilation compilation, string? outputName = null)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(
            stream,
            options: new(outputNameOverride: outputName ?? compilation.AssemblyName)
        );
        if (!emitResult.Success) return Array.Empty<byte>();

        return stream.ToArray();
    }
}
